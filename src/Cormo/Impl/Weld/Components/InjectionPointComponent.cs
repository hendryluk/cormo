using System;
using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
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

        public override void Destroy(object instance, ICreationalContext creationalContext)
        {
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