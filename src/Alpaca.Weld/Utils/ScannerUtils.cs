using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            return GetAttributes<T>(attributeProvider).Any();
        }
        public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider attributeProvider) where T : Attribute
        {
            return GetAttributes(attributeProvider).OfType<T>();
        }

        public static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider attributeProvider)
        {
            var attributes = attributeProvider.GetCustomAttributes(true);
            foreach (var attribute in attributes.OfType<Attribute>())
            {
                yield return attribute;
                foreach (var chainedAttribute in GetAttributes(attribute.GetType()))
                {
                    yield return chainedAttribute;
                }
            }
        } 
    }
}