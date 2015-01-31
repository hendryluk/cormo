using System;
using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Contexts
{
    public abstract class AbstractContext : IContext
    {
        public abstract Type Scope { get; }
        public abstract bool IsActive { get; }
        protected abstract IComponentStore ComponentStore { get; }

        public object Get(IContextual contextual, ICreationalContext creationalContext)
        {
            if(!IsActive)
                throw new ContextNotActiveException(Scope);
            var store = ComponentStore;

            if (store == null)
                return null;
            if (contextual == null)
                throw new ArgumentNullException("contextual");

            var contextualStore = Container.Instance.ContextualStore;
            var id = contextualStore.PutIfAbsent(contextual);
            var instance = store.GetOrPut(id, _ =>
            {
                var i = contextual.Create(creationalContext);
                return new SerializableContextualInstance(contextual, i, creationalContext, contextualStore);
            });
            if (instance != null)
                return instance.Instance;
            return null;
        }

        public object Get(IContextual contextual)
        {
            return Get(contextual, null);
        }
    }
}