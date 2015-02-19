using System;
using System.Net;
using Cormo.Interceptions;

namespace Cormo.Web.Api
{
    /// <summary>
    /// When a method decorated with this attribute is called, it will set the status code of WebApi response with the specified value
    /// </summary>
    public class HttpStatusCodeAttribute: InterceptorBindingAttribute
    {
        public HttpStatusCode StatusCode { get; private set; }

        public HttpStatusCodeAttribute(): this(HttpStatusCode.OK)
        {
        }

        public HttpStatusCodeAttribute(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}