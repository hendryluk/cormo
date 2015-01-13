using System;

namespace Alpaca.Context
{
    public interface IContext
    {
        Type Scope { get; }
    }
}