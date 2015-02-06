using System;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Utils;
using Cormo.Injects;
using Cormo.Interceptions;

namespace Cormo.Impl.Weld.Resolutions
{
    public class IntercetorResolvable : IResolvable
    {
        protected bool Equals(IntercetorResolvable other)
        {
            return InterceptorType == other.InterceptorType && Equals(Bindings, other.Bindings);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IntercetorResolvable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((InterceptorType != null ? InterceptorType.GetHashCode() : 0)*397) ^ (Bindings != null ? Bindings.GetHashCode() : 0);
            }
        }

        public IntercetorResolvable(Type interceptorType, MethodInfo method)
        {
            InterceptorType = interceptorType;
            Bindings = method.GetAttributesRecursive<IInterceptorBinding>().Select(x=> x.GetType()).ToArray();
        }

        public IntercetorResolvable(Type interceptorType, IComponent component)
        {
            InterceptorType = interceptorType;
            Bindings = component.Binders.Select(x => x.GetType()).ToArray();
        }

        public Type InterceptorType { get; private set; }
        public Type[] Bindings { get; private set; }
    }
}