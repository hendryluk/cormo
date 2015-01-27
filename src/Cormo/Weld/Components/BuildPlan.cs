using Cormo.Contexts;
using Cormo.Injects;

namespace Cormo.Weld.Components
{
    public delegate object BuildPlan(ICreationalContext context, IInjectionPoint ip);
}