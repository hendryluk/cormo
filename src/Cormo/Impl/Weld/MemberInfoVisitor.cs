using System;
using System.Reflection;

namespace Cormo.Impl.Weld
{
    public static class MemberInfoVisitor
    {
        public static T VisitInject<T>(MemberInfo member, 
            Func<MethodBase, T> onMethod,
            Func<FieldInfo, T> onField,
            Func<PropertyInfo, T> onProperty)
        {
            var method = member as MethodBase;
            if (method != null)
                return onMethod(method);
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