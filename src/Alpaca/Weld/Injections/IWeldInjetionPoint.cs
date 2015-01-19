using System;
using System.Collections.Generic;
using Alpaca.Inject;

namespace Alpaca.Weld.Injections
{
    public interface IWeldInjetionPoint : IInjectionPoint
    {
        IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations);
        void Inject(object target);
        IComponent Component { get; }
        Type Scope { get; }
    }
}