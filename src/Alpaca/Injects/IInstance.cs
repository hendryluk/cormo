using System.Collections.Generic;

namespace Alpaca.Inject
{
    public interface IInstance<out T>: IEnumerable<T>
    {
        T Value { get; }
    }
}