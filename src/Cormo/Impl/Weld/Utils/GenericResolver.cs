using System;
using System.Collections.Generic;
using System.Linq;

namespace Cormo.Impl.Weld.Utils
{
    public abstract class GenericResolver
    {
        public static GenericResolver ImplementerResolver = new ImplementerGenericResolver();
        public static GenericResolver AncestorResolver = new AncestorGenericResolver();

        public class ImplementerGenericResolver : GenericResolver
        {
            protected override Type ResolveGenericType(Type genericType, Type requestedType, Dictionary<Type, Type> typeTranslations)
            {
                var openRequestedType = GenericUtils.OpenIfGeneric(requestedType);
                var ancestors = TypeUtils.GetAncestors(genericType);

                var resolvedComponentType = (from i in ancestors
                                             where openRequestedType == GenericUtils.OpenIfGeneric(i)
                                             let closedType = CloseGenericType(i, requestedType, typeTranslations)
                                             where closedType != null
                                             select closedType)
                                            .FirstOrDefault();
                return resolvedComponentType;
            }

            protected override bool Matches(Type candidateType, Type requestedType)
            {
                return requestedType.IsAssignableFrom(candidateType);
            }
        }

        public class AncestorGenericResolver : GenericResolver
        {
            protected override bool Matches(Type candidateType, Type requestedType)
            {
                return candidateType.IsAssignableFrom(requestedType);
            }

            protected override Type ResolveGenericType(Type genericType, Type requestedType, Dictionary<Type, Type> typeTranslations)
            {
                var openGenericType = GenericUtils.OpenIfGeneric(genericType);
                var ancestors = TypeUtils.GetAncestors(requestedType);

                var resolvedComponentType = (from i in ancestors
                                             where openGenericType == GenericUtils.OpenIfGeneric(i)
                                             let closedType = CloseGenericType(genericType, i, typeTranslations)
                                             where closedType != null
                                             select closedType)
                                            .FirstOrDefault();
                return resolvedComponentType;
            }
        }

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
                .Select((x, i) => new { x, i })
                .ToDictionary(x => x.x, x => args[x.i]);
        }

        public Resolution ResolveType(Type possiblyGeneric, Type requestedType)
        {
            var typeTranslations = new Dictionary<Type, Type>();
            var resolvedType = ResolveType(possiblyGeneric, requestedType, typeTranslations);

            return new Resolution
            {
                ResolvedType = resolvedType,
                GenericParameterTranslations = typeTranslations
            };
        }

        private Type ResolveType(Type possiblyGeneric, Type requestedType, Dictionary<Type, Type> typeTranslations)
        {
            Type resolvedType;

            if (Matches(possiblyGeneric, requestedType))
                resolvedType = possiblyGeneric;
            else if (!possiblyGeneric.ContainsGenericParameters)
                return null;
            else
            {
                if (possiblyGeneric.IsGenericParameter)
                {
                    typeTranslations.Add(possiblyGeneric, requestedType);
                    return requestedType;
                }

                var resolvedComponentType = ResolveGenericType(possiblyGeneric, requestedType, typeTranslations);

                if (resolvedComponentType == null)
                    return null;

                resolvedType = GenericUtils.TranslateGenericArguments(possiblyGeneric, typeTranslations);
            }

            return resolvedType;
        }

        protected abstract Type ResolveGenericType(Type genericType, Type requestedType,
            Dictionary<Type, Type> typeTranslations);
        

        protected abstract bool Matches(Type candidateType, Type requestedType);

        private Type CloseGenericType(Type componentType, Type requestedType, Dictionary<Type, Type> typeTransations)
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

            if (Matches(closedComponentType, requestedType))
                return closedComponentType;

            return null;
        }

        private IEnumerable<Type> CloseGenericArguments(Type componentType, Type requestedType, Dictionary<Type, Type> typeTransations)
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
                    yield return ResolveType(arg, requestedType.GetGenericArguments()[i], typeTransations);
                }
                else yield return arg;

                i++;
            }
        }


        private static Type SearchClosedArgument(Type arg, Type componentType, Type requestedType, IDictionary<Type, Type> typeTransations)
        {
            var i = 0;
            foreach (var ctArg in componentType.GetGenericArguments())
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
    }
}