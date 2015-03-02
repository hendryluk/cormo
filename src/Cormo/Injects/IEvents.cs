using System.Threading.Tasks;

namespace Cormo.Injects
{
    public interface IEvents<T>
    {
        void Fire(T @event);
        Task FireAsync(T @event);
    }
}