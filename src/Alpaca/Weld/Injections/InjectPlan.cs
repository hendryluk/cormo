using Alpaca.Contexts;
using Alpaca.Injects;

namespace Alpaca.Weld.Injections
{
    public delegate object InjectPlan(object target, ICreationalContext context, IInjectionPoint ip);
}