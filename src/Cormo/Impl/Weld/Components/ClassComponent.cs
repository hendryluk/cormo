using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Injections;
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

            _lazyInterceptors = new Lazy<Interceptor[]>(() => Manager.GetInterceptors(this));
            //_interceptedMethods = InitInterceptedMethods();
        }

        private MethodInfo[] InitInterceptedMethods()
        {
            var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            var typeBindings = Type.GetAttributesRecursive<IInterceptorBinding>().ToArray();
            
            IDictionary<MethodInfo, IInterceptorBinding[]> methodBindings = new Dictionary<MethodInfo, IInterceptorBinding[]>();

            if (typeBindings.Any())
            {
                methodBindings = methods.Where(x => !x.IsPrivate && !x.IsStatic).ToDictionary(x => x, _ => typeBindings);
            }
            else
            {
                var onMethods = (from method in methods
                                let bindings = method.GetAttributesRecursive<IInterceptorBinding>().ToArray()
                                where methodBindings.Any()
                                select new {method, bindings})
                                .ToDictionary(x=> x.method, x=> x.bindings);

            }
                
            TypeUtils.ValidateInterceptable(methods);
            return methods;
        }

        public ClassComponent(Type type, IBinders binders, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs)
            : base(type, binders, scope, manager, postConstructs)
        {
            _lazyMixins = new Lazy<Mixin[]>(() => Manager.GetMixins(this));
            _lazyInterceptors = new Lazy<Interceptor[]>(() => new Interceptor[0]);
        }

        public IEnumerable<Mixin> Mixins
        {
            get { return _lazyMixins.Value; }
        }

        public IEnumerable<Interceptor> Interceptors
        {
            get { return _lazyInterceptors.Value; }
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
        private readonly Lazy<Interceptor[]> _lazyInterceptors;
        private MethodInfo[] _interceptedMethods;

        protected override BuildPlan MakeConstructPlan(IEnumerable<MethodParameterInjectionPoint> injects)
        {
            var paramInjects = injects.GroupBy(x => x.Member)
                .Select(x => x.OrderBy(i => i.Position).ToArray())
                .DefaultIfEmpty(new MethodParameterInjectionPoint[0])
                .First();
            
            if (Mixins.Any() || Interceptors.Any())
            {
                return context =>
                {
                    var paramVals = paramInjects.Select(p => p.GetValue(context)).ToArray();
                    var mixinObjects = (from mixin in Mixins
                            let reference = Manager.GetReference(mixin, context, mixin.InterfaceTypes)
                            from interfaceType in mixin.InterfaceTypes
                            select new {interfaceType, reference})
                            .ToDictionary(x => x.interfaceType, x => x.reference);

                    return CormoProxyGenerator.CreateMixins(Type, mixinObjects, paramVals);
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