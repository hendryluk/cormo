using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;

namespace Alpaca.Weld
{
    public class WeldComponentManager : IComponentManager
    {
        private ConcurrentBag<IWeldComponent> _unresolvedComponents;
        private ConcurrentBag<IWeldComponent> _allComponents;
        private readonly ConcurrentDictionary<Type, IWeldComponent[]> _typeComponents = new ConcurrentDictionary<Type, IWeldComponent[]>();

        private IEnumerable<IWeldComponent> GetComponentsForType(Type type)
        {
            return _typeComponents.GetOrAdd(type, t => 
                _allComponents.Select(x => x.Resolve(t)).Where(x => x != null).ToArray());
        }

        public IComponent GetComponent(IInjectionPoint injectionPoint)
        {
            var unwrappedType = UnwrapType(injectionPoint.ComponentType);
            var isWrapped = unwrappedType != injectionPoint.ComponentType;

            var resolved = GetComponentsForType(unwrappedType).Where(x=> x.CanSatisfy(injectionPoint.Qualifiers)).ToArray();
            if (!isWrapped)
            {
                if (resolved.Length > 1)
                {
                    throw new AmbiguousResolutionException(injectionPoint, resolved.Cast<IComponent>().ToArray());
                }
                if (!resolved.Any())
                {
                    throw new UnsatisfiedDependencyException(injectionPoint);
                }

                return resolved.Single();
            }

            return resolved.Single();
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
            _unresolvedComponents = new ConcurrentBag<IWeldComponent>(environment.Components.Where(x=> x.IsConcrete));
            
            while (_unresolvedComponents.Any())
                ResolveComponents();

            ResolveConfigurations(environment);
        }

        private void ResolveConfigurations(WeldEnvironment environment)
        {
            foreach (var config in environment.Configurations)
            {
                GetReference(config);
            }
        }

        private void ResolveComponents()
        {
            var toBeResolved = _unresolvedComponents;
            _unresolvedComponents = new ConcurrentBag<IWeldComponent>();
            foreach (var component in toBeResolved)
            {
                var _ = component.InjectionPoints.OfType<IWeldInjetionPoint>().Select(x => x.Component).ToArray();
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

        private bool IsNormalScope(Attribute scope)
        {
            return scope is NormalScopeAttribute;
        }
    }
}