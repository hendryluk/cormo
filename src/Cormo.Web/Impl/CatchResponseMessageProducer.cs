using System.Net;
using System.Net.Http;
using Cormo.Catch;
using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Web.Impl
{
    [CatchResource, RequestScoped]
    public class CatchResponseMessageProducer
    {
        [Inject] ResponseEnrichers _enrichers;
        private HttpResponseMessage _response;

        public virtual void ProvideResponse(HttpResponseMessage response)
        {
            if (_response == null)
                _response = response;
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