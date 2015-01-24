using System;
using System.Collections.Generic;
using System.Linq;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Injects;

namespace Alpaca.Weld.Components
{
    public class InstanceComponent : AbstractComponent
    {
        private readonly IWeldComponent[] _components;

        public InstanceComponent(Type baseType, IEnumerable<QualifierAttribute> qualifiers, IComponentManager manager, IWeldComponent[] components) 
            : base("", typeof(Instance<>).MakeGenericType(baseType), qualifiers, typeof(DependentAttribute), manager)
        {
            _components = components;
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            return this;
        }

        protected override BuildPlan GetBuildPlan()
        {
            return context => Activator.CreateInstance(Type, 
                Qualifiers.ToArray(), _components, context);
        }

        public override bool IsConcrete
        {
            get { return true; }
        }
    }
}