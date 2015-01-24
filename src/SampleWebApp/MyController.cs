using System.Collections.Generic;
using System.Web.Http;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Web.Attributes;

namespace SampleWebApp
{
    [RestController]
    public class MyController    
    // Inheriting ApiController or IHttpController is optional. Alpaca.Web will inject that for you.
    // This promotes DI principle and lightweight components.
    {
        [Inject] IGreeter<string> _stringService;                // -> Resolves to UpperCaseGreeter
        [Inject] IGreeter<IEnumerable<int>> _integersService;     // -> Resolves to EnumerableGreeter<int>
        
        [Route("test"), HttpGet]
        public string Test()
        {
            return _stringService.Greet("World");
        }

        [Route("testMany"), HttpGet]
        public string TestMany()
        {
            return _integersService.Greet(new []{1,2,3,4,5});
        }
    }

    // ============= SERVICES BELOW ===============
    public interface IGreeter<T>
    {
        string Greet(T val);
    }

    [Singleton]
    public class UpperCaseGreeter : IGreeter<string>
    {
        public string Greet(string val)
        {
            return string.Format("Hello {0} ({1})", val.ToUpper(), GetHashCode());
        }
    }

    public class EnumerableeGreeter<T>: IGreeter<IEnumerable<T>>
    {
        public string Greet(IEnumerable<T> vals)
        {
            return string.Format("Hello many {0} ({1})", string.Join(",", vals), GetHashCode());
        }
    }
}