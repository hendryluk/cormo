using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Catch;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Contexts;
using Cormo.Impl.Weld.Injections;
using Cormo.Impl.Weld.Introspectors;
using Cormo.Impl.Weld.Resolutions;
using Cormo.Impl.Weld.Serialization;
using Cormo.Impl.Weld.Utils;
using Cormo.Impl.Weld.Validations;
using Cormo.Injects;

namespace Cormo.Impl.Weld
{
    public class WeldComponentManager : IComponentManager, IServiceRegistry
    {
        public WeldComponentManager(string id)
        {
            Id = id;
            _services.Add(typeof(ContextualStore), _contextualStore = new ContextualStore());
            _services.Add(typeof(CurrentInjectionPoint), _currentInjectionPoint = new CurrentInjectionPoint());

            _componentResolver = new ComponentResolver(this, _registeredComponents);
            _observerResolver = new ObserverResolver(this, _registeredObservers);
        }

        private readonly List<IWeldComponent> _registeredComponents = new List<IWeldComponent>();
        private readonly List<EventObserverMethod> _registeredObservers = new List<EventObserverMethod>();

        private readonly ComponentResolver _componentResolver;
        private readonly ObserverResolver _observerResolver;
        private MixinResolver _mixinResolver;
        private InterceptorResolver _interceptorResolver;
        
        private readonly ConcurrentDictionary<Type, IList<IContext>> _contexts = new ConcurrentDictionary<Type, IList<IContext>>();

        private readonly IContextualStore _contextualStore;
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly CurrentInjectionPoint _currentInjectionPoint;
        
        public IContextualStore ContextualStore
        {
            get { return _contextualStore; }
        }


        public Mixin[] GetMixins(IComponent component)
        {
            if (_mixinResolver == null)
                throw new InvalidOperationException("Weld component manager is not yet deployed");

            return _mixinResolver.Resolve(new MixinResolvable(component)).ToArray();
        }

        
        public IEnumerable<IComponent> GetComponents(Type type, IQualifier[] qualifierArray)
        {
            return _componentResolver.Resolve(new ComponentResolvable(type, qualifierArray));
        }

        public IComponent GetComponent(Type type, params IQualifier[] qualifiers)
        {
            qualifiers = qualifiers.DefaultIfEmpty(DefaultAttribute.Instance).ToArray();
            var components = GetComponents(type, qualifiers).ToArray();
            ResolutionValidator.ValidateSingleResult(type, qualifiers, components);
            return components.Single();
        }

        public IComponent GetComponent(IInjectionPoint injectionPoint)
        {
            var components = GetComponents(injectionPoint.ComponentType, injectionPoint.Qualifiers.ToArray()).ToArray();
            ResolutionValidator.ValidateSingleResult(injectionPoint, components);
            return components.Single();
        }

        public object GetReference(IComponent component, ICreationalContext creationalContext, params Type[] proxyTypes)
        {
            return GetReference(null, component, creationalContext, proxyTypes);
        }

        public ICreationalContext CreateCreationalContext(IContextual contextual)
        {
            return new WeldCreationalContext(contextual);
        }

        public void Deploy(WeldEnvironment environment)
        {
            environment.AddValue(this, new IAnnotation[0], this);
            environment.AddValue(new ContextualStore(), new IAnnotation[0], this);
           
            var mixins = environment.Components.OfType<Mixin>().ToArray();
            var interceptors = environment.Components.OfType<Interceptor>().ToArray();

            _registeredComponents.AddRange(environment.Components.Except(mixins).Except(interceptors));
            _componentResolver.Invalidate();
            AddObservers(environment.Observers);
            
            _mixinResolver = new MixinResolver(this, mixins);
            _interceptorResolver = new InterceptorResolver(this, interceptors);
            _services.Add(typeof(IExceptionHandlerDispatcher), new ExceptionHandlerDispatcher(this, environment.ExceptionHandlers));

            _componentResolver.Validate();
            
            ExecuteConfigurations(environment);
        }

        private void ExecuteConfigurations(WeldEnvironment environment)
        {
            foreach (var config in environment.Configurations.OrderBy(x=> x.Type.Name))
            {
                GetReference(config, CreateCreationalContext(config));
            }
        }

        public object GetInjectableReference(IInjectionPoint injectionPoint, ICreationalContext creationalContext)
        {
            var proxyTypes = new []{injectionPoint.ComponentType};
            var weldIp = injectionPoint as IWeldInjetionPoint;
            if (weldIp != null && weldIp.Unwraps)
                proxyTypes = new Type[0];

            return GetReference(injectionPoint, injectionPoint.Component, creationalContext, proxyTypes);
        }

        private object GetReference(IInjectionPoint injectionPoint, IComponent component, ICreationalContext creationalContext, params Type[] proxyTypes)
        {
            var pushInjectionPoint = injectionPoint != null && injectionPoint.ComponentType != typeof (IInjectionPoint);

            try
            {
                if (pushInjectionPoint)
                    _currentInjectionPoint.Push(injectionPoint);

                if (proxyTypes.Any() && component.IsProxyRequired)
                {
                    foreach(var proxyType in proxyTypes)
                        InjectionValidator.ValidateProxiable(proxyType, injectionPoint);

                    return CormoProxyGenerator.CreateProxy(proxyTypes,
                        () =>
                        {
                            try
                            {
                                if (pushInjectionPoint)
                                    _currentInjectionPoint.Push(injectionPoint);
                                return GetReference(injectionPoint, component, creationalContext, new Type[0]);
                            }
                            finally
                            {
                                if (pushInjectionPoint)
                                    _currentInjectionPoint.Pop();
                            }
                        });
                }

                var context = creationalContext as IWeldCreationalContext;
                if (context != null)
                {
                    var incompleteInstance = context.GetIncompleteInstance(component);
                    if (incompleteInstance != null)
                        return incompleteInstance;
                }
                creationalContext = creationalContext.GetCreationalContext(component);
                return GetContext(component.Scope).Get(component, creationalContext);
            }
            finally
            {
                if (pushInjectionPoint)
                    _currentInjectionPoint.Pop();
            }
           
        }

       

        //public T GetReference<T>(params IQualifier[] qualifiers)
        //{
        //    return (T)GetReference(typeof(T), qualifiers);
        //}
        //public object GetReference(Type type, params IQualifier[] qualifiers)
        //{
        //    var component = GetComponent(type, qualifiers);
        //    return GetReference(component, CreateCreationalContext(component));
        //}

        public string Id { get; private set; }

        public void AddContext(IContext context)
        {
            _services.Add(context.GetType(), context);
            _contexts.GetOrAdd(context.Scope, _=> new List<IContext>()).Add(context);
        }

        public T GetService<T>()
        {
            return (T) _services.GetOrDefault(typeof (T));
        }

        public IContext GetContext(Type scope)
        {
            IList<IContext> contexts;
            if(!_contexts.TryGetValue(scope, out contexts))
                throw new ContextNotActiveException(scope);

            var activeContexts = contexts.Where(x => x.IsActive).ToArray();
            if(!activeContexts.Any())
                throw new ContextNotActiveException(scope);

            if (activeContexts.Count() > 1)
                throw new ContextException(string.Format("Duplicate contexts: [{0}]", scope.Name));

            return activeContexts.Single();
        }

        public Interceptor[] GetMethodInterceptors(Type interceptorType, MethodInfo methodInfo)
        {
            if (_interceptorResolver == null)
                throw new InvalidOperationException("Weld component manager is not yet deployed");

            var resolvable = new InterceptorResolvable(interceptorType, methodInfo);
            var interceptors = _interceptorResolver.Resolve(resolvable).ToArray();
            if (interceptors.Any())
                InterceptionValidator.ValidateInterceptableMethod(methodInfo, resolvable);
            
            return interceptors;
        }

        public Interceptor[] GetPropertyInterceptors(Type interceptorType, PropertyInfo property, out MethodInfo[] methods)
        {
            if (_interceptorResolver == null)
                throw new InvalidOperationException("Weld component manager is not yet deployed");

            var resolvable = new InterceptorResolvable(interceptorType, property);
            var interceptors = _interceptorResolver.Resolve(resolvable).ToArray();
            if (interceptors.Any())
            {
                methods = new[] {property.SetMethod, property.GetMethod}.Where(x => x != null).ToArray();
                foreach(var method in methods)
                    InterceptionValidator.ValidateInterceptableMethod(method, resolvable);
            }
            else
                methods = new MethodInfo[0];

            return interceptors;
        }

        public Interceptor[] GetClassInterceptors(Type interceptorType, IComponent component, out MethodInfo[] methods)
        {
            if (_interceptorResolver == null)
                throw new InvalidOperationException("Weld component manager is not yet deployed");

            var intercetorResolvable = new InterceptorResolvable(interceptorType, component);
            var interceptors = _interceptorResolver.Resolve(intercetorResolvable).ToArray();
            var allowPartial = interceptors.All(x => x.AllowPartialInterception);
            if (interceptors.Any())
                InterceptionValidator.ValidateInterceptableClass(component.Type, intercetorResolvable, allowPartial, out methods);
            else
                methods = new MethodInfo[0];

            return interceptors;
        }

        public IEnumerable<EventObserverMethod> ResolveObservers(Type eventType, IQualifiers qualifiers)
        {
            return _observerResolver.Resolve(new ObserverResolvable(eventType, qualifiers));
        }

        public void FireEvent<T>(T ev, IQualifiers qualifiers)
        {
            foreach (var observer in ResolveObservers(typeof (T), qualifiers))
                observer.Notify(ev);
        }

        public void AddExtensions(IEnumerable<ExtensionComponent> extensions)
        {
            _registeredComponents.AddRange(extensions);
            _componentResolver.Invalidate();
        }

        public void AddObservers(IEnumerable<EventObserverMethod> observers)
        {
            _registeredObservers.AddRange(observers);
            _observerResolver.Invalidate();
        }
    }
}