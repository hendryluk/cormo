using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;
using Cormo.Web.Api;

namespace Cormo.Web.Impl
{
    [Provider]
    public class StatusCodeFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            if (context.Exception == null)
            {
                var statusCodeAttribute = context.ActionContext.Request.GetActionDescriptor().GetCustomAttributes<HttpStatusCodeAttribute>().FirstOrDefault();
                if (statusCodeAttribute != null)
                    context.Response.StatusCode = statusCodeAttribute.StatusCode;
            }
        }
    }
}