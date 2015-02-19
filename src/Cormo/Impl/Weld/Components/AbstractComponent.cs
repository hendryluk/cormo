using System;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Impl.Weld.Injections;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public abstract class AbstractComponent : IWeldComponent
    {
        private AbstractComponent(Type type, IBinders binders,
            Type scope, WeldComponentManager manager)
        {
            var qualifierSet = new HashSet<IQualifier>(binders.OfType<IQualifier>());
            if (qualifierSet.All(x => (x is AnyAttribute)))
                qualifierSet.Add(DefaultAttribute.Instance);

            Type = type;
            Manager = manager;
            Binders = binders;
            Scope = scope;
            IsProxyRequired = typeof(NormalScopeAttribute).IsAssignableFrom(scope) && !binders.OfType<UnwrapAttribute>().Any();
            _lazyBuildPlan = new Lazy<BuildPlan>(GetBuildPlan);
            IsConditionalOnMissing = binders.OfType<ConditionalOnMissingComponentAttribute>().Any();
        }

        public IBinders Binders { get; private set; }

        protected AbstractComponent(ComponentIdentifier id, Type type, IBinders binders, Type scope,
            WeldComponentManager manager)
            : this(type, binders, scope, manager)
        {
            _id = id;
        }

        protected AbstractComponent(string idSuffix, Type type, IBinders binders, Type scope, WeldComponentManager manager)
            : this(type, binders, scope, manager)
        {
            _id = new ComponentIdentifier(string.Format("{0}-{1}-{2}", manager.Id, GetType().Name, idSuffix));
        }

        public IQualifiers Qualifiers { get { return Binders.Qualifiers; }}

        public Type Scope { get; private set; }
        public Type Type { get; set; }

        IComponentManager IComponent.Manager
        {
            get { return Manager; }
        }

        public WeldComponentManager Manager { get; set; }
        public bool IsConditionalOnMissing { get; private set; }

        public virtual void Touch()
        {
        }

        

        private readonly Lazy<BuildPlan> _lazyBuildPlan;
        private ComponentIdentifier _id;

        public bool IsProxyRequired { get; private set; }

        public virtual IWeldComponent Resolve(Type requestedType)
        {
            return requestedType.IsAssignableFrom(Type) ? this : null;
        }

        public object Create(ICreationalContext context)
        {
            return _lazyBuildPlan.Value(context);
        }

        public abstract void Destroy(object instance, ICreationalContext creationalContext);

        protected abstract BuildPlan GetBuildPlan();
        public ComponentIdentifier Id { get { return _id; } }
        public abstract IEnumerable<IChainValidatable> NextLinearValidatables { get; }
        public abstract IEnumerable<IChainValidatable> NextNonLinearValidatables { get; }
    }
}