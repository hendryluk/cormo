using System.Collections.Generic;
using Cormo.Contexts;

namespace Cormo.Weld.Contexts
{
    public interface IWeldCreationalContext : ICreationalContext
    {
        IEnumerable<IContextualInstance> DependentInstances { get; }
        void AddDependentInstance(IContextualInstance contextualInstance);
    }
}