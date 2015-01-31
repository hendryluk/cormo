using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Cormo.Utils;

namespace Cormo.Impl.Weld.Utils
{
    public static class TypeUtils
    {
        public static bool IsComponent(Type type)
        {
            return //type.IsClass &&  // Already checked at scanner level
                    !(type.IsSealed && type.IsAbstract) // static classes
                   && (!type.IsAbstract || type.HasAttributeRecursive<MixinAttribute>() || type.HasAttributeRecursive<DecoratorAttribute>())
                   && HasInjectableConstructor(type);
        }

        public static bool HasInjectableConstructor(Type type)
        {
            var accessibleConstructors = type.IsAbstract
                ? type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => !x.IsPrivate)
                : type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            return accessibleConstructors.Any(x => x.HasAttributeRecursive<InjectAttribute>() || !x.GetParameters().Any());
        }

        public static IEnumerable<ConstructorInfo> GetInjectableConstructors(Type type)
        {
            return type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.HasAttributeRecursive<InjectAttribute>());
        }

        public static void CheckProxiable(Type type)
        {
            if (type.IsInterface)
            {
                return;
            }

            if (type.IsEnum)
            {
                throw new NonProxiableTypeException(type, "Enum type");
            }

            if (type.IsPrimitive)
            {
                throw new NonProxiableTypeException(type, "Primitive type");
            }

            if (type.IsSealed)
            {
                throw new NonProxiableTypeException(type, "Class is sealed");
            }

            if (!HasAccessibleParameterlessConstructor(type))
            {
                throw new NonProxiableTypeException(type, "No public/protected parameterless constructor");
            }

            var sealedMembers = GetSealedPublicMembers(type);
            if (sealedMembers.Any())
            {
                throw new NonProxiableTypeException(type, 
                    string.Format("These public members must be virtual: {0}", 
                    string.Join(",/n", sealedMembers.Select(x=> x.ToString()))));
            }

            var publicFields = GetPublicFields(type);
            if (publicFields.Any())
            {
                throw new NonProxiableTypeException(type,
                    string.Format("Must not have public fields: {0}",
                    string.Join(",/n", publicFields.Select(x => x.ToString()))));
            }
        }

        public static IEnumerable<Type> GetComponentTypes(Type type)
        {
            for(var t = type; t != null; t=t.BaseType)
            {
                yield return t;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                yield return interfaceType;
            }
        }

        private static bool HasAccessibleParameterlessConstructor(Type type)
        {
            return type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Any(constructor => !constructor.GetParameters().Any() && !constructor.IsPrivate);
        }

        private static MemberInfo[] GetSealedPublicMembers(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => !x.IsVirtual && !x.IsAbstract);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(x => (!x.GetMethod.IsVirtual && !x.GetMethod.IsAbstract) ||
                                !x.GetMethod.IsVirtual && !x.GetMethod.IsAbstract);

            var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance)
                            .Where(x => (!x.AddMethod.IsVirtual && !x.AddMethod.IsAbstract) ||
                                !x.RemoveMethod.IsVirtual && !x.RemoveMethod.IsAbstract);

            return methods.Cast<MemberInfo>().Union(properties).Union(events).ToArray();
        }

        private static FieldInfo[] GetPublicFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }
    }
}