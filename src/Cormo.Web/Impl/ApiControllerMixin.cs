using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Cormo.Mixins;
using Cormo.Web.Api;

namespace Cormo.Web.Impl
{
    public class QueryParamProducer
    {
        [Produces, QueryParam]
        T GetQueryParam<T>(IInjectionPoint ip)
        {
            var context = HttpContext.Current;
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (context == null || converter == null)
                throw new UnsatisfiedDependencyException(ip);

            var name = GetQueryName(ip);
            var value = context.Request.QueryString[name];

            if (value == null)
                return default(T);

            return (T) converter.ConvertFromString(value);
        }

        protected string GetQueryName(IInjectionPoint ip)
        {
            var attrName = ip.Qualifiers.OfType<QueryParamAttribute>().Select(x => x.Name).SingleOrDefault();
            if (string.IsNullOrEmpty(attrName))
            {
                var methodParam = ip as IMethodParameterInjectionPoint;
                if (methodParam != null)
                    return methodParam.ParameterInfo.Name;

                return ip.Member.Name;
            }
            return attrName;
        }
    }

    [GlobalFilter]
    public class RequestProducerHandler : DelegatingHandler
    {
        [RequestScoped]
        public class RequestProducer
        {
            [Produces, RequestScoped]
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

    [RestController]
    [RequestScoped]
    [Mixin(typeof(IHttpController))]
    public class ApiControllerMixin : ApiController
    {
        
    }
}