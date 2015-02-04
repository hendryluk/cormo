using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Injections;
using Cormo.Injects;
using Cormo.Interceptions;

namespace Cormo.Impl.Weld.Components
{
    public delegate object InterceptorFunc();

    public class Interceptor : ManagedComponent
    {
        private readonly IEnumerable<Type> _interceptorBinders;

        public Interceptor(Type type, IBinders binders, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs) 
            : base(type, binders, scope, manager, postConstructs)
        {
            _interceptorBinders = binders.OfType<IInterceptorBinding>().Select(x => x.GetType());
        }

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

        public bool CanIntercept(IComponent component)
        {
            return _interceptorBinders.Any(component.Binders.Select(x => x.GetType()).Contains);
        }

        public bool CanIntercept(MethodInfo method)
        {
            return _interceptorBinders.Any(method.HasAttributeRecursive);
        }
    }
}