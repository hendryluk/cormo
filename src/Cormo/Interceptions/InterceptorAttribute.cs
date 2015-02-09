using System;
using Cormo.Injects;

namespace Cormo.Interceptions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InterceptorAttribute: Attribute, IBinderAttribute
    {
         
    }
}