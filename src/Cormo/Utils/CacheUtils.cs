using System.Runtime.Remoting.Contexts;
using Cormo.Impl.Weld.Components;

namespace Cormo.Utils
{
    public static class CacheUtils
    {
        public static BuildPlan Cache(BuildPlan plan)
        {
            BuildPlan nextPlan = context =>
            {
                var result = plan(context);
                nextPlan = _ => result;
                return result;
            };

            return context => nextPlan(context);
        }
    }
}