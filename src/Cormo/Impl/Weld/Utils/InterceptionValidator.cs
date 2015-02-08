using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Cormo.Impl.Weld.Resolutions;
using Cormo.Interceptions;

namespace Cormo.Impl.Weld.Utils
{
    public static class InterceptionValidator
    {
        public static void ValidateInterceptableClass(Type type, IntercetorResolvable resolvable, out MethodInfo[] methods)
        {
            var builder = new StringBuilder();
            if (type.IsSealed)
            {
                builder.Append("class is sealed");
            }

            const BindingFlags flagsAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            methods = type.GetMethods(flagsAll)
                .Union(type.GetProperties(flagsAll)
                    .SelectMany(x => new[] { x.SetMethod, x.GetGetMethod() }.Where(m => m != null)))
                .Where(x=> !x.IsPrivate).ToArray();

            var nonVirtualMethods = methods.Where(x => !TypeUtils.IsOveridable(x)).ToArray();
            if (nonVirtualMethods.Any())
            {
                builder.Append(
                    string.Format("These public members must be virtual: {0}",
                        string.Join(",/n", nonVirtualMethods.Select(x => x.ToString()))));
            }
            if(builder.Length > 0)
                ThrowNotInterceptableClassException(type, resolvable, builder.ToString());
        }

        public static void ValidateInterceptableMethod(MethodInfo methodInfo, IntercetorResolvable resolvable)
        {
            if (methodInfo.IsStatic)
                ThrowNotInterceptableMethodException(methodInfo, resolvable, "must not be static");
            if (methodInfo.IsPrivate)
                ThrowNotInterceptableMethodException(methodInfo, resolvable, "must not be private");
            if (!TypeUtils.IsOveridable(methodInfo))
                ThrowNotInterceptableMethodException(methodInfo, resolvable, "must be virtual");
        }

        private static void ThrowNotInterceptableMethodException(MethodInfo methodInfo, IntercetorResolvable resolvable,
            string reason)
        {
            var msg = string.Format("Method [{0}] with interceptor-bindings [{1}] {2}", methodInfo,
                string.Join(",", resolvable.Bindings.Cast<object>().ToArray()),
                reason);

            throw new NotInterceptableException(msg);
        }

        private static void ThrowNotInterceptableClassException(Type type, IntercetorResolvable resolvable,
            string reason)
        {
            var msg = string.Format("Class [{0}] with interceptor-bindings [{1}] {2}", type,
                string.Join(",", resolvable.Bindings.Cast<object>().ToArray()),
                reason);
            throw new NotInterceptableException(msg);
        }
    }
}