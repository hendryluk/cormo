using Cormo.Injects;
using Cormo.Reflects;

namespace Cormo.Events
{
    public interface IProcessAnnotatedType
    {
        IAnnotatedType AnnotatedType { get; }
        void SetAnnotations(IAnnotations annotations);
        void Veto();
    }

   
}