using System;

namespace Alpaca.Injects.Exceptions
{
    public class InjectionException: Exception
    {
        public InjectionException(string message): base(message)
        {
            
        }
    }
}