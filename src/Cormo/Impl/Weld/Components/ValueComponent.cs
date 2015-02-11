using System;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public class ValueComponent : AbstractComponent
    {
        private readonly object _instance;

        public ValueComponent(object instance, IBinders binders, Type scope, WeldComponentManager manager)
            : base(instance.GetType().FullName, instance.GetType(), binders, scope, manager)
        {
            _instance = instance;
        }

        public override void Destroy(object instance, ICreationalContext creationalContext)
        {
        }

        protected override BuildPlan GetBuildPlan()
        {
            return _ => _instance;
        }

        public override IEnumerable<IChainValidatable> NextLinearValidatables
        {
            get { yield break; }
        }

        public override IEnumerable<IChainValidatable> NextNonLinearValidatables
        {
            get { yield break; }
        }

        public override string ToString()
        {
            return string.Format("Instance Component [{0}] with Qualifiers [{1}]", Type, string.Join(",", Qualifiers));
        }
    }
}