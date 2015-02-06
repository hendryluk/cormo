using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Injections;
using Cormo.Injects;
using Cormo.Injects.Exceptions;

namespace Cormo.Impl.Weld.Resolutions
{
    public abstract class TypeSafeResolver<TComponent, TResolvable> 
        where TComponent:IWeldComponent where TResolvable:IResolvable
    {
        protected readonly WeldComponentManager Manager;
        private readonly ConcurrentBag<TComponent> _allComponents;

        protected TypeSafeResolver(WeldComponentManager manager, IEnumerable<TComponent> allComponents)
        {
            Manager = manager;
            _allComponents = new ConcurrentBag<TComponent>(allComponents);
        }

        public bool IsWrappedType(Type type)
        {
            return type.IsGenericType && typeof(IInstance<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }

        public Type UnwrapType(Type type)
        {
            return IsWrappedType(type) ? type.GetGenericArguments()[0] : type;
        }

        private bool _isValidated = false;
        private readonly ConcurrentDictionary<TResolvable, TComponent[]> _resolvedCache = new ConcurrentDictionary<TResolvable, TComponent[]>();

        public void Validate()
        {
            foreach (var component in _allComponents.Where(x => x.IsConcrete).ToArray())
            {
                Validate(component, new IComponent[0]);
            }
            _isValidated = true;
        }

        private void Validate(IComponent component, IComponent[] path)
        {
            var nextPath = path.Concat(new []{component}).ToArray();

            if (path.Contains(component))
                throw new CircularDependenciesException(nextPath);

            var producer = component as AbstractProducer;
            if (producer != null)
                Validate(producer.DeclaringComponent, nextPath);

            // This may not be needed since we allow injections of incomplete instance
            //var classComponent = component as ClassComponent;
            //if (classComponent != null)
            //{
            //    foreach (var mixin in classComponent.Mixins)
            //        Validate(mixin, nextPath);
            //    foreach (var interceptor in classComponent.Interceptors)
            //        Validate(interceptor, nextPath);
            //}
                
            foreach (var inject in component.InjectionPoints.OfType<IWeldInjetionPoint>())
            {
                Validate(inject.Component, (inject.Scope is NormalScopeAttribute)? new IComponent[0] : nextPath);
            }
        }

        protected abstract IEnumerable<TComponent> Resolve(TResolvable resolvable, ref IEnumerable<TComponent> components);

        public IEnumerable<TComponent> Resolve(TResolvable resolvable)
        {
            return _resolvedCache.GetOrAdd(resolvable, r =>
            {
                var components = _allComponents.AsEnumerable();
                var results = Resolve(resolvable, ref components);

                var newComponents = components.Where(x => !_allComponents.Contains(x)).ToArray();
                foreach (var c in newComponents)
                {
                    _allComponents.Add(c);
                    Manager.ContextualStore.PutIfAbsent(c);
                    if (_isValidated)
                        Validate(c, new IComponent[0]);
                }

                return results.ToArray();
            });
        }
    }
}