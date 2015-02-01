using System;
using System.Linq;
using Cormo.Injects;
using Cormo.Injects.Exceptions;

namespace Cormo.Injects.Exceptions
{
    public class UnsatisfiedDependencyException: InjectionException
    {
        public IQualifier[] Qualifiers { get; private set; }
        public Type Type { get; private set; }
        public IInjectionPoint InjectionPoint { get; private set; }

        public UnsatisfiedDependencyException(IInjectionPoint inject) :
            base(string.Format("Unsatisfied depdendency for {0}", inject))
        {
            Type = inject.ComponentType;
            Qualifiers = inject.Qualifiers.ToArray();
            InjectionPoint = inject;
        }

        public UnsatisfiedDependencyException(Type type, IQualifier[] qualifiers) : 
            base(string.Format("Unsatisfied dependency for type [{0}], qualifiers: [{1}]", type, string.Join(",", qualifiers.AsEnumerable())))
        {
            Type = type;
            Qualifiers = qualifiers;
        }

        
    }
}