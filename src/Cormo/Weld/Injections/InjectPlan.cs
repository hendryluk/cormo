using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Weld.Injections
{
    public delegate object InjectPlan(object target, ICreationalContext context, IInjectionPoint ip);
}