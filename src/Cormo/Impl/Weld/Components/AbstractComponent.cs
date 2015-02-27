using System;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Injections;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;
using Cormo.Reflects;

namespace Cormo.Impl.Weld.Components
{
    public abstract class AbstractComponent : IWeldComponent
    {
        private AbstractComponent(Type type, IAnnotations annotations, WeldComponentManager manager)
        {
            Annotations = annotations;
            var qualifierSet = new HashSet<IQualifier>(Annotations.OfType<IQualifier>());
            if (qualifierSet.All(x => (x is AnyAttribute)))
                qualifierSet.Add(DefaultAttribute.Instance);

            Type = type;
            Manager = manager;
            Scope = annotations.OfType<ScopeAttribute>().Select(x => x.GetType()).FirstOrDefault() ?? typeof(DependentAttribute);
            IsProxyRequired = typeof(NormalScopeAttribute).IsAssignableFrom(Scope) && !Annotations.OfType<UnwrapAttribute>().Any();
            _lazyBuildPlan = new Lazy<BuildPlan>(GetBuildPlan);
            IsConditionalOnMissing = Annotations.OfType<ConditionalOnMissingComponentAttribute>().Any();
        }

        public IAnnotations Annotations { get; private set; }

        protected AbstractComponent(ComponentIdentifier id, Type type, IAnnotations annotations, WeldComponentManager manager)
            : this(type, annotations, manager)
        {
            _id = id;
        }

        protected AbstractComponent(string idSuffix, Type type, IAnnotations annotations, WeldComponentManager manager)
            : this(type, annotations, manager)
        {
            _id = new ComponentIdentifier(string.Format("{0}-{1}-{2}", manager.Id, GetType().Name, idSuffix));
        }

        public IQualifiers Qualifiers { get { return Annotations.Qualifiers; }}

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