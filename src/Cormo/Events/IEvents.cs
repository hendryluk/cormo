namespace Cormo.Events
{
    public interface IEvents<T>
    {
        void Fire(T @event);
    }
}