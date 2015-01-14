using System;

namespace Alpaca.Injects.Exceptions
{
    public class InvalidComponentException: InjectionException
    {
        public InvalidComponentException(Type type, string reason) : 
            base(string.Format("Invalid component [{0}]: {1}", type, reason))
        {
        }
    }
}