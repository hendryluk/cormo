using System;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public interface IWeldComponent : IComponent, IPassivationCapable<ComponentIdentifier>
    {
        IWeldComponent Resolve(Type type);
        bool IsConcrete { get; }
        bool IsConditionalOnMissing { get; }
    }
}