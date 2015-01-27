using System;
using Cormo.Injects.Exceptions;

namespace Cormo.Injects.Exceptions
{
    public class InvalidComponentException: InjectionException
    {
        public InvalidComponentException(Type type, string reason) : 
            base(string.Format("Invalid component [{0}]: {1}", type, reason))
        {
        }
    }
}