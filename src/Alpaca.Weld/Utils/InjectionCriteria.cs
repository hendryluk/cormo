using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Alpaca.Weld.Attributes;
using Castle.Core.Internal;

namespace Alpaca.Weld.Utils
{
    public static class InjectionCriteria
    {
        public static bool ScanPredicate(ICustomAttributeProvider provider)
        {
            return AttributesUtil.HasAttribute<InjectAttribute>(provider);
        }
       
        public static void Validate(MemberInfo member)
        {
            var property = member as PropertyInfo;
            if (property != null && property.SetMethod == null)
            {
                throw new InjectionPointException(property, "Injection property must have a setter");
            }
        }
    }

    public static class ConfigurationCriteria
    {
        public static bool ScanPredicate(Type type)
        {
            return !type.IsAbstract && type.HasAttribute<ConfigurationAttribute>();
        }

        public static void Validate(Type type)
        {
            ComponentCriteria.Validate(type);
        }
    }

    public static class ComponentCriteria
    {
        public static void Validate(Type type)
        {
            if(!TypeUtils.HasInjectableConstructor(type))
                throw new InvalidComponentException(type, "Class does not have a parameterless constructor or an [Inject] constructor");

            if(TypeUtils.GetInjectableConstructors(type).Take(2).Count() > 1)
                throw new InvalidComponentException(type, "Class has multiple [Inject] constructors");
        }
    }
}