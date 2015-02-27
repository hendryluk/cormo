using System;

namespace Cormo.Injects
{
    [AttributeUsage(AttributeTargets.Class)]
    public class VetoAttribute: Attribute, IAnnotation
    {
         
    }
}