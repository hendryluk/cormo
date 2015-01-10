using System.Reflection;

namespace Alpaca.Weld
{
    public class InjectionPointException: WeldException
    {
        public InjectionPointException(MemberInfo memberInfo, string reason, params object[] args) 
            : base(string.Format("Cannot inject to member [{0}], reason: {1}", memberInfo, string.Format(reason, args)))
        {
            
        }
    }
}