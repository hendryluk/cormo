using System;
using System.Collections.Generic;
using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public class ValueComponent : AbstractComponent
    {
        private readonly object _instance;

        public ValueComponent(object instance, IEnumerable<QualifierAttribute> qualifiers, Type scope, WeldComponentManager manager)
            : base(instance.GetType().FullName, instance.GetType(), qualifiers, scope, manager)
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

        public override void Destroy(object instance, ICreationalContext creationalContext)
        {
        }

        protected override BuildPlan GetBuildPlan()
        {
            return (_,__) => _instance;
        }

        public override string ToString()
        {
            return string.Format("Instance Component [{0}] with Qualifiers [{1}]", Type, string.Join(",", Qualifiers));
        }
    }
}