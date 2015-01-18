using System;
using System.Linq;
using Alpaca.Weld;

namespace Alpaca.Injects.Exceptions
{
    public class UnsatisfiedDependencyException: InjectionException
    {
        public QualifierAttribute[] Qualifiers { get; private set; }
        public Type Type { get; private set; }
        public IInjectionPoint InjectionPoint { get; private set; }

        public UnsatisfiedDependencyException(IInjectionPoint inject) :
            base(string.Format("Unsatisfied depdendency for {0}", inject))
        {
            Type = inject.ComponentType;
            Qualifiers = inject.Qualifiers.ToArray();
            InjectionPoint = inject;
        }

        public UnsatisfiedDependencyException(Type type, QualifierAttribute[] qualifiers) : 
            base(string.Format("Unsatisfied dependency for type [{0}], qualifiers: [{1}]", type, string.Join(",", qualifiers.AsEnumerable())))
        {
            Type = type;
            Qualifiers = qualifiers;
        }

        
    }
}