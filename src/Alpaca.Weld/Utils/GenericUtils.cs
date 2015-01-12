using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Alpaca.Weld.Utils
{
    public static class GenericUtils
    {
        public class Resolution
        {
            public Type ResolvedType { get; set; }
            public IDictionary<Type, Type> GenericParameterTranslations { get; set; } 
        }

        public static IDictionary<Type, Type> CreateGenericTranslactions(Type type)
        {
            var args = type.GetGenericArguments();
            var openType = type.GetGenericTypeDefinition();

            return openType.GetGenericArguments()
                .Select((x, i) => new {x,i})
                .ToDictionary(x => x.x, x => args[x.i]);
        }

        public static Resolution ResolveGenericType(Type component, Type requestedType)
        {
            Type resolvedType;
            var typeTransations = new Dictionary<Type, Type>();

            if (requestedType.IsAssignableFrom(component))
                resolvedType = component;
            else if (!component.ContainsGenericParameters)
                return null;
            else
            {
                var openRequestedType = OpenIfGeneric(requestedType);
                var interfaces = TypeUtils.GetComponentTypes(component);

                var resolvedComponentType = (from i in interfaces
                        where openRequestedType == OpenIfGeneric(i)
                        let closedType = CloseGenericType(i, requestedType, typeTransations)
                        where closedType != null
                        select closedType)
                        .FirstOrDefault();

                if (resolvedComponentType == null)
                    return null;

                resolvedType = TranslateGenericArguments(component, typeTransations);
            }

            return new Resolution
            {
                ResolvedType = resolvedType,
                GenericParameterTranslations = typeTransations
            };
        }

        private static Type TranslateGenericArguments(Type type, IDictionary<Type, Type> typeTransations)
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

        public static MemberInfo TranslateMemberGenericArguments(MemberInfo member, IDictionary<Type, Type> typeTranslations)
        {
            var type = TranslateGenericArguments(member.ReflectedType, typeTranslations);
            if (type == null)
                return null;

            var method = member as MethodInfo;
            if (method != null)
            {
                if (!method.ContainsGenericParameters && method.ReflectedType == type)
                    return method;

                return TranslateMethodGenericArguments(type, method, typeTranslations);
            }

            if (type == member.ReflectedType)
                return member;

            var field = member as FieldInfo;
            if (field != null)
                return TranslateFieldType(type, field);

            var property = member as PropertyInfo;
            if (property != null)
                return TranslatePropertyType(type, property, typeTranslations);

            return null;
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

        private static MethodInfo TranslateMethodGenericArguments(Type translatedType, MethodInfo method, IDictionary<Type, Type> typeTranslations)
        {
            var translatedMethodParameters = method.GetParameters().Select(x => TranslateGenericArgument(x.ParameterType, typeTranslations)).ToArray();

            if (method.ReflectedType != translatedType)
                method = translatedType.GetMethod(method.Name, translatedMethodParameters);

            if (method.ContainsGenericParameters)
            {
                var translatedMethodGenericArgs = method.GetGenericArguments().Select(x => TranslateGenericArgument(x, typeTranslations));
                return method.GetGenericMethodDefinition().MakeGenericMethod(translatedMethodGenericArgs.ToArray());
            }

            return method;
        }

        private static FieldInfo TranslateFieldType(Type translatedType, FieldInfo field)
        {
            var flag = (field.IsStatic ? BindingFlags.Static : BindingFlags.Instance);
            flag |= (field.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic);

            return translatedType.GetField(field.Name, flag);
        }

        private static PropertyInfo TranslatePropertyType(Type translatedType, PropertyInfo property, IDictionary<Type, Type> typeTranslations)
        {
            return translatedType.GetProperty(property.Name, 
                TranslateGenericArgument(property.PropertyType, typeTranslations),
                property.GetIndexParameters().Select(x => TranslateGenericArgument(x.ParameterType, typeTranslations)).ToArray());
        }

        private static Type CloseGenericType(Type componentType, Type requestedType, Dictionary<Type, Type> typeTransations)
        {
            Type closedComponentType;
            if (componentType.ContainsGenericParameters)
            {
                var args = CloseGenericArguments(componentType, requestedType, typeTransations).ToArray();
                if (args.Contains(null))
                    return null;
                try
                {
                    closedComponentType = componentType.GetGenericTypeDefinition().MakeGenericType(args);
                }
                catch (ArgumentException)
                {
                    // Incomatible constraint
                    closedComponentType = null;
                }
            }
            else
            {
                closedComponentType = componentType;
            }

            if (requestedType.IsAssignableFrom(closedComponentType))
                return closedComponentType;

            return null;
        }

        private static IEnumerable<Type> CloseGenericArguments(Type componentType, Type requestedType, Dictionary<Type, Type> typeTransations)
        {
            var i = 0;
            foreach (var arg in componentType.GetGenericArguments())
            {
                if (arg.IsGenericParameter)
                    yield return SearchClosedArgument(arg, componentType, requestedType, typeTransations);
                else if (arg.ContainsGenericParameters)
                {
                    var argArgs = CloseGenericArguments(arg, requestedType.GetGenericArguments()[i], typeTransations).ToArray();
                    if (argArgs.Contains(null))
                        yield return null;
                    else
                        yield return arg.GetGenericTypeDefinition().MakeGenericType(argArgs);
                }
                else yield return arg;

                i++;
            }
        }

        private static Type SearchClosedArgument(Type arg, Type componentType, Type requestedType, IDictionary<Type, Type> typeTransations)
        {
            var i = 0;
            foreach(var ctArg in componentType.GetGenericArguments())
            {
                Type translated;
                if (typeTransations.TryGetValue(arg, out translated))
                    return translated;
                if (arg == ctArg)
                {
                    var requestedArg = requestedType.GetGenericArguments()[i];
                    typeTransations[arg] = requestedArg;
                    return requestedArg;
                }
                if (!ctArg.IsGenericParameter && ctArg.ContainsGenericParameters)
                    return SearchClosedArgument(arg, ctArg, requestedType.GetGenericArguments()[i], typeTransations);
                i++;
            }

            return null;
        }

        private static Type OpenIfGeneric(Type type)
        {
            if (!type.IsGenericType)
                return type;
            return type.GetGenericTypeDefinition();
        }
    }
}