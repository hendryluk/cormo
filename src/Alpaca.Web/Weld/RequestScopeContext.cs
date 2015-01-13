using System;
using Alpaca.Context;
using Alpaca.Web.Contexts;

namespace Alpaca.Web.Weld
{
    public class RequestScopeContext: IContext
    {
        public Type Scope { get { return typeof (RequestScopedAttribute); } }
    }
}