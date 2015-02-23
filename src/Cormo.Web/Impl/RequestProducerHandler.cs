using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Web.Api;
using Microsoft.Owin;

namespace Cormo.Web.Impl
{
    [WebProvider]
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

            [Produces, RequestScoped]
            public virtual IOwinContext GetOwinContext()
            {
                return _request.GetOwinContext();
            }
        }

        [Inject] private RequestProducer _requestProducer;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requestProducer.SetRequest(request);
            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}