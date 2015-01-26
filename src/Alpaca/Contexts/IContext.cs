using System;
using Alpaca.Injects;

namespace Alpaca.Contexts
{
    public interface IContext
    {
        /// <summary>
        /// Get the scope type of the context object.
        /// </summary>
        Type Scope { get; }

        bool IsActive { get; }

        /// <summary>
        /// Return an existing instance of certain contextual type or create a new 
        /// instance by calling <see cref="IContextual.Create(ICreationalContext)"/>
        /// and return the new instance.
        /// </summary>
        /// <param name="contextual">the contextual type</param>
        /// <param name="creationalContext">the context in which the new instance will be created</param>
        /// <param name="injectionPoint"></param>
        /// <returns>the contextual instance, or a null value</returns>
        object Get(IContextual contextual, ICreationalContext creationalContext, IInjectionPoint injectionPoint);
        
        /// <summary>
        /// Return an existing instance of a certain contextual type or a null value.
        /// </summary>
        /// <param name="contextual">the contextual type</param>
        /// <returns>the contextual instance, or a null value</returns>
        object Get(IContextual contextual);
    }
}