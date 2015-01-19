using System;
using System.Collections.Generic;
using System.Linq;
using Alpaca.Contexts;
using Alpaca.Injects;

namespace Alpaca.Weld.Component
{
    public class InstanceComponent : AbstractComponent
    {
        private readonly IWeldComponent[] _components;

        public InstanceComponent(Type type, IEnumerable<QualifierAttribute> qualifiers, IComponentManager manager, IWeldComponent[] components) 
            : base(type, qualifiers, typeof(DependentAttribute), manager)
        {
            _components = components;
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            return this;
        }

        protected override BuildPlan GetBuildPlan()
        {
            var type = typeof (Instance<>).MakeGenericType(Type);
            return () => Activator.CreateInstance(type, Type, Qualifiers.ToArray(), _components);
        }

        public override bool IsConcrete
        {
            get { return true; }
        }
    }
}