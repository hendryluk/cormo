using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Weld.Injections;
using Cormo.Injects;
using Cormo.Mixins;

namespace Cormo.Impl.Weld.Components
{
    public class Mixin : ManagedComponent
    {
        public IEnumerable<Type> MixinBinders { get; private set; }

        public Mixin(Type[] interfaceTypes, ConstructorInfo ctor, IBinders binders, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs) 
            : base(ctor, binders, scope, manager, postConstructs)
        {
            MixinBinders = binders.OfType<IMixinBinder>().Select(x=> x.GetType());
            InterfaceTypes = interfaceTypes;
        }

        public Type[] InterfaceTypes { get; private set; }

        protected override BuildPlan MakeConstructPlan()
        {
            return InjectableConstructor.Invoke;
        }
    }
}