using System.Collections.Generic;

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