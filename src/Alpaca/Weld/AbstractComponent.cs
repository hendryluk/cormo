using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Injects
{
    public interface IComponent
    {
        IComponentManager Manager { get; }
        Type Type { get; }
        IEnumerable<QualifierAttribute> Qualifiers { get; }
        IEnumerable<IInjectionPoint> InjectionPoints { get; }
        ScopeAttribute Scope { get; }
    }
}

namespace Alpaca.Weld
{
    public static class ComponentIdentifiers
    {
        private const string COMPONENT_ID_SEPARATOR = "%";
        private static StringBuilder GetPrefix(Type type)
        {
            return new StringBuilder(type.AssemblyQualifiedName).Append(COMPONENT_ID_SEPARATOR);
        }
    }

    public interface IWeldComponent : IComponent
    {
        IWeldComponent Resolve(Type type);
        bool CanSatisfy(IEnumerable<QualifierAttribute> qualifiers);

        bool IsProxyRequired { get; }
        bool IsConcrete { get; }
        object Build();
    }

    public abstract class AbstractComponent : IWeldComponent
    {
        protected AbstractComponent(Type type, IEnumerable<QualifierAttribute> qualifiers, ScopeAttribute scope, IComponentManager manager)
        {
            Type = type;
            Manager = manager;
            Qualifiers = qualifiers;
            Scope = scope;
            _lazyBuildPlan = new Lazy<BuildPlan>(GetBuildPlan);
        }

        public IEnumerable<QualifierAttribute> Qualifiers { get; set; }
        public ScopeAttribute Scope { get; private set; }
        public Type Type { get; set; }
        public IComponentManager Manager { get; set; }
        public abstract bool IsConcrete { get; }
        public IEnumerable<IInjectionPoint> InjectionPoints
        {
            get { return _injectionPoints; }
        }

        public void AddInjectionPoints(params IWeldInjetionPoint[] injectionPoints)
        {
            foreach(var inject in injectionPoints)
                _injectionPoints.Add(inject);
        }

        private readonly Lazy<BuildPlan> _lazyBuildPlan;
        private readonly ISet<IWeldInjetionPoint> _injectionPoints = new HashSet<IWeldInjetionPoint>();

        //public virtual IWeldComponent Resolve(Type type, IEnumerable<QualifierAttribute> qualifiers)
        //{
        //    if (!CanSatisfy(Qualifiers))
        //        return null;
        //    return CanSatisfy(type);
        //}

        public bool IsProxyRequired { get; private set; }

        public bool CanSatisfy(IEnumerable<QualifierAttribute> qualifiers)
        {
            return qualifiers.All(Qualifiers.Contains);
        }

        public abstract IWeldComponent Resolve(Type requestedType);

        public object Build()
        {
            return _lazyBuildPlan.Value();
        }

        protected void TransferInjectionPointsTo(AbstractComponent component, GenericUtils.Resolution resolution)
        {
            component.AddInjectionPoints(_injectionPoints.Select(x =>
            {
                if (x.DeclaringComponent.Type == component.Type)
                    return x;
                return x.TranslateGenericArguments(component, resolution.GenericParameterTranslations);
            }).ToArray());
        }

        protected abstract BuildPlan GetBuildPlan();
    }

    public class InstanceComponent : AbstractComponent
    {
        private readonly object _instance;

        public InstanceComponent(object instance, Type type, IEnumerable<QualifierAttribute> qualifiers, ScopeAttribute scope, IComponentManager manager)
            : base(type, qualifiers, scope, manager)
        {
            _instance = instance;
        }

        public override bool IsConcrete
        {
            get { return true; }
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            if (requestedType.IsInstanceOfType(_instance))
                return this;
            return null;
        }

        protected override BuildPlan GetBuildPlan()
        {
            return () => _instance;
        }
    }

    public class ClassComponent : AbstractComponent
    {
        private readonly IEnumerable<MethodInfo> _postConstructs;
        private readonly IEnumerable<MethodInfo> _preDestroys;
        private readonly bool _containsGenericParameters;
        
        public ClassComponent(Type type, IEnumerable<QualifierAttribute> qualifiers, ScopeAttribute scope,  IComponentManager manager, MethodInfo[] postConstructs, MethodInfo[] preDestroys)
            : base(type, qualifiers, scope, manager)
        {
            _postConstructs = postConstructs;
            _preDestroys = preDestroys;
            _containsGenericParameters = Type.ContainsGenericParameters;

            ValidateMethodSignatures();
        }

        private void ValidateMethodSignatures()
        {
            foreach (var m in _postConstructs)
            {
                PostConstructCriteria.Validate(m);
            }
            foreach (var m in _preDestroys)
            {
                PreDestroyCriteria.Validate(m);
            }
        }

        public override bool IsConcrete
        {
            get { return !_containsGenericParameters; }
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            if (!_containsGenericParameters)
                return requestedType.IsAssignableFrom(Type)? this: null;

            var resolution = GenericUtils.ResolveGenericType(Type, requestedType);
            if (resolution == null || resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
                return null;

            var postConstructs = _postConstructs.Select(x=> GenericUtils.TranslateMethodGenericArguments(x, resolution.GenericParameterTranslations)).ToArray();
            var preDestroys = _preDestroys.Select(x => GenericUtils.TranslateMethodGenericArguments(x, resolution.GenericParameterTranslations)).ToArray();

            var components = new ClassComponent(resolution.ResolvedType, Qualifiers, Scope, Manager, postConstructs, preDestroys);
            TransferInjectionPointsTo(components, resolution);
            return components;
        }

        protected override BuildPlan GetBuildPlan()
        {
            var methodInjects = InjectionPoints.OfType<MethodParameterInjectionPoint>().ToArray();
            var ctorInject = InjectMethods(methodInjects.Where(x => x.IsConstructor)).FirstOrDefault();
            var setterInjects = InjectMethods(methodInjects.Where(x=> !x.IsConstructor)).ToArray();
            var otherInjects = InjectionPoints.Except(methodInjects).Cast<IWeldInjetionPoint>();

            var create = ctorInject == null? 
                    new BuildPlan(() => Activator.CreateInstance(Type, true)): 
                    () => ctorInject(null);

            return () =>
            {
                var instance = create();
                foreach (var i in otherInjects)
                    i.Inject(instance);
                foreach (var i in setterInjects)
                    i(instance);
                foreach (var post in _postConstructs)
                    post.Invoke(instance, new object[0]);

                return instance;
            };
        }

        private IEnumerable<InjectPlan> InjectMethods(IEnumerable<MethodParameterInjectionPoint> injects)
        {
            return from g in injects.GroupBy(x => x.Member) 
                   let method = (MethodBase)g.Key 
                   let paramInjects = g.OrderBy(x => x.Position).ToArray() 
                   select (InjectPlan) (x =>
                    {
                        var paramVals = paramInjects.Select(p => p.GetValue()).ToArray();
                        return method.Invoke(x, paramVals);
                    });
        }
    }

    public class ProducerMethod : AbstractComponent
    {
        private readonly bool _containsGenericParameters;
        private readonly MethodInfo _method;

        public ProducerMethod(MethodInfo method, IEnumerable<QualifierAttribute> qualifiers, ScopeAttribute scope, IComponentManager manager)
            : base(method.ReturnType, qualifiers, scope, manager)
        {
            _method = method;
            _containsGenericParameters = GenericUtils.MemberContainsGenericArguments(method);
        }

        public override bool IsConcrete
        {
            get { return !_containsGenericParameters; }
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            if (!_containsGenericParameters)
                return requestedType.IsAssignableFrom(Type)? this: null;

            var typeResolution = GenericUtils.ResolveGenericType(Type, requestedType);
            if (typeResolution == null || typeResolution.ResolvedType == null || typeResolution.ResolvedType.ContainsGenericParameters)
                return null;

            var resolvedProducer = GenericUtils.TranslateMethodGenericArguments(_method, typeResolution.GenericParameterTranslations);
            if (GenericUtils.MemberContainsGenericArguments(resolvedProducer))
                return null;

            var method = resolvedProducer;
            if (method != null)
            {
                var component = new ProducerMethod(method, Qualifiers, Scope, Manager);
                TransferInjectionPointsTo(component, typeResolution);
            }

            return null;
        }

        protected override BuildPlan GetBuildPlan()
        {
            // TODO
            return null;
            //return engine.MakeExecutionPlan(Producer);
        }
    }
}