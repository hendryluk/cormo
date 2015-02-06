using System.Threading.Tasks;

namespace Cormo.Interceptions
{
    public interface IAroundInvokeInterceptor
    {
        Task<object> AroundInvoke(IInvocationContext invocationContext);
    }
}