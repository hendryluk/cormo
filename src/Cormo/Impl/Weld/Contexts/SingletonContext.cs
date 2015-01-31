using System;
using Cormo.Contexts;

namespace Cormo.Impl.Weld.Contexts
{
    public class SingletonContext: AbstractSharedContext, ISingletonContext
    {
        public override Type Scope { get { return typeof (SingletonAttribute); } }
    }
}