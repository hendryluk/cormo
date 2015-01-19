using System;
using Alpaca.Injects.Exceptions;

namespace Alpaca.Injects.Exceptions
{
    public class NonProxiableTypeException: InjectionException
    {
        public Type Type { get; private set; }
        public string Reason { get; private set; }

        public NonProxiableTypeException(Type type, string reason)
            : base(string.Format("Non-proxiable type: [{0}]. Reason: {1}", type.FullName, reason))
        {
            Type = type;
            Reason = reason;
        }
    }
}