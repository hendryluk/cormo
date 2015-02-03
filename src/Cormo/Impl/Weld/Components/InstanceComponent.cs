using System;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public class InstanceComponent : AbstractComponent
    {
        private readonly Type _baseType;
        private readonly IWeldComponent[] _components;

        public InstanceComponent(Type baseType, IEnumerable<IBinderAttribute> binders, WeldComponentManager manager, IWeldComponent[] components)
            : base("", typeof(Instance<>).MakeGenericType(baseType), binders, typeof(DependentAttribute), manager)
        {
            _baseType = baseType;
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
            return context => Activator.CreateInstance(Type, 
                Manager, Qualifiers.ToArray(), _components, context);
        }

        public override bool IsConcrete
        {
            get { return true; }
        }
    }
}