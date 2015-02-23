using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Injects.Exceptions;

namespace Cormo.Impl.Solder
{
    [Singleton]
    public class ValueProvider
    {
        protected string GetValueName(IInjectionPoint ip)
        {
            var attrName = ip.Qualifiers.OfType<ValueAttribute>().Select(x => x.Name).SingleOrDefault();
            if (string.IsNullOrEmpty(attrName))
            {
                var methodParam = ip as IMethodParameterInjectionPoint;
                if (methodParam != null)
                    return methodParam.ParameterInfo.Name;

                return ip.Member.Name;
            }
            return attrName;
        }

        [Produces, Value]
        public T GetValue<T>(IInjectionPoint injectionPoint, IInstance<IValueProvider> providers)
        {
            if (injectionPoint == null)
                throw new InjectionException("Value needs injection point");
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
                throw new UnsatisfiedDependencyException(injectionPoint);

            var name = GetValueName(injectionPoint);

            foreach (var value in providers.OrderByDescending(x=> x.Priority)
                .Select(x=> x.GetValue(name))
                .Where(x=> x!=null))
            {
                try
                {
                    return (T)converter.ConvertFromString(value);
                }
                catch (Exception e)
                {
                    // TODO log
                }
            }

            return GetDefaultValue<T>(injectionPoint);
        }

        protected T GetDefaultValue<T>(IInjectionPoint ip)
        {
            var attrDefault = ip.Qualifiers.OfType<ValueAttribute>().Select(x => x.Default).OfType<T>().ToArray();
            if (attrDefault.Any())
                return attrDefault[0];

            return default(T);
        }
    }

    public class AppSettingsValueProvider: IValueProvider
    {
        public const int PRIORITY = 100;

        public string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? GetFromConnectionString(key);
        }

        public int Priority { get { return PRIORITY; } }

        private string GetFromConnectionString(string key)
        {
            var connection = ConfigurationManager.ConnectionStrings[key];
            return connection == null ? null : connection.ConnectionString;
        }
    }
}