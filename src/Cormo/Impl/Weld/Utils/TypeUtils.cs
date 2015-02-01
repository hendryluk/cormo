using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Cormo.Mixins;
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

        public static bool HasAccessibleDefaultConstructor(Type type)
        {
            return type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Any(constructor => !constructor.GetParameters().Any() && !constructor.IsPrivate);
        }

        public static MemberInfo[] GetSealedPublicMembers(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => (!x.IsVirtual || x.IsFinal) && !x.IsAbstract && x.DeclaringType != typeof(object));

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(x => new []{x.GetMethod, x.SetMethod}
                                .Any(y => y != null && (!y.IsVirtual || y.IsFinal) && !y.IsAbstract));

            var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance)
                            .Where(x => new[] { x.AddMethod, x.RemoveMethod }
                                .Any(y => y != null && (!y.IsVirtual || y.IsFinal) && !y.IsAbstract));

            return methods.Cast<MemberInfo>().Union(properties).Union(events).ToArray();
        }

        public static FieldInfo[] GetPublicFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }
    }
}