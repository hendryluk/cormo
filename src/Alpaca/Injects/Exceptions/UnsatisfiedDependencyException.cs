using Alpaca.Weld;

namespace Alpaca.Injects.Exceptions
{
    public class UnsatisfiedDependencyException: InjectionException
    {
        public IInjectionPoint InjectionPoint { get; private set; }

        public UnsatisfiedDependencyException(IInjectionPoint inject) :
            base(string.Format("Unsatisfied depdendency for {0}", inject))
        {
            InjectionPoint = inject;
        }
    }
}