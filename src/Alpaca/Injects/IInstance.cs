using System.Collections.Generic;

namespace Alpaca.Injects
{
    public interface IInstance<out T>: IEnumerable<T>
    {
        T Value { get; }
    }
}