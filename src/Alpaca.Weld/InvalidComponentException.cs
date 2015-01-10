using System;

namespace Alpaca.Weld
{
    public class InvalidComponentException: WeldException
    {
        public InvalidComponentException(Type type, string reason) : 
            base(string.Format("Invalid component [{0}]: {1}", type, reason))
        {
        }
    }
}