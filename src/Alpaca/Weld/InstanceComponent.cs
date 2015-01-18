using System;
using System.Collections.Generic;
using Alpaca.Contexts;
using Alpaca.Injects;

namespace Alpaca.Weld
{
    public class InstanceComponent : AbstractComponent
    {
        private readonly object _instance;

        public InstanceComponent(object instance, Type type, IEnumerable<QualifierAttribute> qualifiers, ScopeAttribute scope, IComponentManager manager)
            : base(type, qualifiers, scope, manager)
        {
            _instance = instance;
        }

        public override bool IsConcrete
        {
            get { return true; }
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            if (requestedType.IsInstanceOfType(_instance))
                return this;
            return null;
        }

        public override void OnDeploy()
        {
        }

        protected override BuildPlan GetBuildPlan()
        {
            return () => _instance;
        }
    }
}