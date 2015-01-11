using System.Collections.Generic;
using System.Linq;
using Alpaca.Weld.Core;

namespace Alpaca.Weld
{
    public class AmbiguousDependencyException: WeldException
    {
        public InjectionPoint InjectionPoint { get; set; }
        public ComponentRegistration[] Registrations { get; set; }

        public AmbiguousDependencyException(InjectionPoint inject, ComponentRegistration[] registrations)
            : base(ConstructMessage(inject, registrations))
        {
            InjectionPoint = inject;
            Registrations = registrations;
        }

        private static string ConstructMessage(InjectionPoint inject, IEnumerable<ComponentRegistration> registrations)
        {
            return string.Format("Ambiguous dependency for {0}. Possible dependencies [{1}]", inject,
                string.Join(",", registrations));
        }
    }
}