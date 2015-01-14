using System;
using Alpaca.Contexts;
using Alpaca.Weld.Context;

namespace Alpaca.Web.Weld.Context
{
    public class HttpRequestContext: AbstractContext
    {
        protected override void GetComponentStore()
        {
            throw new NotImplementedException();
        }

        public override Type Scope { get { return typeof(RequestScopedAttribute); } }
    }
}