using System.Collections.Generic;
using Cormo.Contexts;

namespace Cormo.Impl.Weld.Contexts
{
    public interface IWeldCreationalContext : ICreationalContext
    {
        IEnumerable<IContextualInstance> DependentInstances { get; }
        void AddDependentInstance(IContextualInstance contextualInstance);
        void Release(IContextual contextual, object instance);
        object GetIncompleteInstance(IContextual contextual);
    }
}