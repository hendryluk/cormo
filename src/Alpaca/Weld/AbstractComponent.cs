using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Injects
{
    public interface IComponent
    {
        IComponentManager Manager { get; }
        Type Type { get; }
        IEnumerable<Attribute> Qualifiers { get; }
        IEnumerable<IInjectionPoint> InjectionPoints { get; }
    }
}

namespace Alpaca.Weld
{
    public interface IWeldComponent : IComponent
    {
        IWeldComponent CanSatisfy(SeekSpec spec);
    }

    public abstract class AbstractComponent : IWeldComponent
    {
        protected AbstractComponent(Type type, IEnumerable<Attribute> qualifiers, IComponentManager manager)
        {
            Type = type;
            Manager = manager;
            Qualifiers = qualifiers;
            _lazyBuildPlan = new Lazy<BuildPlan>(GetBuildPlan);
        }

        public IEnumerable<Attribute> Qualifiers { get; set; }
        public Type Type { get; set; }
        public IComponentManager Manager { get; set; }
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

        public virtual IWeldComponent CanSatisfy(SeekSpec spec)
        {
            if (!CanSatisfy(spec.Qualifiers))
                return null;
            return CanSatisfy(spec.Type);
        }

        private bool CanSatisfy(IEnumerable<object> qualifiers)
        {
            return qualifiers.All(Qualifiers.Contains);
        }

        public abstract IWeldComponent CanSatisfy(Type requestedType);

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

        public InstanceComponent(object instance, Type type, ISet<Attribute> qualifiers, IComponentManager manager)
            : base(type, qualifiers, manager)
        {
            _instance = instance;
        }

        public override IWeldComponent CanSatisfy(Type requestedType)
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
        
        public ClassComponent(Type type, IEnumerable<Attribute> qualifiers, IComponentManager manager, MethodInfo[] postConstructs, MethodInfo[] preDestroys)
            : base(type, qualifiers, manager)
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

        public override IWeldComponent CanSatisfy(Type requestedType)
        {
            if (!_containsGenericParameters)
                return this;

            var resolution = GenericUtils.ResolveGenericType(Type, requestedType);
            if (resolution == null || resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
                return null;

            var postConstructs = _postConstructs.Select(x=> GenericUtils.TranslateMethodGenericArguments(x, resolution.GenericParameterTranslations)).ToArray();
            var preDestroys = _preDestroys.Select(x => GenericUtils.TranslateMethodGenericArguments(x, resolution.GenericParameterTranslations)).ToArray();

            var components = new ClassComponent(resolution.ResolvedType, Qualifiers, Manager, postConstructs, preDestroys);
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

        public ProducerMethod(MethodInfo method, IEnumerable<Attribute> qualifiers, IComponentManager manager)
            : base(method.ReturnType, qualifiers, manager)
        {
            _method = method;
            _containsGenericParameters = GenericUtils.MemberContainsGenericArguments(method);
        }

        public override IWeldComponent CanSatisfy(Type requestedType)
        {
            if (!_containsGenericParameters)
                return this;

            var typeResolution = GenericUtils.ResolveGenericType(Type, requestedType);
            if (typeResolution == null || typeResolution.ResolvedType == null || typeResolution.ResolvedType.ContainsGenericParameters)
                return null;

            var resolvedProducer = GenericUtils.TranslateMethodGenericArguments(_method, typeResolution.GenericParameterTranslations);
            if (GenericUtils.MemberContainsGenericArguments(resolvedProducer))
                return null;

            var method = resolvedProducer;
            if (method != null)
            {
                var component = new ProducerMethod(method, Qualifiers, Manager);
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