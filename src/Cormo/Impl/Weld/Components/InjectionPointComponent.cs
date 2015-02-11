using System;
using System.Collections.Generic;
using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public class InjectionPointComponent: AbstractComponent
    {
        public InjectionPointComponent(WeldComponentManager manager) : 
            base("", typeof(IInjectionPoint), Weld.Binders.Empty, typeof(DependentAttribute), manager)
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
            return context => Manager.GetService<CurrentInjectionPoint>().Peek();
        }

        public override IEnumerable<IChainValidatable> NextLinearValidatables
        {
            get { yield break; }
        }

        public override IEnumerable<IChainValidatable> NextNonLinearValidatables
        {
            get { yield break; }
        }
    }
}