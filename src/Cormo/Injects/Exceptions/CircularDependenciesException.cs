using System.Collections.Generic;
using Cormo.Injects;
using Cormo.Injects.Exceptions;

namespace Cormo.Injects.Exceptions
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