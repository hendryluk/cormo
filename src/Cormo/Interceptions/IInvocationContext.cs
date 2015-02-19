using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cormo.Injects;

namespace Cormo.Interceptions
{
    public interface IInvocationContext
    {
        object Target { get; }
        MethodInfo Method { get; }
        Task<object> Proceed();
        object[] Arguments { get; set; }
        IDictionary<string, object> ContextData { get; }
        IEnumerable<IInterceptorBinding> Bindings { get; }
    }
}