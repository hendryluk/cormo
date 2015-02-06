using System;
using System.Linq;
using Cormo.Injects;
using Cormo.Mixins;

namespace Cormo.Impl.Weld.Resolutions
{
    public class MixinResolvable : IResolvable
    {
        protected bool Equals(MixinResolvable other)
        {
            return Equals(MixinBinders, other.MixinBinders);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MixinResolvable) obj);
        }

        public override int GetHashCode()
        {
            return (MixinBinders != null ? MixinBinders.GetHashCode() : 0);
        }

        public MixinResolvable(IComponent component)
        {
            MixinBinders = component.Qualifiers.OfType<IMixinBinder>().Select(x=> x.GetType()).ToArray();
        }

        public Type[] MixinBinders { get; private set; }
    }
}