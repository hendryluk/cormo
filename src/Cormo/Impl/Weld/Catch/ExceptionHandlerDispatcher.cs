using System;
using System.Collections.Generic;
using System.Linq;
using Cormo.Catch;
using Cormo.Impl.Weld.Introspectors;
using Cormo.Impl.Weld.Resolutions;
using Cormo.Injects;
using Cormo.Interceptions;

namespace Cormo.Impl.Weld.Catch
{
    public interface IExceptionHandlerDispatcher
    {
        bool Dispatch(IInvocationContext invocationContext, Exception exception, params IQualifier[] qualifiers);
    }

    public class ExceptionHandlerDispatcher : IExceptionHandlerDispatcher
    {
        private readonly ObserverResolver _handlerResolver;

        public ExceptionHandlerDispatcher(WeldComponentManager manager, IEnumerable<EventObserverMethod> handlers)
        {
            _handlerResolver = new ObserverResolver(manager, handlers.ToArray());
        }

        public bool Dispatch(IInvocationContext invocationContext, Exception exception, IQualifier[] qualifierArray)
        {
            var qualifiers = new Qualifiers(qualifierArray);
            var context = new ExceptionHandlingContext(invocationContext, exception, qualifiers);
            DispatchCauses(exception, context, qualifiers);

            return context.IsMarkedHandled == true;
        }

        private void DispatchCauses(Exception exception, ExceptionHandlingContext context, IQualifiers qualifiers)
        {
            if (context.StopsHandling)
                return;

            if (exception.InnerException != null)
            {
                DispatchCauses(exception.InnerException, context, qualifiers);
                if (context.StopsHandling)
                    return;
            }

            DispatchSpecificException((dynamic)exception, context, qualifiers);
        }

        private void DispatchSpecificException<T>(T exception, ExceptionHandlingContext context, IQualifiers qualifiers) where T:Exception
        {
            var caught = new CaughtException<T>(exception, context);
            var handlers = _handlerResolver.Resolve(new ObserverResolvable(caught.GetType(), qualifiers));

            foreach (var handler in handlers)
            {
                context.IsMarkedHandled = context.IsMarkedHandled ?? true;
    
                handler.Notify(caught);
                if (caught.IsMarkedHandled)
                    break;
            }
        }
    }

    public class ExceptionHandlingContext
    {
        public IInvocationContext InvocationContext { get; private set; }
        public Exception Exception { get; private set; }
        public IQualifiers Qualifiers { get; private set; }

        public ExceptionHandlingContext(IInvocationContext invocationContext, Exception exception, IQualifiers qualifiers)
        {
            InvocationContext = invocationContext;
            Exception = exception;
            Qualifiers = qualifiers;
        }

        public bool? IsMarkedHandled { get; set; }
        public bool StopsHandling { get; set; }
    }

    public class CaughtException<T> : ICaughtException<T> where T : Exception
    {
        private readonly ExceptionHandlingContext _context;
        public T Exception { get; private set; }

        public CaughtException(T exception, ExceptionHandlingContext context)
        {
            _context = context;
            Exception = exception;
        }

        public bool IsMarkedHandled { get { return _context.IsMarkedHandled==true; } }
        public bool StopsHandling { get { return _context.StopsHandling; } }

        public IInvocationContext InvocationContext
        {
            get { return _context.InvocationContext; }
        }

        public IQualifiers Qualifiers { get { return _context.Qualifiers; }  }

        public Exception ThrownException
        {
            get { return _context.Exception; }
        }

        public void Handled()
        {
            _context.IsMarkedHandled = _context.StopsHandling = true;
        }

        public void MarkHandled()
        {
            _context.IsMarkedHandled = true;
        }

        public void Abort()
        {
            _context.StopsHandling = true;
        }

        public void Rethrow()
        {
            _context.IsMarkedHandled = false;
        }
    }
}