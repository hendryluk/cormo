using System;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using Alpaca.Weld.Utils;
using Castle.Core.Internal;

namespace Alpaca.Weld.Utils
{
    public static class InjectionCriteria
    {
        public static bool ScanPredicate(ICustomAttributeProvider provider)
        {
            return AttributesUtil.HasAttribute<InjectAttribute>(provider);
        }
        public static void Validate(FieldInfo field)
        {
        }

        public static void Validate(MethodBase method)
        {
            if (method.IsGenericMethodDefinition)
            {
                throw new InjectionPointException(method, "Cannot inject into a generic method");
            }
        }
       
        public static void Validate(PropertyInfo property)
        {
            if (property.SetMethod == null)
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
            if(type.ContainsGenericParameters)
                throw new InvalidComponentException(type, "Configuration class cannot contain generic parameters");
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

    public static class PostConstructCriteria
    {
        public static void Validate(MethodInfo method)
        {
            if (method.IsGenericMethod)
                throw new InvalidComponentException(method.ReflectedType, string.Format("PostConstruct method must not be generic: [{0}]", method));

            if (method.GetParameters().Any())
                throw new InvalidComponentException(method.ReflectedType, string.Format("PostConstruct method must not have any parameter: [{0}]", method));

        }
    }
}