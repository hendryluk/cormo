using System;
using Cormo.Injects;

namespace Cormo.Web.Api
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter, AllowMultiple = false)]
    public class HeaderParamAttribute: QualifierAttribute
    {
        public string Name { get; private set; }
        public object Default { get; set; }

        public HeaderParamAttribute()
        {
        }

        public HeaderParamAttribute(string name)
        {
            Name = name;
        }
    }
}