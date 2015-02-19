using System.Linq;
using System.Threading.Tasks;
using Cormo.Injects;
using Cormo.Interceptions;
using Cormo.Web.Api;

namespace Cormo.Web.Impl
{
    [Interceptor, HttpStatusCode]
    public class HttpStatusCodeInterceptor : IAroundInvokeInterceptor
    {
        [Inject]ResponseEnrichers _enrichers;
        
        public Task<object> AroundInvoke(IInvocationContext invocationContext)
        {
            var result = invocationContext.Proceed();

            foreach (var binding in invocationContext.Bindings.OfType<HttpStatusCodeAttribute>())
                _enrichers.SetStatusCode(binding.StatusCode);

            return result;
        }
    }
}