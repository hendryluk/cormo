using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Cormo.Catch;
using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Web.Impl
{
    [CatchResource, RequestScoped]
    public class CatchResponseMessageProducer
    {
        [Inject] HttpResponseEnrichers _enrichers;
        private HttpResponseMessage _response;

        public virtual void ProvideResponse(HttpActionExecutedContext context)
        {
            if (_response == null)
                _response = context.Response;

            if(_response == null && context.Exception != null)
                _response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, context.Exception);
        }

        [Produces, CatchResource, RequestScoped, Unwrap]
        private HttpResponseMessage BuildResponseMessage()
        {
            if (_response == null)
            {
                _response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                _enrichers.ReplaceResponse(_response);
            }
            return _response;
        }
    }
}