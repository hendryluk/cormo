using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using Alpaca.Mixins;
using Alpaca.Weld.Components;
using Alpaca.Weld.Contexts;
using Alpaca.Weld.Injections;
using Alpaca.Weld.Serialization;
using Alpaca.Weld.Validations;

namespace Alpaca.Weld
{
    public class WeldComponentManager : IComponentManager
    {
        public WeldComponentManager(string id)
        {
            Id = id;
        }
        private ConcurrentBag<IWeldComponent> _allComponents;
        private readonly ConcurrentDictionary<Type, IWeldComponent[]> _typeComponents = new ConcurrentDictionary<Type, IWeldComponent[]>();
        private readonly ConcurrentDictionary<Type, IList<IContext>> _contexts = new ConcurrentDictionary<Type, IList<IContext>>();
        private readonly IContextualStore _contextualStore = new ContextualStore();
        private bool _isDeployed = false;
        private IWeldComponent[] _allMixins;

        public IContextualStore ContextualStore
        {
            get { return _contextualStore; }
        }


        public IWeldComponent[] GetMixins(IComponent component)
        {
            return _allMixins.Where(x => x.Qualifiers.All(b => component.Qualifiers.Contains(b))).ToArray();
        }

        public IEnumerable<IComponent> GetComponents(Type type, QualifierAttribute[] qualifiers)
        {
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

        public IComponent GetComponent(Type type, params QualifierAttribute[] qualifiers)
        {
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

        public object GetReference(IComponent component, ICreationalContext creationalContext)
        {
            var context = GetContext(component.Scope);
            return context.Get(component, creationalContext);
        }

        public ICreationalContext CreateCreationalContext(IContextual contextual)
        {
            return new WeldCreationalContext(contextual);
        }

        public object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component, ICreationalContext context)
        {
            return GetReference(component, context);
        }

        public void Deploy(WeldEnvironment environment)
        {
            environment.AddValue(this, new QualifierAttribute[0], this);
            environment.AddValue(new ContextualStore(), new QualifierAttribute[0], this);
            Container.Instance.Initialize(this);
            
            _allMixins = new ConcurrentBag<IWeldComponent>(environment.Components.OfType<Mixin>()).ToArray();
            _allComponents = new ConcurrentBag<IWeldComponent>(environment.Components.Except(_allMixins));
            
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

        private void ValidateComponents()
        {
            foreach (var component in _allComponents.ToArray())
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
                {
                    Validate(mixin, nextPath);
                }
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

        public bool IsProxyRequired(IComponent component)
        {
            return IsNormalScope(component.Scope);
        }

        private bool IsNormalScope(Type scope)
        {
            return typeof(NormalScopeAttribute).IsAssignableFrom(scope);
        }

        public T GetReference<T>(params QualifierAttribute[] qualifiers)
        {
            return (T) GetReference(typeof (T), qualifiers);
        }

        public string Id { get; private set; }

        public object GetReference(Type type, params QualifierAttribute[] qualifiers)
        {
            var component = GetComponent(type, qualifiers);
            return GetReference(component, CreateCreationalContext(component));
        }

        public void AddContext(IContext context)
        {
            _contexts.GetOrAdd(context.Scope, _=> new List<IContext>()).Add(context);
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
    }
}