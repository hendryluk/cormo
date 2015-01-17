using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;

namespace Alpaca.Weld.Utils
{
    public static class ScannerUtils
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types;
            }
        }

        public static bool HasAttribute<T>(this ICustomAttributeProvider attributeProvider) where T : Attribute
        {
            return GetRecursiveAttributes<T>(attributeProvider).Any();
        }
        public static IEnumerable<T> GetRecursiveAttributes<T>(this ICustomAttributeProvider attributeProvider) where T : Attribute
        {
            return GetRecursiveAttributes(attributeProvider).OfType<T>();
        }

        public static IEnumerable<Attribute> GetRecursiveAttributes(this ICustomAttributeProvider attributeProvider)
        {
            var attributes = new HashSet<Attribute>();
            var list = new LinkedList<ICustomAttributeProvider>();
            list.AddLast(attributeProvider);

            for(var node= list.First; node!=null; node = node.Next)
            foreach (var attribute in node.Value.GetCustomAttributes(true).OfType<Attribute>())
            {
                try
                {
                    if (!attributes.Add(attribute)) // Sometimes GetHashCode may throw exception
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
                from attribute in attributeProvider.GetRecursiveAttributes<QualifierAttribute>()
                select attribute).ToArray();
        }
    }
}