using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Contexts
{
    public abstract class ForwardingContextual : IContextual
    {
        protected abstract IContextual Delegate { get;  }

        public object Create(ICreationalContext context)
        {
            return Delegate.Create(context);
        }

        public void Destroy(object instance, ICreationalContext creationalContext)
        {
            Delegate.Destroy(instance, creationalContext);
        }

        public override bool Equals(object obj)
        {
            return this == obj || Delegate.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Delegate.GetHashCode();
        }

        public override string ToString()
        {
            return Delegate.ToString();
        }
    }
}