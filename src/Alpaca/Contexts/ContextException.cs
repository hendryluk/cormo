using System;

namespace Alpaca.Contexts
{
    public class ContextException : Exception
    {
        public ContextException(string message): base(message)
        {
            
        }
    }
}