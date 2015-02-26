using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Weld;
using Cormo.Injects;

namespace Cormo.Impl.Utils
{
    public static class AttributeUtils
    {
        public static bool HasAttributeRecursive(this ICustomAttributeProvider attributeProvider, Type type)
        {
            return GetAttributesRecursive(attributeProvider, type).Any();
        }

        public static bool HasAttribute<T>(this ICustomAttributeProvider attributeProvider)
        {
            return attributeProvider.GetCustomAttributes(typeof(T), true).Any();
        }

        public static bool HasAttributeRecursive<T>(this ICustomAttributeProvider attributeProvider)
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

        public static IEnumerable<T> GetAttributesRecursive<T>(this IEnumerable<Attribute> attributes) where T:IBinderAttribute
        {
            return GetAttributesRecursive(attributes).OfType<T>();
        }

        public static IEnumerable<Attribute> GetAttributesRecursive(this IEnumerable<Attribute> attributes)
        {
            var attributeSet = new HashSet<Attribute>();
            var list = new LinkedList<IEnumerable<Attribute>>();
            list.AddLast(attributes);

            for (var node = list.First; node != null; node = node.Next)
                foreach (var attribute in node.Value)
                {
                    try
                    {
                        if (!attributeSet.Add(attribute)) // Try catch: sometimes GetHashCode may throw exception
                            continue;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    yield return attribute;

                    if (attribute is IStereotype)
                        list.AddLast(((IStereotype)attribute).Attributes);
                }
        }

        public static IEnumerable<Attribute> GetAttributesRecursive(this ICustomAttributeProvider attributeProvider)
        {
            return GetAttributesRecursive(attributeProvider.GetCustomAttributes(true).OfType<Attribute>());
        }

        public static IBinders GetBinders(this ICustomAttributeProvider attributeProvider)
        {
            return new Binders(
                from attribute in attributeProvider.GetAttributesRecursive<IBinderAttribute>()
                select attribute);

        }
    }
}