using System.Linq;
using Cormo.Impl.Weld.Reflects;
using Cormo.Injects;
using Cormo.Injects.Events;
using Cormo.Reflects;

namespace Cormo.Impl.Weld.Events
{
    public class ProcessAnnotatedType: IProcessAnnotatedType
    {
        public IAnnotatedType AnnotatedType { get; private set; }
        public void SetAnnotations(IAnnotations annotations)
        {
            AnnotatedType = new AnnotatedType(AnnotatedType.Type, annotations);
        }

        public void Veto()
        {
            SetAnnotations(new Annotations(AnnotatedType.Annotations.Union(new[]{new VetoAttribute()})));
        }

        public ProcessAnnotatedType(IAnnotatedType annotatedType)
        {
            AnnotatedType = annotatedType;
        }
    }
}