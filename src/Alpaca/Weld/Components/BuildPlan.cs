using Alpaca.Contexts;
using Alpaca.Injects;

namespace Alpaca.Weld.Components
{
    public delegate object BuildPlan(ICreationalContext context, IInjectionPoint ip);
}