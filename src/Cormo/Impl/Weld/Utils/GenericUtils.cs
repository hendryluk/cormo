using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using Cormo.Reflects;

namespace Cormo.Impl.Weld.Utils
{
    public static class GenericUtils
    {
        public static Type TranslateGenericArguments(Type type, IDictionary<Type, Type> typeTransations)
        {
            if (!type.ContainsGenericParameters)
                return type;
            var args = type.GetGenericArguments().Select(x => TranslateGenericArgument(x, typeTransations)).ToArray();
            if (args.Contains(null))
                return null;
            try
            {
                return type.GetGenericTypeDefinition().MakeGenericType(args);
            }
            catch (ArgumentException)
            {
                // Incomatible constraint
                return type;
            }
        }

        private static Type TranslateGenericArgument(Type arg, IDictionary<Type, Type> typeTransations)
        {
            if (arg.IsGenericParameter)
            {
                Type translated;
                if (typeTransations.TryGetValue(arg, out translated))
                    return translated;
                return null;
            }

            if (arg.ContainsGenericParameters)
            {
                return TranslateGenericArguments(arg, typeTransations);
            }
            return arg;
        }

        public static bool MemberContainsGenericArguments(MemberInfo member)
        {
            if (member.ReflectedType.ContainsGenericParameters)
                return true;
            var method = member as MethodInfo;
            if (method != null)
            {
                return method.ContainsGenericParameters;
            }
            return false;
        }

        public static ConstructorInfo TranslateConstructorGenericArguments(ConstructorInfo ctor, IDictionary<Type, Type> typeTranslations)
        {
            var translatedType = TranslateGenericArguments(ctor.ReflectedType, typeTranslations);
            if (translatedType == null)
                return null;

            var translatedMethodParameters = ctor.GetParameters().Select(x => TranslateGenericArgument(x.ParameterType, typeTranslations)).ToArray();

            if (ctor.ReflectedType != translatedType)
                ctor = translatedType.GetConstructor(translatedMethodParameters);

            return ctor;
        }

        public static MethodInfo TranslateMethodGenericArguments(MethodInfo method, IDictionary<Type, Type> typeTranslations)
        {
            var translatedType = TranslateGenericArguments(method.ReflectedType, typeTranslations);
            if (translatedType == null)
                return null;

            var translatedMethodParameters = method.GetParameters().Select(x => TranslateGenericArgument(x.ParameterType, typeTranslations)).ToArray();

            if (method.ReflectedType != translatedType)
                method = translatedType.GetMethod(method.Name, translatedMethodParameters);

            if (method.ContainsGenericParameters)
            {
                var translatedMethodGenericArgs = method.GetGenericArguments().Select(x => TranslateGenericArgument(x, typeTranslations));
                try
                {
                    return method.GetGenericMethodDefinition().MakeGenericMethod(translatedMethodGenericArgs.ToArray());
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }

            return method;
        }

        public static FieldInfo TranslateFieldType(FieldInfo field, IDictionary<Type, Type> typeTranslations)
        {
            var translatedType = TranslateGenericArguments(field.ReflectedType, typeTranslations);
            if (translatedType == null)
                return null;

            var flag = (field.IsStatic ? BindingFlags.Static : BindingFlags.Instance);
            flag |= (field.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic);

            return translatedType.GetField(field.Name, flag);
        }

        public static PropertyInfo TranslatePropertyType(PropertyInfo property, IDictionary<Type, Type> typeTranslations)
        {
            var translatedType = TranslateGenericArguments(property.ReflectedType, typeTranslations);
            if (translatedType == null)
                return null;

            return translatedType.GetProperty(property.Name, 
                TranslateGenericArgument(property.PropertyType, typeTranslations),
                property.GetIndexParameters().Select(x => TranslateGenericArgument(x.ParameterType, typeTranslations)).ToArray());
        }

        public static Type OpenIfGeneric(Type type)
        {
            if (!type.IsGenericType)
                return type;
            return type.GetGenericTypeDefinition();
        }
    }
}