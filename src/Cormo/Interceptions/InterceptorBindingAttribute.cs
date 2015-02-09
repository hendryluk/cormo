using System;

namespace Cormo.Interceptions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = true)]
    public abstract class InterceptorBindingAttribute : Attribute, IInterceptorBinding
    {
    }
}