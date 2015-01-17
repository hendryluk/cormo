using System.Reflection;

namespace Alpaca.Injects.Exceptions
{
    public class InvalidMethodSignatureException: InjectionException
    {
        public MethodBase Method { get; private set; }

        public InvalidMethodSignatureException(MethodBase method, string message, params object[] args) 
            : base(string.Format("Invalid method signature: [{0}], {1}", string.Format(message, args)))
        {
            Method = method;
        }
    }
}