using System;
using System.Collections.Generic;
using Alpaca.Contexts;
using Alpaca.Mixins;

namespace Alpaca.Injects
{
    public interface IComponentManager
    {
        IEnumerable<IComponent> GetComponents(Type type, params QualifierAttribute[] qualifiers);
        IComponent GetComponent(Type type, params QualifierAttribute[] qualifiers);
        IComponent GetComponent(IInjectionPoint injectionPoint);
        object GetReference(IComponent component, ICreationalContext creationalContext);
        object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component, ICreationalContext context);
        object GetReference(Type type, params QualifierAttribute[] qualifiers);
        T GetReference<T>(params QualifierAttribute[] qualifiers);
        string Id { get;}
    }
}