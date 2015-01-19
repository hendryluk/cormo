using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;

namespace Alpaca.Web.Weld
{
    //[Configuration]
    //public class HttpContextLifecycle
    //{
    //    [Inject] HttpRequestContext _requestContext;

    //    [Inject]
    //    void PostConstruct(IAppBuilder appBuilder)
    //    {
    //        appBuilder.Use(Middleware);
    //        appBuilder.UseStageMarker(PipelineStage.Authenticate);
    //    }

    //    public Task Middleware(IOwinContext context, Func<Task> next)
    //    {
    //        // TODO
    //        return null;
    //        //_requestContext
    //    }
    //}
}