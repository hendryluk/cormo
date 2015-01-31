using Cormo.Injects;

namespace Cormo.Contexts
{
    public interface IContextual
    {
        object Create(ICreationalContext context);
        void Destroy(object instance, ICreationalContext creationalContext);
    }
}