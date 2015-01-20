using System;
using System.Linq;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;

namespace Alpaca.Weld.Validations
{
    public static class ResolutionValidator
    {
        public static void ValidateSingleResult(Type type, QualifierAttribute[] qualifiers, IComponent[] components)
        {
            if (components.Length > 1)
            {
                throw new AmbiguousResolutionException(type, qualifiers, components.ToArray());
            }
            if (!components.Any())
            {
                throw new UnsatisfiedDependencyException(type, qualifiers);
            }
        }

        public static void ValidateSingleResult(IInjectionPoint injectionPoint, IComponent[] components)
        {
            if (components.Length > 1)
            {
                throw new AmbiguousResolutionException(injectionPoint, components.ToArray());
            }
            if (!components.Any())
            {
                throw new UnsatisfiedDependencyException(injectionPoint);
            }
        }
    }
}