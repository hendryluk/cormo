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
            var typeTranslations = new Dictionary<Type, Type>();
            var resolvedType = ResolveGenericType(component, requestedType, typeTranslations);
            
            return new Resolution
            {
                ResolvedType = resolvedType,
                GenericParameterTranslations = typeTranslations
            };
        }

        private static Type ResolveGenericType(Type component, Type requestedType, Dictionary<Type, Type> typeTranslations)
        {
            Type resolvedType;
            
            if (requestedType.IsAssignableFrom(component))
                resolvedType = component;
            else if (!component.ContainsGenericParameters)
                return null;
            else
            {
                if (component.IsGenericParameter)
                {
                    typeTranslations.Add(component, requestedType);
                    return requestedType;
                }

                var openRequestedType = OpenIfGeneric(requestedType);
                var ancestors = TypeUtils.GetComponentTypes(component);

                var resolvedComponentType = (from i in ancestors
                        where openRequestedType == OpenIfGeneric(i)
                        let closedType = CloseGenericType(i, requestedType, typeTranslations)
                        where closedType != null
                        select closedType)
                        .FirstOrDefault();

                if (resolvedComponentType == null)
                    return null;

                resolvedType = TranslateGenericArguments(component, typeTranslations);
            }

            return resolvedType;
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
                return method.GetGenericMethodDefinition().MakeGenericMethod(translatedMethodGenericArgs.ToArray());
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
                {
                    yield return SearchClosedArgument(arg, componentType, requestedType, typeTransations);
                }
                    
                else if (arg.ContainsGenericParameters)
                {
                    yield return ResolveGenericType(arg, requestedType.GetGenericArguments()[i], typeTransations);
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

        public static Type OpenIfGeneric(Type type)
        {
            if (!type.IsGenericType)
                return type;
            return type.GetGenericTypeDefinition();
        }
    }
}