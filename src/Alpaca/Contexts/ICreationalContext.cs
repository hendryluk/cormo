namespace Alpaca.Contexts
{
    public interface ICreationalContext
    {
        void Push(object incompleteInstance);
        void Release();
    }
}