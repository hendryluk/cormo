using System;

namespace Alpaca.Contexts
{
    public interface IContext
    {
        Type Scope { get; }
    }
}