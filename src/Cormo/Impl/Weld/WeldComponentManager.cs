using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Contexts;
using Cormo.Impl.Weld.Injections;
using Cormo.Impl.Weld.Serialization;
using Cormo.Impl.Weld.Utils;
using Cormo.Impl.Weld.Validations;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Cormo.Utils;

namespace Cormo.Impl.Weld
{
    public class WeldComponentManager : IComponentManager, IServiceRegistry
    {
        public WeldComponentManager(string id)
        {
            Id = id;
            _services.Add(typeof(ContextualStore), _contextualStore = new ContextualStore());
            _services.Add(typeof(CurrentInjectionPoint), _currentInjectionPoint = new CurrentInjectionPoint());
        }
        private ConcurrentBag<IWeldComponent> _allComponents;
        private readonly ConcurrentDictionary<Type, IWeldComponent[]> _typeComponents = new ConcurrentDictionary<Type, IWeldComponent[]>();
        private readonly ConcurrentDictionary<Type, IList<IContext>> _contexts = new ConcurrentDictionary<Type, IList<IContext>>();
        private readonly IContextualStore _contextualStore;
        private bool _isDeployed = false;
        private Mixin[] _allMixins;
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly CurrentInjectionPoint _currentInjectionPoint;
        private Interceptor[] _allInterceptors;

        public IContextualStore ContextualStore
        {
            get { return _contextualStore; }
        }


        public Mixin[] GetMixins(IComponent component)
        {
            return _allMixins.Where(x => x.CanMixTo(component)).ToArray();
        }

        public IEnumerable<IComponent> GetComponents(Type type, IQualifier[] qualifiers)
        {
            qualifiers = qualifiers.DefaultIfEmpty(DefaultAttribute.Instance).ToArray();

            var unwrappedType = UnwrapType(type);
            var isWrapped = unwrappedType != type;
            
            var components = _typeComponents.GetOrAdd(type, t => 
                _allComponents.Select(x => x.Resolve(t)).Where(x => x != null).ToArray());

            var matched = components.Where(x => x.CanSatisfy(qualifiers)).ToArray();
            var newComponents = matched.Where(x => !_allComponents.Contains(x));
            foreach (var c in newComponents)
            {
                _allComponents.Add(c);
                ContextualStore.PutIfAbsent(c);
                if (_isDeployed)
                    Validate(c, new IComponent[0]);
            }
             
            if (isWrapped)
                matched = new IWeldComponent[] { new InstanceComponent(type, qualifiers, this, matched) };
            
            return matched;
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
            return GetInjectableReference(null, component, creationalContext, proxyTypes);
        }

        public ICreationalContext CreateCreationalContext(IContextual contextual)
        {
            return new WeldCreationalContext(contextual);
        }

        public void Deploy(WeldEnvironment environment)
        {
            Container.Instance.Initialize(this);
            environment.AddValue(this, new IQualifier[0], this);
            environment.AddValue(new ContextualStore(), new IQualifier[0], this);
            
            _allMixins = environment.Components.OfType<Mixin>().ToArray();
            _allInterceptors = environment.Components.OfType<Interceptor>().ToArray();
            _allComponents = new ConcurrentBag<IWeldComponent>(environment.Components.Except(_allMixins).Except(_allInterceptors));
            
            ValidateComponents();
            ExecuteConfigurations(environment);
            _isDeployed = true;
        }

        private void ExecuteConfigurations(WeldEnvironment environment)
        {
            foreach (var config in environment.Configurations)
            {
                GetReference(config, CreateCreationalContext(config));
            }
        }

        public object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component, ICreationalContext creationalContext)
        {
            return GetInjectableReference(injectionPoint, component, creationalContext, injectionPoint.ComponentType);
        }

        private object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component, ICreationalContext creationalContext, params Type[] proxyTypes)
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
                                return GetContext(component.Scope).Get(component, creationalContext);
                            }
                            finally
                            {
                                if (pushInjectionPoint)
                                    _currentInjectionPoint.Pop();
                            }
                        });
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

        private void ValidateComponents()
        {
            foreach (var component in _allComponents.Where(x=> x.IsConcrete).ToArray())
            {
                Validate(component, new IComponent[0]);
            }
        }

        private void Validate(IComponent component, IComponent[] path)
        {
            var nextPath = path.Concat(new []{component}).ToArray();

            if (path.Contains(component))
                throw new CircularDependenciesException(nextPath);

            var producer = component as AbstractProducer;
            if (producer != null)
                Validate(producer.DeclaringComponent, nextPath);
            var classComponent = component as ClassComponent;
            if (classComponent != null)
            {
                foreach (var mixin in classComponent.Mixins)
                    Validate(mixin, nextPath);
                foreach (var interceptor in classComponent.Interceptors)
                    Validate(interceptor, nextPath);
            }
                
            foreach (var inject in component.InjectionPoints.OfType<IWeldInjetionPoint>())
            {
                Validate(inject.Component, (inject.Scope is NormalScopeAttribute)? new IComponent[0] : nextPath);
            }
        }

        public bool IsWrappedType(Type type)
        {
            return type.IsGenericType && typeof (IInstance<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }

        public Type UnwrapType(Type type)
        {
            return IsWrappedType(type) ? type.GetGenericArguments()[0] : type;
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

        public Interceptor[] GetInterceptors(IComponent component)
        {
            return _allInterceptors.Where(x => x.CanIntercept(component)).ToArray();
        }
    }
}