using System;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Cormo.Web.Api;

namespace Cormo.Web.Impl
{
    public class HeaderParamProducer
    {
        [Produces]
        [HeaderParam]
        protected T GetHeaderParam<T>(IInjectionPoint ip)
        {
            if (ip == null)
                throw new InjectionException("HeaderParam needs injection point");

            var context = HttpContext.Current;
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (context == null || converter == null)
                throw new UnsatisfiedDependencyException(ip);

            try
            {
                var header = context.Request.Headers.Get(GetHeaderName(ip));
                if (header != null)
                    return (T) converter.ConvertFromString(header);
            }
            catch (Exception)
            {
                // TODO: LOG
            }

            return GetDefaultValue<T>(ip);
        }

        protected string GetHeaderName(IInjectionPoint ip)
        {
            var attrName = ip.Qualifiers.OfType<HeaderParamAttribute>().Select(x => x.Name).SingleOrDefault();
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
            var attrDefault = ip.Qualifiers.OfType<HeaderParamAttribute>().Select(x => x.Default).OfType<T>().ToArray();
            if (attrDefault.Any())
                return attrDefault[0];

            return default(T);
        }
    }
}