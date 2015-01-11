using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Alpaca.Weld.Utils
{
    public static class GenericUtils
    {
        public static FieldInfo ResolveFieldToReturn(FieldInfo field, Type wantedReturnType)
        {
            var resolved = field;
            return resolved;
        }

        public static PropertyInfo ResolvePropertyToReturn(PropertyInfo property, Type resolvedType)
        {
            throw new NotImplementedException();
        }

        public static MethodInfo ResolveMethodToReturn(MethodInfo method, Type wantedReturnType)
        {
            var resolved = method;
            if (method.ReturnType.IsGenericTypeDefinition)
            {
                var type = method.ReflectedType ?? method.DeclaringType;
                if (type.IsGenericType)
                {
                    
                }
            }

            if (method.IsGenericMethodDefinition || (method.ReflectedType??method.DeclaringType).IsGenericTypeDefinition)
                return null;

            return resolved;
        }

        public class Resolution
        {
            public Type ResolvedType { get; set; }
            public IDictionary<Type, Type> GenericParameterTranslations { get; set; } 
        }

        public static Resolution ResolveGenericType(Type component, Type requestedType)
        {
            Type resolvedType = null;
            var typeTransations = new Dictionary<Type, Type>();

            if (requestedType.IsAssignableFrom(component))
                resolvedType = component;
            else if (!component.ContainsGenericParameters)
                return null;
            else
            {
                var openRequestedType = OpenIfGeneric(requestedType);
                var interfaces = TypeUtils.GetComponentTypes(component);

                resolvedType = (from i in interfaces
                        where openRequestedType == OpenIfGeneric(i)
                        let closedType = CloseGenericType(i, requestedType, typeTransations)
                        where closedType != null
                        select closedType)
                        .FirstOrDefault();
            }

            if (resolvedType == null)
                return null;

            return new Resolution
            {
                ResolvedType = resolvedType,
                GenericParameterTranslations = typeTransations
            };
        }

        private static Type CloseGenericType(Type componentType, Type requestedBase, Dictionary<Type, Type> typeTransations)
        {
            Type closedComponentType;
            if (componentType.ContainsGenericParameters)
            {
                var args = CloseGenericArguments(componentType, requestedBase, typeTransations).ToArray();
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

            if (requestedBase.IsAssignableFrom(closedComponentType))
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
                        yield return arg.MakeGenericType(argArgs);
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
                    typeTransations[arg] = ctArg;
                    return requestedType.GetGenericArguments()[i];
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