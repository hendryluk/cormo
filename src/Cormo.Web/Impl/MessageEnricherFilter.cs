using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Cormo.Catch;
using Cormo.Injects;
using Cormo.Web.Api;

namespace Cormo.Web.Impl
{
    [WebProvider]
    public class MessageEnricherFilter : ActionFilterAttribute
    {
        [Inject] private HttpResponseEnrichers _enricher;
        [Inject, CatchResource] private CatchResponseMessageProducer _producer;
        
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            base.OnActionExecuted(context);
             
            _producer.ProvideResponse(context);
            context.Response = _enricher.Enrich(context.Response, context.Exception);
        }
    }
}