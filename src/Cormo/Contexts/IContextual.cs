using Cormo.Injects;

namespace Cormo.Contexts
{
    public interface IContextual
    {
        object Create(ICreationalContext context, IInjectionPoint ip);
        void Destroy();
    }
}