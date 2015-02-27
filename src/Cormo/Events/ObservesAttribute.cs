using System;
using Cormo.Injects;

namespace Cormo.Events
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ObservesAttribute: Attribute, IAnnotation
    {
         
    }
}