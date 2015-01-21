using System.Web.Http;
using Alpaca.Injects;
using Alpaca.Web.Attributes;

namespace Alpaca.Web.WebApi
{
    [RestController]
    [Mixin]
    public class ApiControllerMixin : ApiController
    {

    }
}