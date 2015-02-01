using System;

namespace Cormo.Data.EntityFramework.Api.Audits
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class LastModifiedByAttribute : Attribute
    {
    }
}