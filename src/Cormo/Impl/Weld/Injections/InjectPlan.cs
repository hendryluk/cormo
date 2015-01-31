using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Injections
{
    public delegate object InjectPlan(object target, ICreationalContext context, IInjectionPoint ip);
}