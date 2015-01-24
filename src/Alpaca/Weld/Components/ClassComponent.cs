using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Weld.Injections;
using Alpaca.Weld.Utils;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;

namespace Alpaca.Weld.Components
{
    public abstract class ManagedComponent : AbstractComponent
    {
        protected ManagedComponent(ComponentIdentifier id, Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, IComponentManager manager)
            : base(id, type, qualifiers, scope, manager)
        {
        }

        protected ManagedComponent(Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, IComponentManager manager) 
            : base(type.FullName, type, qualifiers, scope, manager)
        {
        }
    }

    public class ClassComponent : ManagedComponent
    {
        public IEnumerable<MethodInfo> PostConstructs { get; private set; }
        private readonly bool _containsGenericParameters;

        private ClassComponent(ClassComponent parent, Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, IComponentManager manager, GenericUtils.Resolution typeResolution)
            : base(new ComponentIdentifier(parent.Id.Key, type), type, qualifiers, scope, manager)
        {
            var postConstructs = parent.PostConstructs.Select(x => GenericUtils.TranslateMethodGenericArguments(x, typeResolution.GenericParameterTranslations)).ToArray();
            PostConstructs = postConstructs;
            _containsGenericParameters = Type.ContainsGenericParameters;
            IsDisposable = typeof(IDisposable).IsAssignableFrom(Type);

            Mixins = parent.Mixins;
            TransferInjectionPointsTo(this, typeResolution);

            ValidateMethodSignatures();
        }

        public ClassComponent(Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope,  IComponentManager manager, MethodInfo[] postConstructs)
            : base(type, qualifiers, scope, manager)
        {
            PostConstructs = postConstructs;
            _containsGenericParameters = Type.ContainsGenericParameters;
            IsDisposable = typeof (IDisposable).IsAssignableFrom(Type);

            ValidateMethodSignatures();
        }

        private void ValidateMethodSignatures()
        {
            foreach (var m in PostConstructs)
            {
                PostConstructCriteria.Validate(m);
            }
        }

        public override bool IsConcrete
        {
            get { return !_containsGenericParameters; }
        }

        public Type[] Mixins { get; set; }
        public bool IsDisposable { get; private set; }

        public override IWeldComponent Resolve(Type requestedType)
        {
            if (!_containsGenericParameters)
                return requestedType.IsAssignableFrom(Type)? this: null;

            var resolution = GenericUtils.ResolveGenericType(Type, requestedType);
            if (resolution == null || resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
                return null;

            
            var component = new ClassComponent(this, 
                resolution.ResolvedType, 
                Qualifiers, 
                Scope, Manager, resolution);

            return component;
        }

        protected override BuildPlan GetBuildPlan()
        {
            var paramInject = InjectionPoints.OfType<MethodParameterInjectionPoint>().ToArray();
            var ctorInject = InjectConstructor(paramInject.Where(x => x.IsConstructor));
            var methodInject = InjectMethods(paramInject.Where(x=> !x.IsConstructor)).ToArray();
            var otherInjects = InjectionPoints.Except(paramInject).Cast<IWeldInjetionPoint>();

            //var create = ctorInject == null? 
            //    new BuildPlan(context => Activator.CreateInstance(Type, true)): 
            //    context => ctorInject(null, context);

            return context =>
            {
                var instance = ctorInject(context);
                foreach (var i in otherInjects)
                    i.Inject(instance, context);
                foreach (var i in methodInject)
                    i(instance, context);
                foreach (var post in PostConstructs)
                    post.Invoke(instance, new object[0]);

                return instance;
            };
        }

        public class AlpacaNamingScope : INamingScope
        {
            private readonly INamingScope _delegate;

            public AlpacaNamingScope()
            {
                _delegate = new NamingScope();
            }
            private AlpacaNamingScope(AlpacaNamingScope alpacaNamingScope)
            {
                ParentScope = alpacaNamingScope;
                _delegate = alpacaNamingScope._delegate.SafeSubScope();
            }

            public string GetUniqueName(string suggestedName)
            {
                var name = _delegate.GetUniqueName(suggestedName);
                return name.Replace("Castle.Proxies", PROXY_PREFIX);
            }

            public INamingScope SafeSubScope()
            {
                return new AlpacaNamingScope(this);
            }

            public INamingScope ParentScope { get; private set; }
        }

        private const string PROXY_PREFIX = "Alpaca.Weld.Proxies";
        private static readonly ProxyGenerator ProxyGenerator =
            new ProxyGenerator(new DefaultProxyBuilder(new ModuleScope(false, false, new AlpacaNamingScope(), 
                PROXY_PREFIX, PROXY_PREFIX, PROXY_PREFIX, PROXY_PREFIX)));

        private BuildPlan InjectConstructor(IEnumerable<MethodParameterInjectionPoint> injects)
        {
            var paramInjects = injects.GroupBy(x => x.Member)
                .Select(x => x.OrderBy(i => i.Position).ToArray())
                .DefaultIfEmpty(new MethodParameterInjectionPoint[0])
                .First();
            if (Mixins.Any())
            {
                return context =>
                {
                    var pgo = new ProxyGenerationOptions();
                    foreach (var mixin in Mixins)
                    {
                        pgo.AddMixinInstance(Activator.CreateInstance(mixin));
                    }

                    var paramVals = paramInjects.Select(p => p.GetValue(context)).ToArray();
                    return ProxyGenerator.CreateClassProxy(Type, pgo, paramVals);
                };
            }
        
            return context =>
            {
                var paramVals = paramInjects.Select(p => p.GetValue(context)).ToArray();
                return Activator.CreateInstance(Type, paramVals);
            };
        }

        private IEnumerable<InjectPlan> InjectMethods(IEnumerable<MethodParameterInjectionPoint> injects)
        {
            return from g in injects.GroupBy(x => x.Member)
                let method = (MethodInfo)g.Key
                let paramInjects = g.OrderBy(x => x.Position).ToArray()
                select (InjectPlan) ((target, context) =>
                {
                    var paramVals = paramInjects.Select(p => p.GetValue(context)).ToArray();
                    return method.Invoke(target, paramVals);
                });
        }

        public override string ToString()
        {
            return string.Format("Component [{0}] with Qualifiers [{1}]", Type, string.Join(",", Qualifiers));
        }
    }
}