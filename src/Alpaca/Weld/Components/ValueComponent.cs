using System;
using System.Collections.Generic;
using Alpaca.Injects;
using Alpaca.Injects;

namespace Alpaca.Weld.Components
{
    public class ValueComponent : AbstractComponent
    {
        private readonly object _instance;

        public ValueComponent(object instance, IEnumerable<QualifierAttribute> qualifiers, Type scope, IComponentManager manager)
            : base(instance.GetType(), qualifiers, scope, manager)
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

        protected override BuildPlan GetBuildPlan()
        {
            return () => _instance;
        }

        public override string ToString()
        {
            return string.Format("Instance Component [{0}] with Qualifiers [{1}]", Type, string.Join(",", Qualifiers));
        }
    }
}