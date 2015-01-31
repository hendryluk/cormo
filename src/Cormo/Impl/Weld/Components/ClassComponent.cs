using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;
using Cormo.Impl.Weld.Injections;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
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
                    var paramVals = paramInjects.Select(p => p.GetValue(context, p)).ToArray();
                    var mixinObjects = Mixins.Select(x => Manager.GetReference(x.Type, x, context)).ToArray();
                    return CormoProxyGenerator.CreateMixins(Type, mixinObjects, paramVals);
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