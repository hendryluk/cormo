using System;

namespace Alpaca.Injects
{
    public interface IComponentManager
    {
        IComponent GetComponent(Type type, params QualifierAttribute[] qualifiers);
        IComponent GetComponent(IInjectionPoint injectionPoint);
        object GetReference(IComponent component);
        object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component);
    }
}