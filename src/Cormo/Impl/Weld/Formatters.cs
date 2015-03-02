using System;
using System.Linq;
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

        private static string ShortType(Type type)
        {
            var str = type.Name;
            if (type.IsGenericType)
                str = FormatGenerics(str, type.GetGenericArguments());

            return str;
        }

        public static string LongType(Type type)
        {
            var str = type.FullName;
            if (type.IsGenericType)
                str = FormatGenerics(str, type.GetGenericArguments());
            
            return str;
        }

        private static string FormatGenerics(string originalName, Type[] types)
        {
            return originalName.Replace("`"+types.Length, string.Format("<{0}>", string.Join(", ", types.Select(ShortType))));
        }
        
        public static string Field(FieldInfo field)
        {
            return string.Format("{0} {1} {2}", ShortType(field.FieldType), LongType(field.DeclaringType), field.Name);
        }

        public static string Property(PropertyInfo property)
        {
            return string.Format("{0} {1} {2}", ShortType(property.PropertyType), LongType(property.DeclaringType), property.Name);
        }

        public static string Parameter(ParameterInfo parameter)
        {
            return string.Format("{0} {1}", ShortType(parameter.ParameterType), parameter.Name);
        }

        public static string DescribeMethodBase(MethodBase method)
        {
            return method is MethodInfo
                ? string.Format("method [{0}]", Method((MethodInfo) method))
                : string.Format("constructor [{0}]", Constructor((ConstructorInfo) method));
        }

        public static string Constructor(ConstructorInfo method)
        {
            var str = string.Format("{0}({1})", LongType(method.DeclaringType), string.Join(", ", method.GetParameters().Select(Parameter)));
            if (method.IsGenericMethod)
                str = FormatGenerics(str, method.GetGenericArguments());

            return str;
        }

        public static string Method(MethodInfo method)
        {
            var str = string.Format("{0} {1}::{2}({3})", ShortType(method.ReturnType), LongType(method.DeclaringType), method.Name, string.Join(", ", method.GetParameters().Select(Parameter)));
            if (method.IsGenericMethod)
                str = FormatGenerics(str, method.GetGenericArguments());

            return str;
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
            return string.Format("[Handles] must take ICaughtException<> parameter at [{0}]", parameter.Member);
        }

        public static string FormatUnproxiableType(IInjectionPoint injectionPoint, string reason)
        {
            return string.Format("Normal-scoped component must be proxyable, consider using IInstance<> instead. {0}Reason: {1}",
                    injectionPoint == null ? "" : "Injected at: " + injectionPoint + ". ",
                    reason);
        }

        public static string Member(MemberInfo member)
        {
            return MemberInfoVisitor.Visit(member, Formatters.Constructor, Formatters.Method, Formatters.Field, Formatters.Property);
        }

        public static string InvalidEventHandlingMethodReturnType(MethodInfo method)
        {
            return string.Format("Method must return void or Task [{0}]", Method(method));
        }
    }
}