using System.Collections.Generic;

namespace Cormo.Injects
{
    public interface IInstance<out T>: IEnumerable<T>
    {
        T Value { get; }
    }
}