using System;
using Alpaca.Injects.Exceptions;

namespace Alpaca.Injects.Exceptions
{
    public class CreationException: InjectionException
    {
        public CreationException(Type type, string reason)
            : base(string.Format("Type cannot be instantiated: [{0}]. Reason: {1}", type.FullName, reason))
        {
        }
    }
}