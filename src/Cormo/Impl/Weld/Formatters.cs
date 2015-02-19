using System;
using System.Reflection;
using Cormo.Injects;

namespace Cormo.Impl.Weld
{
    public static class Formatters
    {
        public static string MultipleObservesParameter(MethodInfo methodInfo)
        {
            return string.Format("Method [{0}] has more than one parameter marked with [Observes]", methodInfo);
        }

        public static string MultipleHandlesParameter(MethodInfo methodInfo)
        {
            return string.Format("Method [{0}] has more than one parameter marked with [Handles]", methodInfo);
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

        public static string WrongHandlesParamType(ParameterInfo parameter)
        {
            return string.Format("[Handles] must take CaughtException<> parameter at [{0}]", parameter.Member);
        }

        public static string FormatUnproxiableType(IInjectionPoint injectionPoint, string reason)
        {
            return string.Format("Normal-scoped component must be proxyable, consider using IInstance<> instead. {0}Reason: {1}",
                    injectionPoint == null ? "" : "Injected at: " + injectionPoint + ". ",
                    reason);
        }

        public static object Format(MemberInfo method)
        {
            // TODO: make it pretty
            return method.ToString();
        }
    }
}