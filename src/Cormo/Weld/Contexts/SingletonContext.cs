using System;
using Cormo.Contexts;
using Cormo.Weld.Injections;

namespace Cormo.Weld.Contexts
{
    public class SingletonContext: AbstractSharedContext, ISingletonContext
    {
        public override Type Scope { get { return typeof (SingletonAttribute); } }
    }
}