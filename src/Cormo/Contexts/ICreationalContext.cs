using Cormo.Injects;

namespace Cormo.Contexts
{
    public interface ICreationalContext
    {
        void Push(object incompleteInstance);
        void Release();
        ICreationalContext GetCreationalContext(IContextual component);      
    }
}