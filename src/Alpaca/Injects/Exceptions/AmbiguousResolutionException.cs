using System.Collections.Generic;
using Alpaca.Weld;

namespace Alpaca.Injects.Exceptions
{
    public class AmbiguousResolutionException: InjectionException
    {
        public InjectRegistration InjectRegistration { get; set; }
        public AbstractComponent[] Registrations { get; set; }

        public AmbiguousResolutionException(InjectRegistration inject, AbstractComponent[] registrations)
            : base(ConstructMessage(inject, registrations))
        {
            InjectRegistration = inject;
            Registrations = registrations;
        }

        private static string ConstructMessage(InjectRegistration inject, IEnumerable<AbstractComponent> registrations)
        {
            return string.Format("Ambiguous dependency for {0}. Possible dependencies [{1}]", inject,
                string.Join(",", registrations));
        }
    }
}