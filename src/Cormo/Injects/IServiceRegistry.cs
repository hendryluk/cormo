namespace Cormo.Injects
{
    public interface IServiceRegistry
    {
        T GetService<T>();
    }
}