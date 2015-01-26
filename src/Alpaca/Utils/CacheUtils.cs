using System.Runtime.Remoting.Contexts;
using Alpaca.Weld.Components;
using Alpaca.Weld.Injections;

namespace Alpaca.Utils
{
    public static class CacheUtils
    {
        public static BuildPlan Cache(BuildPlan plan)
        {
            BuildPlan nextPlan = (context, ip) =>
            {
                var result = plan(context, ip);
                nextPlan = (_, __) => result;
                return result;
            };

            return (context, ip) => nextPlan(context, ip);
        }
    }
}