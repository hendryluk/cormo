using System;
using Alpaca.Weld.Core;

namespace Alpaca.Weld
{
    public class InstantiationError: WeldException
    {
        public InstantiationError(Type type, string reason)
            : base(string.Format("Type cannot be instantiated: [{0}]. Reason: {1}", type.FullName, reason))
        {
        }
    }
}