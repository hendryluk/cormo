using System;
using Cormo.Interceptions;

namespace Cormo.Catch
{
    public interface ICaughtException<T> where T:Exception
    {
        /// <summary>
        /// Instructs the dispatcher to terminate additional handler processing and mark the event as handled.
        /// </summary>
        void Handled();

        /// <summary>
        /// Default instruction to dispatcher, continues handler processing.
        /// </summary>
        void MarkHandled();

        /// <summary>
        /// Instructs the dispatcher to abort further processing of handlers.
        /// </summary>
        void Abort();

        /// <summary>
        /// Instructs the dispatcher to rethrow the event exception after handler processing.
        /// </summary>
        void Rethrow();

        T Exception { get; }
        IInvocationContext InvocationContext { get; }
        Exception ThrownException { get; }
    }
}