using System.Collections.Generic;
using System.Reflection;

namespace Cormo.Interceptions
{
    public interface IInvocationContext
    {
        object Target { get; }
        MethodInfo Method { get; }
        object Proceed();
        object[] Arguments { get; set; }
        IDictionary<string, object> ContextData { get; } 
    }
}