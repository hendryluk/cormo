namespace Alpaca.Injects
{
    public interface IComponentManager
    {
        IComponent GetComponent(IInjectionPoint injectionPoint);
        object GetReference(IComponent component);
        object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component);
    }
}