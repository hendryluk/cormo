using System;
using System.Collections.Generic;
using Alpaca.Contexts;

namespace Alpaca.Injects
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