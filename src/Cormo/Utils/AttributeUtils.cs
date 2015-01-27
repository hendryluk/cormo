using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Injects;

namespace Cormo.Utils
{
    public static class AttributeUtils
    {
        public static bool HasAttributeRecursive(this ICustomAttributeProvider attributeProvider, Type type)
        {
            return GetAttributesRecursive(attributeProvider, type).Any();
        }

        public static bool HasAttributeRecursive<T>(this ICustomAttributeProvider attributeProvider) where T : Attribute
        {
            return HasAttributeRecursive(attributeProvider, typeof(T));
        }

        public static IEnumerable<Attribute> GetAttributesRecursive(this ICustomAttributeProvider attributeProvider, Type type)
        {
            return GetAttributesRecursive(attributeProvider).Where(type.IsInstanceOfType);
        }

        public static IEnumerable<T> GetAttributesRecursive<T>(this ICustomAttributeProvider attributeProvider)
        {
            return GetAttributesRecursive(attributeProvider).OfType<T>();
        }

        public static IEnumerable<Attribute> GetAttributesRecursive(this IEnumerable<Attribute> attributes)
        {
            foreach (var attr in attributes)
            {
                yield return attr;
                foreach (var r in GetAttributesRecursive(attr.GetType()))
                {
                    yield return r;
                }
            }
        }

        public static IEnumerable<T> GetAttributesRecursive<T>(this IEnumerable<Attribute> attributes)
        {
            return GetAttributesRecursive(attributes).OfType<T>();
        }

        public static IEnumerable<Attribute> GetAttributesRecursive(this ICustomAttributeProvider attributeProvider)
        {
            var attributes = new HashSet<Attribute>();
            var list = new LinkedList<ICustomAttributeProvider>();
            list.AddLast(attributeProvider);

            for (var node = list.First; node != null; node = node.Next)
                foreach (var attribute in node.Value.GetCustomAttributes(true).OfType<Attribute>())
                {
                    try
                    {
                        if (!attributes.Add(attribute)) // Try catch: sometimes GetHashCode may throw exception
                            continue;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    yield return attribute;
                    list.AddLast(attribute.GetType());

                }
        }

        public static QualifierAttribute[] GetQualifiers(this ICustomAttributeProvider attributeProvider)
        {
            return (
                from attribute in attributeProvider.GetAttributesRecursive<QualifierAttribute>()
                select attribute).ToArray();
        }
    }
}