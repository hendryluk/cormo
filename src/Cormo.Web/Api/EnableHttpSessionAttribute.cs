using System;
using Cormo.Injects;
using Cormo.Web.Impl;

namespace Cormo.Web.Api
{
    [Import(typeof(HttpSessionContextRegistrar))]
    public class EnableHttpSessionStateAttribute : StereotypeAttribute
    {
    }
}