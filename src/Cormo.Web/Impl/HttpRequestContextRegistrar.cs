using System;
using System.Runtime.Remoting.Contexts;
using System.Web.Caching;
using Cormo.Injects;
using Cormo.Web.Impl.Contexts;
using Microsoft.Owin.Extensions;
using Owin;

namespace Cormo.Web.Impl
{
    [Configuration]
    public class HttpRequestContextRegistrar
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
            appBuilder.UseStageMarker(PipelineStage.Authorize);
        }
    }
}