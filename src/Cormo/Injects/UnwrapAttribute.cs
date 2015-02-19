using System;

namespace Cormo.Injects
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Method)]
    public class UnwrapAttribute : Attribute, IBinderAttribute
    {
         
    }
}