using System.Collections.Generic;
using System.Linq;
using Cormo.Impl.Weld.Components;

namespace Cormo.Impl.Weld.Resolutions
{
    public class InterceptorResolver : TypeSafeResolver<Interceptor, IntercetorResolvable>
    {
        public InterceptorResolver(WeldComponentManager manager, IEnumerable<Interceptor> allComponents) 
            : base(manager, allComponents)
        {
        }

        protected override IEnumerable<Interceptor> Resolve(IntercetorResolvable resolvable, ref IEnumerable<Interceptor> interceptors)
        {
            return interceptors.Where(x => 
                x.InterceptorTypes.Contains(resolvable.InterceptorType) 
                && x.InterceptorBindings.Any(resolvable.Bindings.Contains));
        }
    }
}