using System;
using Cormo.Injects;

namespace Cormo.Data.EntityFramework.Api
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class EntityContextAttribute : QualifierAttribute
    {
        public string ConnectionName { get; private set; }

        public EntityContextAttribute(): this("Default")
        {
        }

        public EntityContextAttribute(string connectionName)
        {
            ConnectionName = connectionName;
        }
    }
}