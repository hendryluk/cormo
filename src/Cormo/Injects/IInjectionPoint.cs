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
        IEnumerable<IQualifier> Qualifiers { get; }
        IComponent Component { get; }

    }
}