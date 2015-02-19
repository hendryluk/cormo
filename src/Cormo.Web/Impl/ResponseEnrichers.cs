using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Cormo.Catch;
using Cormo.Contexts;
using Cormo.Impl.Weld.Catch;
using Cormo.Injects;

namespace Cormo.Web.Impl
{
    public interface IHttpResponseEnricher
    {
        HttpResponseMessage Enrich(HttpResponseMessage message, Exception exception);
    }

    public interface IHttpResponseEnrichers
    {
        void Do(Action<HttpResponseMessage> action);
        void SetStatusCode(HttpStatusCode code);
        void ReplaceResponse(HttpResponseMessage response);
    }

    [RequestScoped]
    public class HttpResponseEnrichers : IHttpResponseEnrichers
    {
        [Inject, CatchResource] IInstance<HttpResponseMessage> _catchResponseMessage;
        private readonly IExceptionHandlerDispatcher _exceptionHandlerDispatcher;

        protected HttpResponseEnrichers()
        {
        }

        [Inject]
        public HttpResponseEnrichers(IServiceRegistry services)
        {
            _exceptionHandlerDispatcher = services.GetService<IExceptionHandlerDispatcher>();
        }

        private class StatusCodeEnricher : IHttpResponseEnricher
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

        private class ReplaceResponseEnricher : IHttpResponseEnricher
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

        private class DelegateEnricher : IHttpResponseEnricher
        {
            private readonly Func<HttpResponseMessage, Exception, HttpResponseMessage> _delegate;

            public DelegateEnricher(Func<HttpResponseMessage, Exception, HttpResponseMessage> @delegate)
            {
                _delegate = @delegate;
            }

            public HttpResponseMessage Enrich(HttpResponseMessage message, Exception exception)
            {
                return _delegate(message, exception);
            }
        }

        public virtual void Do(Action<HttpResponseMessage> action)
        {
            _enrichers.Add(new DelegateEnricher((r, e) =>
            {
                action(r);
                return r;
            }));
        }

        public virtual void SetStatusCode(HttpStatusCode code)
        {
            _enrichers.Add(new StatusCodeEnricher(code));
        }

        public virtual void ReplaceResponse(HttpResponseMessage response)
        {
            _enrichers.Add(new ReplaceResponseEnricher(response));
        }

        private readonly IList<IHttpResponseEnricher> _enrichers = new List<IHttpResponseEnricher>();

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