using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Weld.Injections;

namespace Alpaca.Weld.Components
{
    public class Mixin : ManagedComponent
    {
        public Mixin(Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs) 
            : base(type, qualifiers, scope, manager, postConstructs)
        {
        }

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

            return (context, ip) =>
            {
                var paramVals = paramInjects.Select(p => p.GetValue(context, p)).ToArray();
                return Activator.CreateInstance(Type, paramVals);
            };
        }
    }
}