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
        where TComponent : IChainValidatable, IContextual
        where TResolvable:IResolvable
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
            foreach (var component in _allComponents.ToArray())
            {
                Validate(component, new IChainValidatable[0]);
            }
            _isValidated = true;
        }

        private void Validate(IChainValidatable component, IChainValidatable[] path)
        {
            var nextPath = path.Concat(new []{component}).ToArray();

            if (path.Contains(component))
                throw new CircularDependenciesException(nextPath);

            // This may not be needed since we allow injections of incomplete instance
            //var classComponent = component as ClassComponent;
            //if (classComponent != null)
            //{
            //    foreach (var mixin in classComponent.Mixins)
            //        Validate(mixin, nextPath);
            //    foreach (var interceptor in classComponent.Interceptors)
            //        Validate(interceptor, nextPath);
            //}

            foreach (var next in component.NextLinearValidatables)
                Validate(next, nextPath);

            component.NextNonLinearValidatables.ToArray();
            //Validate(next, new IChainValidatable[0]);
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
                    //if (_isValidated)
                        Validate(c, new IChainValidatable[0]);
                }

                return results.ToArray();
            });
        }
    }
}