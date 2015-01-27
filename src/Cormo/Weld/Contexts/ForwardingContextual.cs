using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Weld.Contexts
{
    public abstract class ForwardingContextual : IContextual
    {
        protected abstract IContextual Delegate { get;  }

        public object Create(ICreationalContext context, IInjectionPoint ip)
        {
            return Delegate.Create(context, ip);
        }

        public void Destroy()
        {
            Delegate.Destroy();
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