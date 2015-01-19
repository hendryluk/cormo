using System;
using System.Collections.Generic;

namespace Alpaca.Inject
{
    public interface IComponent
    {
        IComponentManager Manager { get; }
        Type Type { get; }
        IEnumerable<QualifierAttribute> Qualifiers { get; }
        IEnumerable<IInjectionPoint> InjectionPoints { get; }
        Type Scope { get; }
    }
}