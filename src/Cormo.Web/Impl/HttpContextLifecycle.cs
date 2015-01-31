using System;
using System.Web;
using System.Web.Caching;
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
            var sessionContext = serviceRegistry.GetService<HttpSessionContext>();
            
            appBuilder.Use(async(context, next) =>
            {
                try
                {
                    if(sessionContext.IsActive)
                        sessionContext.Activate();

                    requestContext.Activate();
                    await next();
                }
                finally
                {
                    requestContext.Deactivate();
                }
            });
            appBuilder.UseStageMarker(PipelineStage.Authenticate);
        }
    }
}