using System;
using Cormo.Injects.Exceptions;

namespace Cormo.Interceptions
{
    public class NotInterceptableException: InvalidComponentException
    {
        public NotInterceptableException(string message) : base(message)
        {
        }
    }
}