using System;
using System.Collections.Concurrent;
using Alpaca.Contexts;
using Alpaca.Utils;
using Alpaca.Weld.Components;

namespace Alpaca.Weld.Contexts
{
    public interface IComponentStore
    {
        IContextualInstance Get(ComponentIdentifier id);
        IContextualInstance GetOrPut(ComponentIdentifier id, Func<ComponentIdentifier, IContextualInstance> create);
    }

    public class ConcurrentDictionaryComponentStore : IComponentStore
    {
        private ConcurrentDictionary<ComponentIdentifier, IContextualInstance> _instances = new ConcurrentDictionary<ComponentIdentifier, IContextualInstance>();
        public IContextualInstance Get(ComponentIdentifier id)
        {
            return _instances.GetOrDefault(id);
        }

        public IContextualInstance GetOrPut(ComponentIdentifier id, Func<ComponentIdentifier, IContextualInstance> create)
        {
            return _instances.GetOrAdd(id, create);
        }
    }
}