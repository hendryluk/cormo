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

namespace Cormo.Impl.Weld.Components
{
    public delegate object InterceptorFunc();

    public class Interceptor : ManagedComponent
    {
        private static Type[] AllInterceptorTypes = {typeof (IAroundInvokeInterceptor)};

        public Type[] InterceptorBindings { get; private set; }

        public Interceptor(Type type, IBinders binders, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs) 
            : base(type, binders, scope, manager, postConstructs)
        {
            InterceptorBindings = binders.OfType<IInterceptorBinding>().Select(x => x.GetType()).ToArray();
            InterceptorTypes = AllInterceptorTypes.Where(x => x.IsAssignableFrom(type)).ToArray();
            
            if(!InterceptorBindings.Any())
                throw new InvalidComponentException(type, "Interceptor must have at least one interceptor-binding attribute");
            if (!InterceptorTypes.Any())
                throw new InvalidComponentException(type, "Interceptor must implement " + string.Join(" or ", AllInterceptorTypes.Select(x=> x.ToString())));
        
        }

        public Type[] InterceptorTypes { get; private set; }

        public Type[] InterfaceTypes { get; private set; }

        public override IWeldComponent Resolve(Type requestedType)
        {
            return requestedType.IsAssignableFrom(Type) ? this : null;
        }

        protected override BuildPlan MakeConstructPlan(IEnumerable<MethodParameterInjectionPoint> injects)
        {
            var paramInjects = injects.GroupBy(x => x.Member)
                .Select(x => x.OrderBy(i => i.Position).ToArray())
                .DefaultIfEmpty(new MethodParameterInjectionPoint[0])
                .First();

            return context =>
            {
                var paramVals = paramInjects.Select(p => p.GetValue(context)).ToArray();
                return Activator.CreateInstance(Type, paramVals);
            };
        }
    }
}