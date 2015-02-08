using System;
using System.Security.Cryptography.X509Certificates;

namespace Cormo.Interceptions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = true)]
    public abstract class InterceptorBindingAttribute : Attribute, IInterceptorBinding
    {
    }
}