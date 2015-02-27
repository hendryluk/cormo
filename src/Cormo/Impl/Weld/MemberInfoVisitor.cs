using System;
using System.Reflection;

namespace Cormo.Impl.Weld
{
    public static class MemberInfoVisitor
    {
        public static T Visit<T>(MemberInfo member,
            Func<ConstructorInfo, T> onConstructor,
            Func<MethodInfo, T> onMethod,
            Func<FieldInfo, T> onField,
            Func<PropertyInfo, T> onProperty)
        {
            var method = member as MethodInfo;
            if (method != null)
                return onMethod(method);
            var ctor = member as ConstructorInfo;
            if (ctor != null)
                return onConstructor(ctor);
            var field = member as FieldInfo;
            if (field != null)
                return onField(field);
            var property = member as PropertyInfo;
            if (property != null)
                return onProperty(property);

            return default(T);
        }
    }
}