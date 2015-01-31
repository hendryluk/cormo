using System;
using System.Collections.Generic;
using Cormo.Impl.Weld.Components;

namespace Cormo.Impl.Weld.Contexts
{
    public interface IComponentStore
    {
        IContextualInstance Get(ComponentIdentifier id);
        IContextualInstance GetOrPut(ComponentIdentifier id, Func<ComponentIdentifier, IContextualInstance> create);
        IEnumerable<IContextualInstance> AllInstances { get; }
    }
}