using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Alpaca.Weld.Core
{
    public class InjectRegistration
    {
        public InjectRegistration(MemberInfo member, SeekSpec[] dependencies)
        {
            MemberInfo = member;
            Dependencies = dependencies;
        }

        public InjectRegistration(MemberInfo member, Type type, object[] qualifiers)
        {
            MemberInfo = member;
            Dependencies = new []{new SeekSpec(type, qualifiers) };
        }

        public MemberInfo MemberInfo { get; private set; }
        public SeekSpec[] Dependencies { get; set; }

        //public override string ToString()
        //{
        //    return string.Format("type [{0}] with qualifiers [{1}] at injection point [{2}]",
        //        RequestedType, string.Join(",", Qualifiers.Select(x => x.GetType().Name)), MemberInfo);
        //}
    }
}