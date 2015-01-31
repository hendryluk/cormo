using System;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public class InstanceComponent : AbstractComponent
    {
        private readonly IWeldComponent[] _components;

        public InstanceComponent(Type baseType, IEnumerable<QualifierAttribute> qualifiers, WeldComponentManager manager, IWeldComponent[] components) 
            : base("", typeof(Instance<>).MakeGenericType(baseType), qualifiers, typeof(DependentAttribute), manager)
        {
            _components = components;
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            return this;
        }

        public override void Destroy(object instance, ICreationalContext creationalContext)
        {
            creationalContext.Release();
        }

        protected override BuildPlan GetBuildPlan()
        {
            return (context, ip) => Activator.CreateInstance(Type, 
                Qualifiers.ToArray(), _components, context);
        }

        public override bool IsConcrete
        {
            get { return true; }
        }
    }
}