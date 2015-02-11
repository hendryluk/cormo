using System;
using Cormo.Injects;

namespace Cormo.Contexts
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field)]
    public abstract class ScopeAttribute : Attribute, IBinderAttribute
    {
        public static bool IsNormal(Type scope)
        {
            return typeof (NormalScopeAttribute).IsAssignableFrom(scope);
        }
    }
}