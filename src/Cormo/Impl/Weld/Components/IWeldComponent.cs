using System;
using System.Collections.Generic;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public interface IWeldComponent : IComponent, IPassivationCapable<ComponentIdentifier>
    {
        IWeldComponent Resolve(Type type);
        bool CanSatisfy(IEnumerable<IQualifier> qualifiers);

        bool IsProxyRequired { get; }
        bool IsConcrete { get; }
    }
}