using System;
using System.Collections.Generic;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public interface IChainValidatable
    {
        IEnumerable<IChainValidatable> NextLinearValidatables { get; }
        IEnumerable<IChainValidatable> NextNonLinearValidatables { get; }
    }

    public interface IWeldComponent : IComponent, IChainValidatable, IPassivationCapable<ComponentIdentifier>
    {
        IWeldComponent Resolve(Type type);
        bool IsConditionalOnMissing { get; }
        void Touch();
    }
}