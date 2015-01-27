namespace Cormo.Contexts
{
    /// <summary>
    /// Specifies that a component belongs to the dependent pseudo-scope.
    /// Components declared with scope [Dependent] behave differently 
    /// to components with other built-in scope types. When a component is declared 
    /// to have scope [Dependent]:
    /// <list type="bullet">
    /// <item>
    /// <description>No injected instance of the component is ever shared between 
    /// multiple injection points.</description>
    /// </item>
    /// <item><description>Any instance of the component injected into an object that is being 
    /// created by the container is bound to the lifecycle of the newly 
    /// created object.
    /// </description></item>
    /// <item><description>Any instance of the component that receives a producer method, 
    /// producer field, disposer method or observer method invocation 
    /// exists to service that invocation only.
    /// </description></item>
    /// <item><description>Any instance of the component that receives a producer method,
    /// producer field, disposer method or observer method invocation exists to service that invocation only.
    /// </description></item>
    /// <item><description>Any instance of the component injected into method parameters of a 
    /// disposer method or observer method exists to service the method 
    /// invocation only.
    /// </description></item>
    /// </list>
    /// </summary>
    public class DependentAttribute : ScopeAttribute
    {
        
    }
}