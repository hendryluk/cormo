using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Injections;
using Cormo.Impl.Weld.Interceptions;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;
using Cormo.Interceptions;

namespace Cormo.Impl.Weld.Components
{
    public class ClassComponent : ManagedComponent
    {
        private ClassComponent(ClassComponent parent, Type type, IBinders binders, Type scope, WeldComponentManager manager, GenericUtils.Resolution typeResolution)
            : base(new ComponentIdentifier(parent.Id.Key, type), type, binders, scope, manager,
                parent.PostConstructs.Select(x => GenericUtils.TranslateMethodGenericArguments(x, typeResolution.GenericParameterTranslations)).ToArray())
        {
            parent.TransferInjectionPointsTo(this, typeResolution);
            _lazyMixins = new Lazy<Mixin[]>(() => Manager.GetMixins(this));
        }

        public ClassComponent(Type type, IBinders binders, Type scope, WeldComponentManager manager,
            MethodInfo[] postConstructs)
            : base(type, binders, scope, manager, postConstructs)
        {
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
                Binders, 
                Scope, Manager, resolution);

            return component;
        }

        private readonly Lazy<Mixin[]> _lazyMixins;
        
        protected override BuildPlan MakeConstructPlan(IEnumerable<MethodParameterInjectionPoint> injects)
        {
            var paramInjects = injects.GroupBy(x => x.Member)
                .Select(x => x.OrderBy(i => i.Position).ToArray())
                .DefaultIfEmpty(new MethodParameterInjectionPoint[0])
                .First();

            var mixins = Manager.GetMixins(this);
            // todo: preconstruct/dispose intercetor

            var aroundMethodInterceptors = 
                (from method in Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                let interceptors = Manager.GetMethodInterceptors(typeof(IAroundInvokeInterceptor), method).ToArray()
                where interceptors.Any()
                select new {method, interceptors}).ToArray();

            if (mixins.Any() || aroundMethodInterceptors.Any())
            {
                return context =>
                {
                    var paramVals = paramInjects.Select(p => p.GetValue(context)).ToArray();
                    var mixinObjects = (from mixin in mixins
                            let reference = Manager.GetReference(mixin, context, mixin.InterfaceTypes)
                            from interfaceType in mixin.InterfaceTypes
                            select new {interfaceType, reference})
                            .ToDictionary(x => x.interfaceType, x => x.reference);
                    var interceptorHandlers = aroundMethodInterceptors
                        .ToDictionary(x => x.method,
                            x => new InterceptorMethodHandler(Manager, x.method, x.interceptors, context));

                    return CormoProxyGenerator.CreateComponentProxy(Type, mixinObjects, interceptorHandlers, paramVals);
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