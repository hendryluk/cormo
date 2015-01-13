using System;
using Alpaca.Inject;
using Alpaca.Web.Weld;

namespace Alpaca.Web.WebApi
{
    [AttributeUsage(AttributeTargets.Class)]
    [Import(typeof(WebApiRegistrar))]
    public sealed class EnableWebApiAttribute: Attribute
    {
    }
}