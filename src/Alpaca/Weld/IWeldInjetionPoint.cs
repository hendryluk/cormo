using System;
using System.Collections.Generic;
using Alpaca.Contexts;
using Alpaca.Injects;

namespace Alpaca.Weld
{
    public interface IWeldInjetionPoint : IInjectionPoint
    {
        IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations);
        void Inject(object target);
        IComponent Component { get; }
        Type Scope { get; }
    }
}