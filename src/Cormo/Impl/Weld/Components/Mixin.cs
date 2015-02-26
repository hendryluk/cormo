using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Weld.Injections;
using Cormo.Injects;
using Cormo.Mixins;
using Cormo.Reflects;

namespace Cormo.Impl.Weld.Components
{
    public class Mixin : ManagedComponent
    {
        public IEnumerable<Type> MixinBinders { get; private set; }

        public Mixin(IAnnotatedType type, WeldComponentManager manager) 
            : base(type, manager)
        {
            MixinBinders = Binders.OfType<IMixinBinder>().Select(x=> x.GetType());
            InterfaceTypes = Binders.OfType<MixinAttribute>().SelectMany(x=> x.InterfaceTypes).ToArray();
        }

        public Type[] InterfaceTypes { get; private set; }

        protected override BuildPlan MakeConstructPlan()
        {
            return InjectableConstructor.Invoke;
        }
    }
}