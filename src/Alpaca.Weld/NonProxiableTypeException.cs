using System;
using Alpaca.Weld.Core;

namespace Alpaca.Weld
{
    public class NonProxiableTypeException: WeldException
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