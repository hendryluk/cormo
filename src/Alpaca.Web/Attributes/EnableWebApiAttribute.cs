using System;
using Alpaca.Weld;
using Alpaca.Weld.Attributes;

namespace Alpaca.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [Import(typeof(WebApiRegistrar))]
    public sealed class EnableWebApiAttribute: Attribute
    {
    }
}