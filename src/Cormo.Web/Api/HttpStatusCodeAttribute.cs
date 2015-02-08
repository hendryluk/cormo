using System;
using System.Net;

namespace Cormo.Web.Api
{
    /// <summary>
    /// When declared on WebApi actions, it will set the http status code of successful responses
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpStatusCodeAttribute: Attribute
    {
        public HttpStatusCode StatusCode { get; private set; }

        public HttpStatusCodeAttribute(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}