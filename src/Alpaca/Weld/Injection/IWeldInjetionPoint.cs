using System;
using System.Collections.Generic;
using Alpaca.Injects;

namespace Alpaca.Weld.Injection
{
    public interface IWeldInjetionPoint : IInjectionPoint
    {
        IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations);
        void Inject(object target);
        IComponent Component { get; }
        Type Scope { get; }
    }
}