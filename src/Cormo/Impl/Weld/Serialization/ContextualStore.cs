using System;
using System.Collections.Concurrent;
using System.Threading;
using Cormo.Contexts;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Components;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Serialization
{
    public class ContextualStore : IContextualStore
    {
        private static readonly String GeneratedIdPrefix = typeof(ContextualStore).Name;

        private readonly ConcurrentDictionary<IContextual, ComponentIdentifier> _contextuals = new ConcurrentDictionary<IContextual, ComponentIdentifier>();
        private readonly ConcurrentDictionary<ComponentIdentifier, IContextual> _contextualsInverse = new ConcurrentDictionary<ComponentIdentifier, IContextual>();
        private readonly ConcurrentDictionary<ComponentIdentifier, IContextual> _passivationCapableContextuals = new ConcurrentDictionary<ComponentIdentifier, IContextual>();

        private int _idIncrement = 0;
        public void Cleanup()
        {
            _contextuals.Clear();
            _contextualsInverse.Clear();
            _passivationCapableContextuals.Clear();
        }

        /// <summary>
        /// Add a contextual (if not already present) to the store, and return it's
        /// id. If the contextual is passivation capable, it's id will be used,
        /// otherwise an id will be generated
        /// </summary>
        /// <param name="contextual">contextual the contexutal to add</param>
        /// <returns>the current id for the contextual</returns>
        public ComponentIdentifier PutIfAbsent(IContextual contextual)
        {
            var passivationCapable = contextual as IPassivationCapable<ComponentIdentifier>;
            if (passivationCapable != null)
            {
                var id = passivationCapable.Id;
                _passivationCapableContextuals.TryAdd(id, contextual);
                return id;
            }

            return _contextuals.GetOrAdd(contextual, _ =>
            {
                var id = new ComponentIdentifier(
                    string.Format("{0}{1}", GeneratedIdPrefix, Interlocked.Increment(ref _idIncrement)));
                _contextualsInverse[id] = contextual;
                return id;
            });
        }

        public IContextual GetContextual(ComponentIdentifier id)
        {
            if (id.Key.StartsWith(GeneratedIdPrefix))
            {
                return _contextualsInverse.GetOrDefault(id);
            }

            var contextual = _passivationCapableContextuals.GetOrDefault(id);
            if (contextual != null) 
                return contextual;

            var seed = _passivationCapableContextuals.GetOrDefault(new ComponentIdentifier(id.Key)) as IWeldComponent;
            if (seed == null) 
                return null;

            contextual = seed.Resolve(seed.Type);
            if (contextual != null)
            {
                PutIfAbsent(contextual);
            }
            return contextual;
        }
    }
}