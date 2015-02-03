using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Web.Api;

namespace Cormo.Web.Impl
{
    [GlobalFilter]
    public class RequestProducerHandler : DelegatingHandler
    {
        [RequestScoped]
        public class RequestProducer
        {
            [Produces]
            private HttpRequestMessage _request;

            public virtual void SetRequest(HttpRequestMessage request)
            {
                _request = request;
            }

            [Produces, RequestScoped]
            public virtual IPrincipal GetPrincipal()
            {
                return _request.GetRequestContext().Principal;
            }
        }

        [Inject] private RequestProducer _requestProducer;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requestProducer.SetRequest(request);
            return base.SendAsync(request, cancellationToken);
        }
    }
}