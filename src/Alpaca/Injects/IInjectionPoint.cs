using System;
using System.Collections.Generic;
using System.Reflection;

namespace Alpaca.Inject
{
    public interface IInjectionPoint
    {
        IComponent DeclaringComponent { get; }
        MemberInfo Member { get; }
        Type ComponentType { get; }
        IEnumerable<QualifierAttribute> Qualifiers { get; }
    }
}