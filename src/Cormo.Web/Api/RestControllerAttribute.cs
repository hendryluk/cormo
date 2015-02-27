using Cormo.Injects;
using Cormo.Mixins;

namespace Cormo.Web.Api
{
    [Default]
    public class RestControllerAttribute : StereotypeAttribute, IQualifier, IMixinBinding
    {

    }
}