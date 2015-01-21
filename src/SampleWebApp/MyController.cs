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
using Alpaca.Web.Attributes;

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

    [RestController]
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
        public string Test()
        {
            return _stringService.Greet("World") + " --> by " + GetType();
        }

        [Route("testMany"), HttpGet]
        public string TestMany()
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