using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Alpaca.Weld.Core
{
    public class InjectionPoint
    {
        public InjectionPoint(MemberInfo member, object[] qualifiers)
        {
            MemberInfo = member;
            Qualifiers = new ReadOnlyCollection<object>(qualifiers);
        }

        public MemberInfo MemberInfo { get; private set; }
        public IEnumerable<object> Qualifiers { get; private set; } 
    }
}