using Cormo.Reflects;

namespace Cormo.Injects.Events
{
    public interface IProcessAnnotatedType
    {
        IAnnotatedType AnnotatedType { get; }
        void SetAnnotations(IAnnotations annotations);
    }

   
}