using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Injects;
using Cormo.Mixins;
using Cormo.Weld.Injections;
using Cormo.Weld.Utils;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;

namespace Cormo.Weld.Components
{
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

        public class CormoNamingScope : INamingScope
        {
            private readonly INamingScope _delegate;

            public CormoNamingScope()
            {
                _delegate = new NamingScope();
            }
            private CormoNamingScope(CormoNamingScope cormoNamingScope)
            {
                ParentScope = cormoNamingScope;
                _delegate = cormoNamingScope._delegate.SafeSubScope();
            }

            public string GetUniqueName(string suggestedName)
            {
                var name = _delegate.GetUniqueName(suggestedName);
                return name.Replace("Castle.Proxies", PROXY_PREFIX);
            }

            public INamingScope SafeSubScope()
            {
                return new CormoNamingScope(this);
            }

            public INamingScope ParentScope { get; private set; }
        }

        private const string PROXY_PREFIX = "Cormo.Weld.Proxies";
        private static readonly ProxyGenerator ProxyGenerator =
            new ProxyGenerator(new DefaultProxyBuilder(new ModuleScope(false, false, new CormoNamingScope(), 
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
                return (context, ip) =>
                {
                    var pgo = new ProxyGenerationOptions();
                    foreach (var mixin in Mixins)
                        pgo.AddMixinInstance(Manager.GetReference(mixin, context));
                    
                    var paramVals = paramInjects.Select(p => p.GetValue(context, p)).ToArray();
                    return ProxyGenerator.CreateClassProxy(Type, pgo, paramVals);
                };
            }
        
            return (context, ip) =>
            {
                var paramVals = paramInjects.Select(p => p.GetValue(context, p)).ToArray();
                return Activator.CreateInstance(Type, paramVals);
            };
        }

        public override string ToString()
        {
            return string.Format("Component [{0}] with Qualifiers [{1}]", Type, string.Join(",", Qualifiers));
        }
    }
}