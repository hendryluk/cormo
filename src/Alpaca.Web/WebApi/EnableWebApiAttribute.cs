using System;
using Alpaca.Injects;
using Alpaca.Web.Weld;

namespace Alpaca.Web.WebApi
{
    [AttributeUsage(AttributeTargets.Class)]
    [Import(typeof(WebApiRegistrar))]
    public sealed class EnableWebApiAttribute: Attribute
    {
    }
}