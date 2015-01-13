using System;
using Alpaca.Inject;

namespace Alpaca.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [Import(typeof(WebApiRegistrar))]
    public sealed class EnableWebApiAttribute: Attribute
    {
    }
}