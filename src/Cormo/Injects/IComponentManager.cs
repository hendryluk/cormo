using System;
using System.Collections.Generic;
using Cormo.Contexts;

namespace Cormo.Injects
{
    public interface IComponentManager
    {
        IEnumerable<IComponent> GetComponents(Type type, params IQualifier[] qualifiers);
        IComponent GetComponent(Type type, params IQualifier[] qualifiers);
        IComponent GetComponent(IInjectionPoint injectionPoint);
        object GetReference(IComponent component, ICreationalContext creationalContext, params Type[] proxyTypes);
        object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component, ICreationalContext context);
        string Id { get;}
        ICreationalContext CreateCreationalContext(IContextual contextual);
    }
}