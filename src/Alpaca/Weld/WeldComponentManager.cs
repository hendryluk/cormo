using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using Alpaca.Weld.Component;
using Alpaca.Weld.Injection;
using Alpaca.Weld.Validation;

namespace Alpaca.Weld
{
    public class WeldComponentManager : IComponentManager
    {
        private ConcurrentBag<IWeldComponent> _allComponents;
        private readonly ConcurrentDictionary<Type, IWeldComponent[]> _typeComponents = new ConcurrentDictionary<Type, IWeldComponent[]>();

        private IEnumerable<IWeldComponent> GetComponents(Type type, IEnumerable<QualifierAttribute> qualifiers)
        {
            var unwrappedType = UnwrapType(type);
            var isWrapped = unwrappedType != type;
            
            var components = _typeComponents.GetOrAdd(type, t => 
                _allComponents.Select(x => x.Resolve(t)).Where(x => x != null).ToArray());

            var newComponents = components.Where(x => !_allComponents.Contains(x));

            foreach(var c in newComponents)
            {
                _allComponents.Add(c);
            }

            if (isWrapped)
            {
                components = new IWeldComponent[] {new InstanceComponent(type, qualifiers, this, components)};
            }

            return components;
        }

        public IComponent GetComponent(Type type, params QualifierAttribute[] qualifiers)
        {
            var components = GetComponents(type, qualifiers).ToArray();
            ResolutionValidator.ValidateSingleResult(type, qualifiers, components);
            return components.Single();
        }

        public IComponent GetComponent(IInjectionPoint injectionPoint)
        {
            var components = GetComponents(injectionPoint.ComponentType, injectionPoint.Qualifiers).ToArray();
            ResolutionValidator.ValidateSingleResult(injectionPoint, components);
            return components.Single();
        }

        public object GetReference(IComponent component)
        {
            return ((IWeldComponent) component).Build();
        }

        public object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component)
        {
            return GetReference(component);
        }

        public void Deploy(WeldEnvironment environment)
        {
            _allComponents = new ConcurrentBag<IWeldComponent>(environment.Components);
            ValidateComponents();
            ExecuteConfigurations(environment);
        }

        private void ExecuteConfigurations(WeldEnvironment environment)
        {
            foreach (var config in environment.Configurations)
            {
                GetReference(config);
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

        public object GetReference(Type type, params QualifierAttribute[] qualifiers)
        {
            return GetReference(GetComponent(type, qualifiers));
        }
    }
}