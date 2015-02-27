using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Injects;
using Cormo.Injects.Exceptions;

namespace Cormo.Impl.Weld.Resolutions
{
    public abstract class ContextualResolver<TComponent, TResolvable>:
        TypeSafeResolver<TComponent, TResolvable>
        where TComponent : IChainValidatable, IContextual
        where TResolvable : IResolvable 
    {
        protected ContextualResolver(WeldComponentManager manager, IEnumerable<TComponent> allComponents) : base(manager, allComponents)
        {
        }

        protected override void RegisterNewComponent(TComponent c)
        {
            base.RegisterNewComponent(c);
            Manager.ContextualStore.PutIfAbsent(c);
        }
    }

    public abstract class TypeSafeResolver<TComponent, TResolvable>
        where TComponent : IChainValidatable
        where TResolvable: IResolvable
    {
        protected readonly WeldComponentManager Manager;
        private readonly IEnumerable<TComponent> _registeredComponents;
        private ConcurrentBag<TComponent> _allComponents;

        protected TypeSafeResolver(WeldComponentManager manager, IEnumerable<TComponent> allComponents)
        {
            Manager = manager;
            _registeredComponents = allComponents;
            _allComponents = new ConcurrentBag<TComponent>(_registeredComponents);
        }

        public bool IsWrappedType(Type type)
        {
            return type.IsGenericType && typeof(IInstance<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }

        public Type UnwrapType(Type type)
        {
            return IsWrappedType(type) ? type.GetGenericArguments()[0] : type;
        }

        private readonly ConcurrentDictionary<TResolvable, TComponent[]> _resolvedCache = new ConcurrentDictionary<TResolvable, TComponent[]>();

        public void Validate()
        {
            foreach (var component in _allComponents.ToArray())
            {
                Validate(component, new IChainValidatable[0]);
            }
        }

        private void Validate(IChainValidatable component, IChainValidatable[] path)
        {
            var nextPath = path.Concat(new []{component}).ToArray();

            if (path.Contains(component))
                throw new CircularDependenciesException(nextPath);

            foreach (var next in component.NextLinearValidatables)
                Validate(next, nextPath);

            component.NextNonLinearValidatables.ToArray();
        }

        protected abstract IEnumerable<TComponent> Resolve(TResolvable resolvable, ref IEnumerable<TComponent> components);

        public IEnumerable<TComponent> Resolve(TResolvable resolvable)
        {
            return _resolvedCache.GetOrAdd(resolvable, r =>
            {
                var components = _registeredComponents.AsEnumerable();
                var results = Resolve(resolvable, ref components);

                var newComponents = components.Where(x => !_allComponents.Contains(x)).ToArray();
                foreach (var c in newComponents)
                {
                    RegisterNewComponent(c);
                    Validate(c, new IChainValidatable[0]);
                }

                return results.ToArray();
            });
        }

        public void Invalidate()
        {
            _allComponents = new ConcurrentBag<TComponent>(_registeredComponents);
            _resolvedCache.Clear();
        }

        protected virtual void RegisterNewComponent(TComponent c)
        {
            _allComponents.Add(c);
        }
    }
}