using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Injections;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Cormo.Interceptions;
using Cormo.Reflects;

namespace Cormo.Impl.Weld.Components
{
    public delegate object InterceptorFunc();

    public class Interceptor : ManagedComponent
    {
        public bool AllowPartialInterception { get; private set; }
        private static Type[] AllInterceptorTypes = {typeof (IAroundInvokeInterceptor)};

        public Type[] InterceptorBindings { get; private set; }

        public Interceptor(IAnnotatedType type, WeldComponentManager manager, bool allowPartialInterception)
            : base(type, manager)
        {
            AllowPartialInterception = allowPartialInterception;
            InterceptorBindings = Annotations.OfType<IInterceptorBinding>().Select(x => x.GetType()).ToArray();
            InterceptorTypes = AllInterceptorTypes.Where(x => x.IsAssignableFrom(type.Type)).ToArray();
            
            if(!InterceptorBindings.Any())
                throw new InvalidComponentException(type.Type, "Interceptor must have at least one interceptor-binding attribute");
            if (!InterceptorTypes.Any())
                throw new InvalidComponentException(type.Type, "Interceptor must implement " + string.Join(" or ", AllInterceptorTypes.Select(x => x.ToString())));
        
        }

        public Type[] InterceptorTypes { get; private set; }

        public Type[] InterfaceTypes { get; private set; }

        protected override BuildPlan MakeConstructPlan()
        {
            return InjectableConstructor.Invoke;
        }
    }
}