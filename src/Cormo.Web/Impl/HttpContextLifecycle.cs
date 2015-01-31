using Cormo.Contexts;
using Cormo.Impl.Weld.Contexts;
using Cormo.Injects;
using Cormo.Web.Impl.Contexts;
using Microsoft.Owin.Extensions;
using Owin;

namespace Cormo.Web.Impl
{
    [Configuration]
    public class HttpContextLifecycle
    {
        [Inject]
        void PostConstruct(IServiceRegistry serviceRegistry, IAppBuilder appBuilder)
        {
            var requestContext = serviceRegistry.GetService<HttpRequestContext>();
            appBuilder.Use(async(context, next) =>
            {
                try
                {
                    requestContext.Activate();
                    await next();
                }
                finally
                {
                    requestContext.Deactivate();
                }
            });
        }
    }
}