using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cormo.Impl.Weld.Components;
using Cormo.Utils;

namespace Cormo.Impl.Weld.Contexts
{
    [Serializable]
    public class ConcurrentDictionaryComponentStore : IComponentStore
    {
        private readonly ConcurrentDictionary<ComponentIdentifier, IContextualInstance> _instances = new ConcurrentDictionary<ComponentIdentifier, IContextualInstance>();
        public IContextualInstance Get(ComponentIdentifier id)
        {
            return _instances.GetOrDefault(id);
        }

        public IContextualInstance GetOrPut(ComponentIdentifier id, Func<ComponentIdentifier, IContextualInstance> create)
        {
            return _instances.GetOrAdd(id, create);
        }

        public IEnumerable<IContextualInstance> AllInstances { get { return _instances.Values; } }
    }
}