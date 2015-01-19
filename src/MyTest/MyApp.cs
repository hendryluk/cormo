using System.Web.Http;
using Alpaca.Inject;

namespace MyTest
{
    public class GreetingService
    {
        public string Greet(string name)
        {
            return "Hello " + name;
        }
    }

    public class MyController : ApiController
    {
        [Inject] private GreetingService _service;

        [Route("test"), HttpGet]
        public string Test()
        {
            return _service.Greet("World");
        }
    }
}