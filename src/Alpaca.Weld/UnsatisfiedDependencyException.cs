using System;
using System.Linq;
using Alpaca.Weld.Core;

namespace Alpaca.Weld
{
    public class UnsatisfiedDependencyException: WeldException
    {
        public WeldEngine.InjectionPoint InjectRegistration { get; private set; }

        public UnsatisfiedDependencyException(WeldEngine.InjectionPoint inject):
            base(string.Format("Unsatisfied depdendency for {0}", inject))
        {
            InjectRegistration = inject;
        }
    }
}