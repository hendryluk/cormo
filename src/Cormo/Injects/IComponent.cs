using System;
using System.Collections.Generic;
using Cormo.Contexts;

namespace Cormo.Injects
{
    public interface IComponent: IContextual
    {
        IComponentManager Manager { get; }
        Type Type { get; }
        IQualifiers Qualifiers { get; }
        Type Scope { get; }
        bool IsProxyRequired { get; }
        IBinders Binders { get; }
    }
}