using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cormo.Events;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Utils;

namespace Cormo.Impl.Weld.Resolutions
{
    public class ComponentResolver : ContextualResolver<IWeldComponent, ComponentResolvable>
    {
        private readonly ConcurrentDictionary<Type, IWeldComponent[]> _typeComponents = new ConcurrentDictionary<Type, IWeldComponent[]>();

        public ComponentResolver(WeldComponentManager manager, IEnumerable<IWeldComponent> allComponents) 
            : base(manager, allComponents)
        {
        }

        protected override IEnumerable<IWeldComponent> Resolve(ComponentResolvable resolvable, ref IEnumerable<IWeldComponent> components)
        {
            var results = components.ToArray();
            
            if (GenericUtils.OpenIfGeneric(resolvable.Type) == typeof(IEvents<>))
                return new IWeldComponent[]
                {new EventComponent(resolvable.Type.GetGenericArguments()[0], new Annotations(resolvable.Qualifiers), Manager)};
            
            var unwrappedType = UnwrapType(resolvable.Type);
            var isWrapped = unwrappedType != resolvable.Type;

            results = _typeComponents.GetOrAdd(unwrappedType, t =>
                results.Select(x => x.Resolve(t)).Where(x => x != null).ToArray());

            results = results.Where(c => c.Qualifiers.CanSatisfy(resolvable.Qualifiers)).ToArray();

            if (results.Length > 1)
            {
                var onMissings = results.Where(x => x.IsConditionalOnMissing).ToArray();
                var others = results.Except(onMissings).ToArray();

                results = others.Any() ? others : onMissings.Take(1).ToArray();
            }

            foreach (var result in results)
                result.Touch();
            components = results;
            
            return isWrapped
                ? new IWeldComponent[] {new InstanceComponent(unwrappedType, new Annotations(resolvable.Qualifiers), Manager, results)}
                : results;
        }
    }
}