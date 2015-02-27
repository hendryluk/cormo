using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;
using Cormo.Impl.Utils;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Cormo.Reflects;

namespace Cormo.Impl.Weld.Utils
{
    public static class InjectionValidator
    {
        public static bool ScanPredicate(IAnnotated provider)
        {
            return provider.Annotations.OfType<InjectAttribute>().Any();
        }

        public static bool ScanPredicate(IAnnotatedProperty property)
        {
            return ScanPredicate((IAnnotated)property) &&
                   (property.Property.SetMethod == null || property.Property.SetMethod.IsAbstract);
        }
        public static void Validate(IAnnotatedField field)
        {
        }

        public static void Validate(IAnnotatedMethod method)
        {
            if (method.Method.IsGenericMethodDefinition)
            {
                throw new InjectionPointException(method.Method, "Cannot inject into a generic method");
            }
        }
       
        public static void Validate(IAnnotatedProperty property)
        {
            if (property.Property.SetMethod == null)
            {
                throw new InjectionPointException(property.Property, "Injection property must have a setter");
            }
        }

        private static readonly ConcurrentDictionary<Type, string> _checkedProxyableTypes = new ConcurrentDictionary<Type, string>();
        public static void ValidateProxiable(Type type, IInjectionPoint injectionPoint)
        {
            var error = CheckProxiable(type);

            if (error != null)
            {
                var message = Formatters.FormatUnproxiableType(injectionPoint, error);
                throw new NonProxiableTypeException(type, message);
            }
        }

        public static string CheckProxiable(Type type)
        {
            var error = _checkedProxyableTypes.GetOrAdd(type, _ =>
            {
                if (type.IsInterface /* || typeof (MulticastDelegate).IsAssignableFrom(type.BaseType) */)
                {
                    return null;
                }

                var builder = new StringBuilder();
                if (type.IsValueType)
                {
                    builder.Append("value type");
                }

                if (type.IsPrimitive)
                {
                    builder.Append("primitive type");
                }

                if (type.IsSealed)
                {
                    builder.Append("class is sealed");
                }

                if (!TypeUtils.HasAccessibleDefaultConstructor(type))
                {
                    builder.Append("No public/protected parameterless constructor");
                }

                var sealedMembers = TypeUtils.GetSealedPublicMembers(type);
                if (sealedMembers.Any())
                {
                    builder.Append(
                        string.Format("These public members must be virtual: {0}",
                            string.Join(",/n", sealedMembers.Select(Formatters.Member))));
                }

                var publicFields = TypeUtils.GetPublicFields(type);
                if (publicFields.Any())
                {
                    builder.Append(
                        string.Format("Must not have public fields: {0}",
                            string.Join(",/n", publicFields.Select(x => x.ToString()))));
                }

                if (builder.Length == 0)
                    return null;
                return builder.ToString();
            });
            return error;
        }
    }

    public static class ConfigurationCriteria
    {
        public static bool ScanPredicate(Type type)
        {
            return !type.IsAbstract && type.HasAttributeRecursive<ConfigurationAttribute>();
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

    public static class MixinCriteria
    {
        public static void Validate(Type type)
        {
            if(type.IsGenericType)
                throw new InvalidComponentException(type, "Mixin cannot be a generic class");
        }
    }

    public static class PostConstructCriteria
    {
        public static void Validate(MethodInfo method)
        {
            if (method.IsGenericMethod)
                throw new InvalidMethodSignatureException(method, "PostConstruct method must not be generic");

            if (method.GetParameters().Any())
                throw new InvalidMethodSignatureException(method, "PostConstruct method must not have any parameter");

        }
    }
}