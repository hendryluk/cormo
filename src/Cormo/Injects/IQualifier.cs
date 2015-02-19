using System;
using Cormo.Catch;

namespace Cormo.Injects
{
    public interface IQualifier : IBinderAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter)]
    public abstract class QualifierAttribute : Attribute, IQualifier
    {
        public override string ToString()
        {
            var name = GetType().Name;
            if(name.EndsWith("Attribute"))
            {
                return name.Substring(0, name.Length - "Attribute".Length);
            }
            return name;
        }
    }
}