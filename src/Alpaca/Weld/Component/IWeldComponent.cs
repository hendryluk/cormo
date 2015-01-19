using System;
using System.Collections.Generic;
using Alpaca.Injects;

namespace Alpaca.Weld.Component
{
    public interface IWeldComponent : IComponent
    {
        IWeldComponent Resolve(Type type);
        bool CanSatisfy(IEnumerable<QualifierAttribute> qualifiers);

        bool IsProxyRequired { get; }
        bool IsConcrete { get; }
        object Build();
    }
}