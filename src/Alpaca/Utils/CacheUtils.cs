using System.Runtime.Remoting.Contexts;
using Alpaca.Weld.Components;
using Alpaca.Weld.Injections;

namespace Alpaca.Utils
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