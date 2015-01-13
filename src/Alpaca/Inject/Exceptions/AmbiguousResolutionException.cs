using System.Collections.Generic;
using Alpaca.Weld;

namespace Alpaca.Inject.Exceptions
{
    public class AmbiguousResolutionException: InjectionException
    {
        public InjectRegistration InjectRegistration { get; set; }
        public ComponentRegistration[] Registrations { get; set; }

        public AmbiguousResolutionException(InjectRegistration inject, ComponentRegistration[] registrations)
            : base(ConstructMessage(inject, registrations))
        {
            InjectRegistration = inject;
            Registrations = registrations;
        }

        private static string ConstructMessage(InjectRegistration inject, IEnumerable<ComponentRegistration> registrations)
        {
            return string.Format("Ambiguous dependency for {0}. Possible dependencies [{1}]", inject,
                string.Join(",", registrations));
        }
    }
}