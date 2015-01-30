using System;
using System.Collections.Generic;
using Cormo.Weld.Components;

namespace Cormo.Weld.Contexts
{
    public interface IComponentStore
    {
        IContextualInstance Get(ComponentIdentifier id);
        IContextualInstance GetOrPut(ComponentIdentifier id, Func<ComponentIdentifier, IContextualInstance> create);
        IEnumerable<IContextualInstance> AllInstances { get; }
    }
}