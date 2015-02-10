using System;
using System.Reflection;

namespace Cormo.Impl.Weld
{
    public static class Formatters
    {
        public static string MultipleObservesParameter(MethodInfo methodInfo)
        {
            return string.Format("Method [{0}] has more than one parameter marked with [Observes]", methodInfo);
        }

        private static string Method(MethodInfo method)
        {
            return method.ToString();
        }

        private static string Attribute(Type type)
        {
            var name = type.Name;
            if (name.EndsWith("Attribute"))
            {
                name = name.Substring(0, name.Length - "Attribute".Length);
            }
            return "[" + name + "]";
        }
    }
}