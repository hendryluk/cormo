using System;
using Cormo.Injects.Exceptions;

namespace Cormo.Injects.Exceptions
{
    public class InvalidComponentException: InjectionException
    {
        public InvalidComponentException(string message) : base(message)
        {
            
        }
        public InvalidComponentException(Type type, string reason) : 
                this(string.Format("Invalid component [{0}]: {1}", type, reason))
        {
        }
    }
}