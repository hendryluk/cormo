using System;
using System.Collections.Generic;
using System.Linq;

namespace Cormo.Injects.Exceptions
{
    public class AmbiguousResolutionException: InjectionException
    {
        public Type Type { get; private set; }
        public IQualifier[] Qualifiers { get; private set; }
        public IInjectionPoint InjectionPoint { get; private set; }
        public IComponent[] Registrations { get; private set; }

        public AmbiguousResolutionException(Type type, IQualifier[] qualifiers, IComponent[] registrations)
            : base(ConstructMessage(type, qualifiers, registrations))
        {
            Type = type;
            Qualifiers = qualifiers;
            Registrations = registrations;
        }

        public AmbiguousResolutionException(IInjectionPoint inject, IComponent[] registrations)
            : base(ConstructMessage(inject, registrations))
        {
            InjectionPoint = inject;
            Type = inject.ComponentType;
            Qualifiers = inject.Qualifiers.ToArray();
            Registrations = registrations;
        }

        private static string ConstructMessage(Type type, IEnumerable<IQualifier> qualifiers, IEnumerable<IComponent> components)
        {
            return string.Format("Ambiguous dependency for type [{0}], qualifiers [{1}]. Possible dependencies [{2}]", type, string.Join(", ", qualifiers),
                string.Join(",", components));
        }

        private static string ConstructMessage(IInjectionPoint inject, IEnumerable<IComponent> components)
        {
            return string.Format("Ambiguous dependency for {0}. Possible dependencies [{1}]", inject,
                string.Join(",", components));
        }
    }
}