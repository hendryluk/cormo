using System;
using System.Collections.Generic;
using Alpaca.Contexts;
using Alpaca.Injects;

namespace Alpaca.Injects
{
    public interface IComponent: IContextual
    {
        IComponentManager Manager { get; }
        Type Type { get; }
        IEnumerable<QualifierAttribute> Qualifiers { get; }
        IEnumerable<IInjectionPoint> InjectionPoints { get; }
        Type Scope { get; }
    }
}