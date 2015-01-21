using System;
using System.Collections.Generic;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Weld.Components;

namespace Alpaca.Injects
{
    public interface IComponentManager
    {
        IEnumerable<IWeldComponent> GetComponents(Type type, params QualifierAttribute[] qualifiers);
        IComponent GetComponent(Type type, params QualifierAttribute[] qualifiers);
        IComponent GetComponent(IInjectionPoint injectionPoint);
        object GetReference(IComponent component, ICreationalContext context);
        object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component, ICreationalContext context);
        object GetReference(Type type, params QualifierAttribute[] qualifiers);
        T GetReference<T>(params QualifierAttribute[] qualifiers);
    }
}