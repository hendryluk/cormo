using System;
using Cormo.Injects;

namespace Cormo.Catch
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class HandlesAttribute :Attribute
    {
    }
}