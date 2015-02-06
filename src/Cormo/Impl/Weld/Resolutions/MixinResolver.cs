using System.Collections.Generic;
using System.Linq;
using Cormo.Impl.Weld.Components;

namespace Cormo.Impl.Weld.Resolutions
{
    public class MixinResolver: TypeSafeResolver<Mixin, MixinResolvable>
    {
        public MixinResolver(WeldComponentManager manager, IEnumerable<Mixin> allComponents) : base(manager, allComponents)
        {
        }

        protected override IEnumerable<Mixin> Resolve(MixinResolvable resolvable, ref IEnumerable<Mixin> mixins)
        {
            return mixins.Where(x => x.MixinBinders.Any(resolvable.MixinBinders.Contains));
        }
    }
}