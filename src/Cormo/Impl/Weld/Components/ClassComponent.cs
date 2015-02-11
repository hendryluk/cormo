using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        private ClassComponent(ClassComponent parent, ConstructorInfo ctor, IBinders binders, Type scope, WeldComponentManager manager, GenericResolver.Resolution typeResolution)
            : base(new ComponentIdentifier(parent.Id.Key, ctor.DeclaringType), ctor, binders, scope, manager,
                parent.PostConstructs.Select(x => GenericUtils.TranslateMethodGenericArguments(x, typeResolution.GenericParameterTranslations)).ToArray())
        {
            parent.TransferInjectionPointsTo(this, typeResolution);
        }

        public ClassComponent(ConstructorInfo ctor, IBinders binders, Type scope, WeldComponentManager manager,
            MethodInfo[] postConstructs)
            : base(ctor, binders, scope, manager, postConstructs)
        {
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            ClassComponent component;
            if (IsConcrete)
                component = (ClassComponent) base.Resolve(requestedType);

            else
            {
                var resolution = GenericResolver.ImplementerResolver.ResolveType(Type, requestedType);
                if (resolution == null || resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
                    return null;
                var resolvedCtor = GenericUtils.TranslateConstructorGenericArguments(InjectableConstructor.Constructor,
                    resolution.GenericParameterTranslations);

                component = new ClassComponent(this,
                    resolvedCtor,
                    Binders,
                    Scope, Manager, resolution);
            }
            if(component != null)
                RuntimeHelpers.RunClassConstructor(component.Type.TypeHandle);
            return component;
        }

        private IEnumerable<KeyValuePair<MethodInfo, Interceptor[]>> GetInterceptors(Type interceptorType)
        {
            MethodInfo[] classInterceptorMethods;
            var classInterceptors = Manager.GetClassInterceptors(interceptorType, this, out classInterceptorMethods);

            foreach (var method in classInterceptorMethods)
                yield return new KeyValuePair<MethodInfo, Interceptor[]>(method, classInterceptors);

            const BindingFlags bindingFlagsAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                                 BindingFlags.Static;
            foreach (var method in
                    Type.GetMethods(bindingFlagsAll))
            {
                var interceptors = Manager.GetMethodInterceptors(interceptorType, method).ToArray();
                if(interceptors.Any())
                    yield return new KeyValuePair<MethodInfo, Interceptor[]>(method, interceptors);
            }

            foreach (var property in
                    Type.GetProperties(bindingFlagsAll))
            {
                MethodInfo[] methods;
                var interceptors = Manager.GetPropertyInterceptors(interceptorType, property, out methods).ToArray();
                foreach(var method in methods)
                    yield return new KeyValuePair<MethodInfo, Interceptor[]>(method, interceptors);
            }
        }
        
        protected override BuildPlan MakeConstructPlan()
        {
            var mixins = Manager.GetMixins(this);
            // todo: preconstruct/dispose intercetor

            var aroundMethodInterceptors =
                GetInterceptors(typeof (IAroundInvokeInterceptor)).GroupBy(x => x.Key)
                    .Select(x=> new {method = x.Key, interceptors = x.SelectMany(g => g.Value).ToArray()})
                    .ToArray();
                
            if (mixins.Any() || aroundMethodInterceptors.Any())
            {
                return context =>
                {
                    var paramVals = InjectableConstructor.GetParameterValues(context);
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
        
            return InjectableConstructor.Invoke;
        }

        public override string ToString()
        {
            return string.Format("Component [{0}] with Qualifiers [{1}]", Type, string.Join(",", Qualifiers));
        }
    }
}