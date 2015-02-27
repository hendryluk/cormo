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
            return Equals(MixinBindings, other.MixinBindings);
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
            return (MixinBindings != null ? MixinBindings.GetHashCode() : 0);
        }

        public MixinResolvable(IComponent component)
        {
            MixinBindings = component.Qualifiers.OfType<IMixinBinding>().Select(x=> x.GetType()).ToArray();
        }

        public Type[] MixinBindings { get; private set; }
    }
}