using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Cormo.Catch;
using Cormo.Contexts;
using Cormo.Impl.Weld.Catch;
using Cormo.Injects;
using Cormo.Web.Api;

namespace Cormo.Web.Impl
{
    [WebProvider]
    public class MessageEnricherFilter : ActionFilterAttribute
    {
        [Inject] private ResponseEnrichers _enricher;
        [Inject, CatchResource] private CatchResponseMessageProducer _producer;
        
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            base.OnActionExecuted(context);
             
            _producer.ProvideResponse(context.Response);
            context.Response = _enricher.Enrich(context.Response, context.Exception);
        }
    }

    public interface IResponseEnricher
    {
        HttpResponseMessage Enrich(HttpResponseMessage message, Exception exception);
    }

    [RequestScoped]
    public class ResponseEnrichers
    {
        [Inject, CatchResource] IInstance<HttpResponseMessage> _catchResponseMessage;
        private readonly IExceptionHandlerDispatcher _exceptionHandlerDispatcher;

        protected ResponseEnrichers()
        {
        }

        [Inject]
        public ResponseEnrichers(IServiceRegistry services)
        {
            _exceptionHandlerDispatcher = services.GetService<IExceptionHandlerDispatcher>();
        }

        private class StatusCodeEnricher : IResponseEnricher
        {
            private readonly HttpStatusCode _statusCode;

            public StatusCodeEnricher(HttpStatusCode statusCode)
            {
                _statusCode = statusCode;
            }

            public HttpResponseMessage Enrich(HttpResponseMessage message, Exception exception)
            {
                message.StatusCode = _statusCode;
                return message;
            }
        }

        private class ReplaceResponseEnricher : IResponseEnricher
        {
            private readonly HttpResponseMessage _responseMessage;

            public ReplaceResponseEnricher(HttpResponseMessage responseMessage)
            {
                _responseMessage = responseMessage;
            }

            public HttpResponseMessage Enrich(HttpResponseMessage message, Exception exception)
            {
                return _responseMessage;
            }
        }

        public virtual void SetStatusCode(HttpStatusCode code)
        {
            _enrichers.Add(new StatusCodeEnricher(code));
        }

        public virtual void ReplaceResponse(HttpResponseMessage response)
        {
            _enrichers.Add(new ReplaceResponseEnricher(response));
        }

        private readonly IList<IResponseEnricher> _enrichers = new List<IResponseEnricher>();

        public virtual HttpResponseMessage Enrich(HttpResponseMessage message, Exception exception)
        {
            if (exception != null)
                _exceptionHandlerDispatcher.Dispatch(null, exception);

            if (message == null)
                message = _catchResponseMessage.Value;
            
            foreach (var enricher in _enrichers)
                message = enricher.Enrich(message, exception);

            return message;
        }
    }
}