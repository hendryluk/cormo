using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Alpaca.Injects;

namespace SampleWebApp
{
    //public class AlpacaActionSelector : IHttpActionSelector
    //{
    //    public HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public ILookup<string, HttpActionDescriptor> GetActionMapping(HttpControllerDescriptor controllerDescriptor)
    //    {
    //        return Lookup<>
    //    }
    //}

    public class AlpacaControllerSelector : IHttpControllerSelector
    {
        public static HttpControllerDescriptor _descriptor;
        private HttpControllerDescriptor _proxyDescriptor;

        [Inject]
        public AlpacaControllerSelector(HttpConfiguration httpConfiguration)
        {
            _proxyDescriptor = new HttpControllerDescriptor(httpConfiguration, "ProxyController",
                typeof (MyGlobalController));
            _descriptor = new HttpControllerDescriptor(httpConfiguration, "MyController",
                typeof(MyController));
            
        }

        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            return _descriptor;
        }

        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return new Dictionary<string, HttpControllerDescriptor>
            {
                {"MyController", _descriptor}, {"ProxyController", _proxyDescriptor}
            };
        }
    }

    public class MyGlobalController: IHttpController
    {
        [Inject] private IComponentManager _manager;

        public async Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            var services = controllerContext.ControllerDescriptor.Configuration.Services;
            var route = (IHttpRouteData[])controllerContext.RouteData.Values["MS_SubRoutes"];
            var actions = (HttpActionDescriptor[]) route[0].Route.DataTokens["actions"];

            
            var xx= await actions[0].ExecuteAsync(controllerContext, new Dictionary<string, object>(), cancellationToken);
            //var action = ServicesExtensions.GetActionSelector(services).SelectAction(controllerContext);
            return null;
        }
    }

    public class MyController
    {
        private readonly IGreeter<IEnumerable<int>> _integersService;

        [Inject]
        public MyController(IGreeter<IEnumerable<int>> integersService)
        {
            _integersService = integersService;
        }

        [Inject]
        IGreeter<string> _stringService;                // -> UpperCaseGreeter
        
        [Route("test"), HttpGet]
        public virtual string Test()
        {
            return _stringService.Greet("World");
        }

        [Route("testMany"), HttpGet]
        public virtual string TestMany()
        {
            return _integersService.Greet(new []{1,2,3,4,5});
        }
    }

    public interface IGreeter<T>
    {
        string Greet(T val);
    }

    public class UpperCaseGreeter : IGreeter<string>
    {
        public string Greet(string val)
        {
            return "Hello " + val.ToUpper();
        }
    }

    public class EnumerableeGreeter<T>: IGreeter<IEnumerable<T>>
    {
        public string Greet(IEnumerable<T> vals)
        {
            return "Hello many " + string.Join(",", vals);
        }
    }
}