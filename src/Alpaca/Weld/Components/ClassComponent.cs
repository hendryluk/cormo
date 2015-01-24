using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Mixins;
using Alpaca.Weld.Injections;
using Alpaca.Weld.Utils;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;

namespace Alpaca.Weld.Components
{
    public class Mixin : ManagedComponent
    {
        public Mixin(Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs) 
            : base(type, qualifiers, scope, manager, postConstructs)
        {
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            return requestedType.IsAssignableFrom(Type) ? this : null;
        }

        protected override BuildPlan MakeConstructPlan(IEnumerable<MethodParameterInjectionPoint> injects)
        {
            var paramInjects = injects.GroupBy(x => x.Member)
                .Select(x => x.OrderBy(i => i.Position).ToArray())
                .DefaultIfEmpty(new MethodParameterInjectionPoint[0])
                .First();

            return context =>
            {
                var paramVals = paramInjects.Select(p => p.GetValue(context)).ToArray();
                return Activator.CreateInstance(Type, paramVals);
            };
        }
    }

    public abstract class ManagedComponent : AbstractComponent
    {
        public IEnumerable<MethodInfo> PostConstructs { get; private set; }
        protected readonly bool ContainsGenericParameters;

        protected ManagedComponent(ComponentIdentifier id, Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs)
            : base(id, type, qualifiers, scope, manager)
        {
            PostConstructs = postConstructs;
            ContainsGenericParameters = Type.ContainsGenericParameters;
            IsDisposable = typeof(IDisposable).IsAssignableFrom(Type);

            ValidateMethodSignatures();
        }

        protected ManagedComponent(Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs) 
            : base(type.FullName, type, qualifiers, scope, manager)
        {
            PostConstructs = postConstructs;
            ContainsGenericParameters = Type.ContainsGenericParameters;
            IsDisposable = typeof(IDisposable).IsAssignableFrom(Type);

            ValidateMethodSignatures();
        }

        protected override BuildPlan GetBuildPlan()
        {
            var paramInject = InjectionPoints.OfType<MethodParameterInjectionPoint>().ToArray();
            var constructPlan = MakeConstructPlan(paramInject.Where(x => x.IsConstructor));
            var methodInject = InjectMethods(paramInject.Where(x => !x.IsConstructor)).ToArray();
            var otherInjects = InjectionPoints.Except(paramInject).Cast<IWeldInjetionPoint>();

            //var create = ctorInject == null? 
            //    new BuildPlan(context => Activator.CreateInstance(Type, true)): 
            //    context => ctorInject(null, context);

            return context =>
            {
                var instance = constructPlan(context);
                foreach (var i in otherInjects)
                    i.Inject(instance, context);
                foreach (var i in methodInject)
                    i(instance, context);
                foreach (var post in PostConstructs)
                    post.Invoke(instance, new object[0]);

                return instance;
            };
        }

        protected abstract BuildPlan MakeConstructPlan(IEnumerable<MethodParameterInjectionPoint> injects);

        private IEnumerable<InjectPlan> InjectMethods(IEnumerable<MethodParameterInjectionPoint> injects)
        {
            return from g in injects.GroupBy(x => x.Member)
                   let method = (MethodInfo)g.Key
                   let paramInjects = g.OrderBy(x => x.Position).ToArray()
                   select (InjectPlan)((target, context) =>
                   {
                       var paramVals = paramInjects.Select(p => p.GetValue(context)).ToArray();
                       return method.Invoke(target, paramVals);
                   });
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
            get { return !ContainsGenericParameters; }
        }

        public bool IsDisposable { get; private set; }
    }

    public class ClassComponent : ManagedComponent
    {
        
        private ClassComponent(ClassComponent parent, Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, WeldComponentManager manager, GenericUtils.Resolution typeResolution)
            : base(new ComponentIdentifier(parent.Id.Key, type), type, qualifiers, scope, manager,
                parent.PostConstructs.Select(x => GenericUtils.TranslateMethodGenericArguments(x, typeResolution.GenericParameterTranslations)).ToArray())
        {
            parent.TransferInjectionPointsTo(this, typeResolution);
            _lazyMixins = new Lazy<IComponent[]>(() => Manager.GetMixins(this));
        }

        public ClassComponent(Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs)
            : base(type, qualifiers, scope, manager, postConstructs)
        {
            _lazyMixins = new Lazy<IComponent[]>(() => Manager.GetMixins(this));
        }

        public IEnumerable<IComponent> Mixins
        {
            get { return _lazyMixins.Value; }
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            if (!ContainsGenericParameters)
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

        private readonly Lazy<IComponent[]> _lazyMixins;

        protected override BuildPlan MakeConstructPlan(IEnumerable<MethodParameterInjectionPoint> injects)
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
                        pgo.AddMixinInstance(Manager.GetReference(mixin, context));
                    
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

        public override string ToString()
        {
            return string.Format("Component [{0}] with Qualifiers [{1}]", Type, string.Join(",", Qualifiers));
        }
    }
}