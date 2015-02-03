using System.ComponentModel;
using System.Linq;
using System.Web;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Cormo.Web.Api;

namespace Cormo.Web.Impl
{
    public class QueryParamProducer
    {
        [Produces, QueryParam]
        T GetQueryParam<T>(IInjectionPoint ip)
        {
            var context = HttpContext.Current;
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (context == null || converter == null)
                throw new UnsatisfiedDependencyException(ip);

            var name = GetQueryName(ip);
            var value = context.Request.QueryString[name];

            if (value == null)
                return default(T);

            return (T) converter.ConvertFromString(value);
        }

        protected string GetQueryName(IInjectionPoint ip)
        {
            var attrName = ip.Qualifiers.OfType<QueryParamAttribute>().Select(x => x.Name).SingleOrDefault();
            if (string.IsNullOrEmpty(attrName))
            {
                var methodParam = ip as IMethodParameterInjectionPoint;
                if (methodParam != null)
                    return methodParam.ParameterInfo.Name;

                return ip.Member.Name;
            }
            return attrName;
        }
    }
}