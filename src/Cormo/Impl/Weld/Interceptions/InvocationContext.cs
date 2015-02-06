using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Cormo.Interceptions;

namespace Cormo.Impl.Weld.Interceptions
{
    public class InvocationContext : IInvocationContext
    {
        private readonly IInvocation _castleInvocation;
        private LinkedListNode<IAroundInvokeInterceptor> _nextInterceptor;
        private readonly bool _isAsync;
        private readonly ITaskCaster _taskCaster;

        public InvocationContext(IInvocation castleInvocation, LinkedListNode<IAroundInvokeInterceptor> nextInterceptor, bool isAsync, ITaskCaster taskCaster)
        {
            _castleInvocation = castleInvocation;
            _nextInterceptor = nextInterceptor;
            _isAsync = isAsync;
            _taskCaster = taskCaster;
        }

        public object Target { get { return _castleInvocation.InvocationTarget; } }
        public MethodInfo Method { get { return _castleInvocation.Method; } }
        public Task<object> Proceed()
        {
            var interceptor = _nextInterceptor;
            if (_nextInterceptor != null)
            {
                _nextInterceptor = interceptor.Next;
                return interceptor.Value.AroundInvoke(this);
            }

            _castleInvocation.Proceed();
            var returnValue = _castleInvocation.ReturnValue;
            if (_isAsync)
            {
                if (_taskCaster == null || returnValue == null)
                    return Task.FromResult((object)null);
                return _taskCaster.ToObject((Task)returnValue);
            }

            return Task.FromResult(returnValue);
        }

        public object[] Arguments
        {
            get { return _castleInvocation.Arguments; }
            set
            {
                for (var i = 0; i < value.Length; i++)
                    _castleInvocation.SetArgumentValue(i, value);
            }
        }
        public IDictionary<string, object> ContextData { get; private set; }
    }
}