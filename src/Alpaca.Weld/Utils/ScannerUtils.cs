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
    }
}