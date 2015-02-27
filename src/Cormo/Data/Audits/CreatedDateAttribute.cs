using System;

namespace Cormo.Data.Audits
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CreatedDateAttribute: Attribute
    {    
    }
}