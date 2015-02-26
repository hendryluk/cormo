using System;
using System.Collections.Generic;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Injects.Exceptions;

namespace Cormo.Impl.Weld.Components
{
    public class ExtensionComponent : AbstractComponent
    {
        private readonly IExtension _instance;

        public ExtensionComponent(Type type, WeldComponentManager manager) : base(type.FullName, type, Weld.Binders.Empty, manager)
        {
            try
            {
                _instance = (IExtension) Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                throw new CreationException(type, e);
            }
        }

        public override void Destroy(object instance, ICreationalContext creationalContext)
        {
            // nothing
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
    }
}