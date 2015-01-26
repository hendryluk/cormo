using System.ComponentModel;
using System.Linq;
using System.Web;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using Alpaca.Web.Api;

namespace Alpaca.Web.Impl
{
    public class CookieParamProducer
    {
        [Produces]
        [CookieParam]
        protected T GetCookieParam<T>(IInjectionPoint ip)
        {
            if(ip == null)
                throw new InjectionException("Cookie param must be injected statically");

            var context = HttpContext.Current;
            var converter = TypeDescriptor.GetConverter(typeof (T));
            if(context == null || converter == null)
                throw new UnsatisfiedDependencyException(ip);

            var httpCookie = context.Request.Cookies.Get(GetCookieName(ip));
            if (httpCookie == null)
                return GetDefaultValue<T>(ip);

            var cookieValue = httpCookie.Value;
            return (T) converter.ConvertFromString(cookieValue);
        }

        protected string GetCookieName(IInjectionPoint ip)
        {
            var attrName = ip.Qualifiers.OfType<CookieParamAttribute>().Select(x => x.Name).SingleOrDefault();
            if (string.IsNullOrEmpty(attrName))
            {
                var methodParam = ip as IMethodParameterInjectionPoint;
                if (methodParam != null)
                    return methodParam.ParameterInfo.Name;

                return ip.Member.Name;
            }
            return attrName;
        }

        protected T GetDefaultValue<T>(IInjectionPoint ip)
        {
            var attrDefault = ip.Qualifiers.OfType<CookieParamAttribute>().Select(x => x.Default).OfType<T>().ToArray();
            if (attrDefault.Any())
                return attrDefault[0];

            return default(T);
        }
    }
}