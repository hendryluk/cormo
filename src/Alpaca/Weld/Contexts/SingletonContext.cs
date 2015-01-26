using System;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Weld.Injections;
using Alpaca.Weld.Serialization;

namespace Alpaca.Weld.Contexts
{
    public abstract class AbstractContext : IContext
    {
        private readonly IContextualStore _contextualStore;
        public abstract Type Scope { get; }
        public abstract bool IsActive { get; }
        protected abstract IComponentStore ComponentStore { get; }

        protected AbstractContext(IContextualStore contextualStore)
        {
            _contextualStore = contextualStore;
        }

        public object Get(IContextual contextual, ICreationalContext creationalContext, IInjectionPoint injectionPoint)
        {
            if(!IsActive)
                throw new ContextNotActiveException(Scope);
            var store = ComponentStore;

            if (store == null)
                return null;
            if (contextual == null)
                throw new ArgumentNullException("contextual");

            var id = _contextualStore.PutIfAbsent(contextual);
            var instance = store.GetOrPut(id, _ =>
            {
                var i = contextual.Create(creationalContext, injectionPoint);
                return new SerializableContextualInstance(contextual, i, creationalContext, _contextualStore);
            });
            if (instance != null)
                return instance.Instance;
            return null;
        }

        public object Get(IContextual contextual)
        {
            return Get(contextual, null, null);
        }
    }

    public abstract class AbstractSharedContext : AbstractContext
    {
        private readonly IComponentStore _componentStore;
        protected override IComponentStore ComponentStore
        {
            get { return _componentStore; }
        }

        protected AbstractSharedContext(IContextualStore contextualStore) : base(contextualStore)
        {
            _componentStore = new ConcurrentDictionaryComponentStore();
        }

        public override bool IsActive { get { return true; } }
        
    }

    public class SingletonContext: AbstractSharedContext, ISingletonContext
    {
        public SingletonContext(IContextualStore contextualStore) : base(contextualStore)
        {
        }

        public override Type Scope { get { return typeof (SingletonAttribute); } }
    }
}