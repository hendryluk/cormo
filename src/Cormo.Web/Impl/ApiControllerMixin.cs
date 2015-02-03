using System.Web.Http;
using System.Web.Http.Controllers;
using Cormo.Contexts;
using Cormo.Mixins;
using Cormo.Web.Api;

namespace Cormo.Web.Impl
{
    [RestController]
    [RequestScoped]
    [Mixin(typeof(IHttpController), typeof(ICormoHttpController))]
    public class ApiControllerMixin : ApiController, ICormoHttpController
    {
        
    }

    public interface ICormoHttpController
    {
    }
}