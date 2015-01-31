using System;
using System.Collections.Generic;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Mixins;

namespace Cormo.Injects
{
    public interface IComponentManager
    {
        IEnumerable<IComponent> GetComponents(Type type, params QualifierAttribute[] qualifiers);
        IComponent GetComponent(Type type, params QualifierAttribute[] qualifiers);
        IComponent GetComponent(IInjectionPoint injectionPoint);
        object GetReference(Type proxyType, IComponent component, ICreationalContext creationalContext);
        object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component, ICreationalContext context);
        string Id { get;}
        ICreationalContext CreateCreationalContext(IContextual contextual);
    }
}