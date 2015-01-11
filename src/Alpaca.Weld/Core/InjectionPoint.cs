using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Alpaca.Weld.Core
{
    public class InjectionPoint
    {
        public InjectionPoint(Type requestedType, MemberInfo member, object[] qualifiers)
        {
            RequestedType = requestedType;
            MemberInfo = member;
            Qualifiers = new ReadOnlyCollection<object>(qualifiers);
        }

        public Type RequestedType { get; private set; }
        public MemberInfo MemberInfo { get; private set; }
        public IEnumerable<object> Qualifiers { get; private set; }
        public int Index { get; set; }

        public override string ToString()
        {
            return string.Format("type [{0}] with qualifiers [{1}] at injection point [{2}]",
                RequestedType, string.Join(",", Qualifiers.Select(x => x.GetType().Name)), MemberInfo);
        }
    }
}