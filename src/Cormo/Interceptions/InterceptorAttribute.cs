using System;
using System.Security.Cryptography.X509Certificates;

namespace Cormo.Interceptions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class InterceptorBinding : Attribute, IInterceptorBinding
    {
    }
}