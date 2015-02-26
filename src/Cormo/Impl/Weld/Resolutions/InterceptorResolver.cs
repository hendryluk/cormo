using System.Collections.Generic;
using System.Linq;
using Cormo.Impl.Weld.Components;

namespace Cormo.Impl.Weld.Resolutions
{
    public class InterceptorResolver : ContextualResolver<Interceptor, InterceptorResolvable>
    {
        public InterceptorResolver(WeldComponentManager manager, IEnumerable<Interceptor> allComponents) 
            : base(manager, allComponents)
        {
        }

        protected override IEnumerable<Interceptor> Resolve(InterceptorResolvable resolvable, ref IEnumerable<Interceptor> interceptors)
        {
            if (!resolvable.Bindings.Any())
                return Enumerable.Empty<Interceptor>();

            return interceptors.Where(x => 
                x.InterceptorTypes.Contains(resolvable.InterceptorType) 
                && x.InterceptorBindings.Any(resolvable.Bindings.Contains));
        }
    }
}