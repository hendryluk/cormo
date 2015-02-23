using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cormo.Impl.Weld.Utils
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


        public static IEnumerable<FieldInfo> GetAllField(Type type, BindingFlags bindingFlags)
        {
            bindingFlags |= BindingFlags.DeclaredOnly;

            var fields = Enumerable.Empty<FieldInfo>();
            for (; type != null; type = type.BaseType)
            {
                fields = fields.Concat(type.GetFields(bindingFlags));
            }

            return fields;
        }
    }
}