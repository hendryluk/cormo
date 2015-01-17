using System.Collections.Generic;
using Alpaca.Weld;

namespace Alpaca.Injects.Exceptions
{
    public class AmbiguousResolutionException: InjectionException
    {
        public IInjectionPoint InjectionPoint { get; set; }
        public IComponent[] Registrations { get; set; }

        public AmbiguousResolutionException(IInjectionPoint inject, IComponent[] registrations)
            : base(ConstructMessage(inject, registrations))
        {
            InjectionPoint = inject;
            Registrations = registrations;
        }

        private static string ConstructMessage(IInjectionPoint inject, IEnumerable<IComponent> registrations)
        {
            return string.Format("Ambiguous dependency for {0}. Possible dependencies [{1}]", inject,
                string.Join(",", registrations));
        }
    }
}