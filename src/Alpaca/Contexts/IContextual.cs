using Alpaca.Injects;

namespace Alpaca.Contexts
{
    public interface IContextual
    {
        object Create(ICreationalContext context, IInjectionPoint ip);
        void Destroy();
    }
}