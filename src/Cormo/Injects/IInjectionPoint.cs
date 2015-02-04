using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cormo.Injects
{
    public interface IInjectionPoint
    {
        IComponent DeclaringComponent { get; }
        MemberInfo Member { get; }
        Type ComponentType { get; }
        IQualifiers Qualifiers { get; }
        IComponent Component { get; }

    }
}