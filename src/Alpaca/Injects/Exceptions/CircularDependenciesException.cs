using System.Collections.Generic;
using Alpaca.Inject;
using Alpaca.Injects.Exceptions;

namespace Alpaca.Injects.Exceptions
{
    public class CircularDependenciesException : InjectionException
    {
        public CircularDependenciesException(IEnumerable<IComponent> nextPath):
            base(string.Format("Pseudo scoped component has circular dependencies. Dependency path [{0}]",
                string.Join(",", nextPath)))
        {
        }
    }
}