using System.Reflection;
using System.Threading.Tasks;
using Cormo.Injects.Exceptions;

namespace Cormo.Impl.Weld.Utils
{
    public class EventHandlerValidator
    {
        public void ValidateEventHandlerMethod(MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (returnType != typeof (void) && returnType != typeof (Task))
            {
                throw new InvalidComponentException(method.DeclaringType, Formatters.InvalidEventHandlingMethodReturnType(method));
            }
        }
    }
}