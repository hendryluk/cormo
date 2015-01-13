using System.Reflection;

namespace Alpaca.Inject.Exceptions
{
    public class InjectionPointException: InjectionException
    {
        public InjectionPointException(MemberInfo memberInfo, string reason, params object[] args) 
            : base(string.Format("Cannot inject to member [{0}], reason: {1}", memberInfo, string.Format(reason, args)))
        {
            
        }
    }
}