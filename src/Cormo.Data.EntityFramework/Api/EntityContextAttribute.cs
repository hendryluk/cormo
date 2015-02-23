using System;
using Cormo.Injects;

namespace Cormo.Data.EntityFramework.Api
{
    public class EntityContextAttribute : QualifierAttribute
    {
        // TODO: let's see later how to allow this, if at all
        //public EntityContextAttribute(): this("DefaultConnection")
        //{
        //}

        //public EntityContextAttribute(string connectionName)
        //{
        //    ConnectionName = connectionName;
        //}

        //public string ConnectionName { get; private set; }

    }
}