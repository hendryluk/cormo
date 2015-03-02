using System;

namespace Cormo.Injects
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ObservesAttribute: Attribute, IAnnotation
    {
         
    }
}