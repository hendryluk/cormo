using System;
using System.Collections.Generic;
using System.Web.Http;
using Alpaca.Injects;

namespace SampleWebApp
{
    public class MyController : ApiController
    {
        [Inject]
        IGreeter<string> _stringService;                // -> UpperCaseGreeter
        [Inject]
        IGreeter<IEnumerable<int>> _integersService;    // -> EnumerableeGreeter<int>

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