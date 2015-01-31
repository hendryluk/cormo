using System.Web;
using System.Web.SessionState;
using Cormo.Injects;
using Cormo.Web.Impl.Contexts;
using Microsoft.Owin.Extensions;
using Owin;

namespace Cormo.Web.Impl
{
    public class HttpSessionContextRegistrar
    {
        [Inject] void Configure(IServiceRegistry serviceRegistry, IAppBuilder appBuilder)
        {
            appBuilder.Use((context, next) =>
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
                return next();
            });
            appBuilder.UseStageMarker(PipelineStage.MapHandler);

            var sessionContext = serviceRegistry.GetService<HttpSessionContext>();
            appBuilder.Use((context, next) =>
            {
                if (HttpContext.Current.Session != null && !sessionContext.IsActive)
                    sessionContext.Activate();
                return next();
            });
        }
    }
}