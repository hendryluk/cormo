using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Web.Api;

namespace SampleWebApp
{
    [RestController]
    public class MyController
    // Inheriting ApiController or IHttpController is optional. Cormo.Web will inject that for you.
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
        [Inject, HeaderParam] private string Accept;
        
        public string Greet(string val)
        {
            return string.Format("Hello {0} ({1}). Accept: {2}", val.ToUpper(), GetHashCode(), Accept);
        }
    }

    public class EnumerableeGreeter<T>: IGreeter<IEnumerable<T>>, IDisposable
    {
        public string Greet(IEnumerable<T> vals)
        {
            return string.Format("Hello many {0} ({1})", string.Join(",", vals), GetHashCode());
        }


        public void Dispose()
        {
            // Clear some resources here
            Debug.WriteLine("Disposed EnumerableeGreeter: " + this);
        }
    }
}