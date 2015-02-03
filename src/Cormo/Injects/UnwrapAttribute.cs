using System;

namespace Cormo.Injects
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
    public class UnwrapAttribute : Attribute, IBinderAttribute
    {
         
    }
}