using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cormo.Injects;

namespace Cormo.Impl.Weld
{
    public class CurrentInjectionPoint
    {
        private static readonly ThreadLocal<Stack<IInjectionPoint>> _injectionPoints = 
            new ThreadLocal<Stack<IInjectionPoint>>(()=> new Stack<IInjectionPoint>());

        public void Push(IInjectionPoint injectionPoint)
        {
            _injectionPoints.Value.Push(injectionPoint);
        }

        public IInjectionPoint Pop()
        {
            return _injectionPoints.Value.Pop();
        }

        public IInjectionPoint Peek()
        {
            return _injectionPoints.Value.FirstOrDefault();
        }
    }
}