using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Alpaca.Weld.Utils
{
    public static class GenericUtils
    {
        public static MethodInfo ResolveMethodToReturn(MethodInfo method, Type wantedReturnType)
        {
            MethodInfo resolved = method;
            if (method.ReturnType.IsGenericTypeDefinition)
            {
                var type = method.ReflectedType ?? method.DeclaringType;
                if (type.IsGenericType)
                {
                    
                }
            }

            if (method.IsGenericMethodDefinition || (method.ReflectedType??method.DeclaringType).IsGenericTypeDefinition)
                return null;

            return method;
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

            if (!component.ContainsGenericParameters && requestedType.IsAssignableFrom(component))
                resolvedType = component;
            else
            {
                var openRequestedType = OpenIfGeneric(requestedType);
                var interfaces = TypeUtils.GetComponentTypes(component);

                resolvedType = (from i in interfaces
                        where openRequestedType == OpenIfGeneric(i)
                                 let closedType = CloseGenericType(component, i, requestedType, typeTransations)
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

        private static Type CloseGenericType(Type component, Type componentType, Type requestedBase, Dictionary<Type, Type> typeTransations)
        {
            Type closedComponent;
            if (component.ContainsGenericParameters)
            {
                var args = CloseGenericArguments(component, componentType, requestedBase, typeTransations).ToArray();
                if (args.Contains(null))
                    return null;
                closedComponent = component.GetGenericTypeDefinition().MakeGenericType(args);
            }
            else
            {
                closedComponent = component;
            }

            if (requestedBase.IsAssignableFrom(closedComponent))
                return closedComponent;

            return null;
        }

        private static IEnumerable<Type> CloseGenericArguments(Type component, Type componentType, Type requestedType, Dictionary<Type, Type> typeTransations)
        {
            foreach (var arg in component.GetGenericArguments())
            {
                if (!arg.IsGenericParameter && !arg.ContainsGenericParameters)
                {
                    yield return arg;
                    continue;
                }

                yield return SearchClosedArgument(arg, componentType, requestedType, typeTransations);
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