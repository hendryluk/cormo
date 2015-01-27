using System;

namespace Cormo.Injects.Exceptions
{
    public class InjectionException: Exception
    {
        public InjectionException(string message): base(message)
        {
            
        }
    }
}