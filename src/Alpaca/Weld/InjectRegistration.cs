using System;
using System.Reflection;

namespace Alpaca.Weld
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
    }
}