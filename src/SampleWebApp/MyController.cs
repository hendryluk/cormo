using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.ModelBinding;
using Cormo.Contexts;
using Cormo.Data.EntityFramework.Api;
using Cormo.Injects;
using Cormo.Web.Api;
using Owin;

[assembly:EnableHttpSessionState]

namespace SampleWebApp
{
    [RestController]
    public class MyController
    // Inheriting ApiController or IHttpController is optional. Cormo.Web will inject that for you.
    // This promotes DI principle and lightweight components.
    {
        [Inject] IGreeter<string> _stringService;                // -> Resolves to UpperCaseGreeter
        [Inject] IGreeter<IEnumerable<int>> _integersService;     // -> Resolves to EnumerableGreeter<int>

        [Route("test/{id}"), HttpGet]
        public string TestWithId(HttpRequestMessage msg)
        {
            return _stringService.Greet("World") + "/" + GetHashCode();
        }

        [Route("test"), HttpGet]
        public string Test(HttpRequestMessage msg)
        {
            return _stringService.Greet("World") + "/" + GetHashCode();
        }

        [Route("testMany"), HttpGet]
        public string TestMany()
        {
            return _integersService.Greet(new[] { 1, 2, 3, 4, 5 });
        }
    }

    // ============= SERVICES BELOW ===============
    public interface IGreeter<T>
    {
        string Greet(T val);
    }

    [Value("limit", Default = 50)]
    public class LimitAttribute : StereotypeAttribute
    {
    }

    [RequestScoped]
    public class UpperCaseGreeter : IGreeter<string>, IDisposable
    {
        [Inject, HeaderParam] string Accept;
        [Inject] IDbSet<Person> _persons;
        [Inject] private IPrincipal _principal;

        [Inject, RouteParam]
        private int id;

        [Inject, Limit] private int xxxx;

        public virtual string Greet(string val)
        {
            return string.Format("Hello {0} ({1}). Count: {2}. Accept: {3}", val.ToUpper(), 
                _principal.Identity, 
                _persons.Count(), 
                Accept) + "/" + id;
        }

        public void Dispose()
        {
            // Clear some resources here
            Debug.WriteLine("Disposed EnumerableeGreeter: " + this);
        }
    }

    [FromBody]
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    [SessionScoped]
    public class EnumerableeGreeter<T>: IGreeter<IEnumerable<T>>
    {
        public string Greet(IEnumerable<T> vals)
        {
            return string.Format("Hello many {0} ({1})", string.Join(",", vals), GetHashCode());
        }
    }

    [Configuration]
    public class MyConfig
    {
        [Inject]
        void GetConfig(HttpConfiguration config, IAppBuilder builder)
        {
            config.Routes.MapHttpRoute("api", "api/{controller}");
            builder.UseWebApi(config);
        }
    }
}