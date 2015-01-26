using System.Collections.Generic;
using Alpaca.Contexts;

namespace Alpaca.Weld.Contexts
{
    public interface IWeldCreationalContext : ICreationalContext
    {
        IEnumerable<IContextualInstance> DependentInstances { get; }
        void AddDependentInstance(IContextualInstance contextualInstance);
    }
}