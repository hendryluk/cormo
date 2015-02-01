using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Cormo.Injects;

namespace Cormo.Interceptions
{
    public interface IAroundnvokeInterceptor
    {
        object AroundInvoke(IInvocationContext invocationContext);
    }

    public interface IInvocationContext
    {
        object Target { get; }
        MethodInfo Method { get; }
        object Proceed();
        object[] Arguments { get; set; }
        IDictionary<string, object> ContextData { get; } 
    }

    public interface IInterceptorBinding: IBinderAttribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class InterceptorBinding : Attribute, IInterceptorBinding
    {
    }
}