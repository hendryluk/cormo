using System;
using System.Collections.Generic;
using Alpaca.Injects;

namespace Alpaca.Weld.Components
{
    public interface IWeldComponent : IComponent, IPassivationCapable
    {
        IWeldComponent Resolve(Type type);
        bool CanSatisfy(IEnumerable<QualifierAttribute> qualifiers);

        bool IsProxyRequired { get; }
        bool IsConcrete { get; }
    }
}