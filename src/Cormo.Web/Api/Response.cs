using System.Net;
using System.Net.Http;
using Cormo.Impl.Weld.Contexts;
using Cormo.Injects;

namespace Cormo.Web.Api
{
    public static class Response
    {
        private static IInstance<HttpRequestMessage> _request;

        public static HttpResponseMessage Create(HttpStatusCode statusCode)
        {
            return _request.Value.CreateResponse(statusCode);
        }

        public static HttpResponseMessage Create<T>(HttpStatusCode statusCode, T value)
        {
            return _request.Value.CreateResponse(statusCode, value);
        }

        public static HttpResponseMessage Ok<T>(T value)
        {
            return Create(HttpStatusCode.OK, value);
        }

        public static HttpResponseMessage Accepted()
        {
            return Create(HttpStatusCode.Accepted);
        }

        public static HttpResponseMessage Accepted<T>(T value)
        {
            return Create(HttpStatusCode.Accepted, value);
        }

        public static HttpResponseMessage BadRequest()
        {
            return Create(HttpStatusCode.BadRequest);
        }

        public static HttpResponseMessage BadRequest<T>(T value)
        {
            return Create(HttpStatusCode.BadRequest, value);
        }

        public static HttpResponseMessage NotFound()
        {
            return Create(HttpStatusCode.NotFound);
        }

        [Configuration]
        public class Configurator
        {
            [Inject]
            void Init(IInstance<HttpRequestMessage> request)
            {
                _request = request;
            }
        }
    }
}