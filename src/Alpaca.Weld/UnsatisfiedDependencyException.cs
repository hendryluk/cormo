using System;
using System.Linq;
using Alpaca.Weld.Core;

namespace Alpaca.Weld
{
    public class UnsatisfiedDependencyException: WeldException
    {
        public InjectionPoint InjectionPoint { get; private set; }

        public UnsatisfiedDependencyException(InjectionPoint inject):
            base(string.Format("Unsatisfied depdendency for {0}", inject))
        {
            InjectionPoint = inject;
        }
    }
}