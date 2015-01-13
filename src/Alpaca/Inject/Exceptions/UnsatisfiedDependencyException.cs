using Alpaca.Weld;

namespace Alpaca.Inject.Exceptions
{
    public class UnsatisfiedDependencyException: InjectionException
    {
        public WeldEngine.InjectionPoint InjectRegistration { get; private set; }

        public UnsatisfiedDependencyException(WeldEngine.InjectionPoint inject):
            base(string.Format("Unsatisfied depdendency for {0}", inject))
        {
            InjectRegistration = inject;
        }
    }
}