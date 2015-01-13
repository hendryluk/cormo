using System;

namespace Alpaca.Inject.Exceptions
{
    public class InjectionException: Exception
    {
        public InjectionException(string message): base(message)
        {
            
        }
    }
}