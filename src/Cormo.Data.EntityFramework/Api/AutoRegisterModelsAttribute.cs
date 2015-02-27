using System;
using Cormo.Interceptions;

namespace Cormo.Data.EntityFramework.Api
{
    /// <summary>
    /// A DbContext implementation class that is annotated with this attribute will automatically be registered Entities referenced via Cormo
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegisterModelsAttribute : InterceptorBindingAttribute
    {
        
    }
}