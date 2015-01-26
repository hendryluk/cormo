using System;
using System.Collections.Generic;
using Alpaca.Contexts;
using Alpaca.Injects;

namespace Alpaca.Weld.Components
{
    public class InjectionPointComponent: AbstractComponent
    {
        public InjectionPointComponent(WeldComponentManager manager) : 
            base("", typeof(IInjectionPoint), new QualifierAttribute[0], typeof(DependentAttribute), manager)
        {
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            return requestedType.IsAssignableFrom(Type) ? this : null;
        }

        protected override BuildPlan GetBuildPlan()
        {
            return (context, ip) => ip;
        }

        public override bool IsConcrete
        {
            get { return true; }
        }
    }
}