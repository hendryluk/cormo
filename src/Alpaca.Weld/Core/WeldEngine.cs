using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld.Core
{
    public class WeldEngine
    {
        private readonly WeldCatalog _catalog;
        //private Dictionary<Type, DependencyInjector[]> _typeDependencies;
        private Dictionary<ComponentRegistration, object> _componentValues = new Dictionary<ComponentRegistration, object>();

        public WeldEngine(WeldCatalog catalog)
        {
            _catalog = catalog;
        }

        public void Run()
        {
            BuildIndex();
            BuildDependencyGraph();
            Configure();
        }

        public class InjectionPoint
        {
            public MemberInfo Member { get; private set; }
            public Type RequestedType { get; private set; }
            public IEnumerable<object> Qualifiers { get; private set; }

            public InjectionPoint(MemberInfo member, Type requestedType, IEnumerable<object> qualifiers)
            {
                Member = member;
                RequestedType = requestedType;
                Qualifiers = qualifiers; 
            }

            public override string ToString()
            {
                return string.Format("type [{0}] with qualifiers [{1}] at injection point [{2}]",
                    RequestedType, string.Join(",", Qualifiers.Select(x => x.GetType().Name)), Member);
            }
        }

        IDictionary<Type, MethodInfo[]> _postConstructs;
        Dictionary<ComponentRegistration, ComponentGraphNode> _nodeIndex = new Dictionary<ComponentRegistration, ComponentGraphNode>();
        Dictionary<Type, ComponentRegistration[]> _regIndex;
        Dictionary<Type, InjectRegistration[]> _injectIndex;
        List<ComponentRegistration> _unprocessedRegistrations;

        void BuildIndex()
        {
            _postConstructs = _catalog.PostConstructs.GroupBy(x => x.ReflectedType).ToDictionary(x => x.Key, x => x.ToArray());
        }

        // GRAPH
        public class DependencyLink
        {
            public bool AllowMultiple { get; set; }
            public List<ComponentGraphNode> Components = new List<ComponentGraphNode>();
            public InjectionPoint InjectionPoint { get; set; }

            public void AddAll(ComponentGraphNode[] nodes)
            {
                Components.AddRange(nodes);
                foreach (var node in nodes)
                {
                    node.Dependents.Add(this);
                }
            }
        }

        public class ComponentGraphNode
        {
            public ComponentRegistration Registration { get; private set; }
            public ConcurrentBag<DependencyLink> Dependents = new ConcurrentBag<DependencyLink>();

            public ComponentGraphNode(ComponentRegistration reg)
            {
                Registration = reg;
            }

            public IDictionary<MemberInfo, DependencyLink[]> Dependencies { get; set; }
        }
        
        void BuildDependencyGraph()
        {
            var components = _catalog.Components.Union(_catalog.Configurations).ToArray();
            _regIndex = (from component in components
                         from type in TypeUtils.GetComponentTypes(component.Type)
                         let openType = type.ContainsGenericParameters? type.GetGenericTypeDefinition(): type
                         group component by openType)
                         .ToDictionary(x => x.Key, x => x.ToArray());
            _injectIndex = _catalog.InjectRegistrations.GroupBy(x => x.MemberInfo.ReflectedType).ToDictionary(x=> x.Key, x=> x.ToArray());

            _nodeIndex = components.ToDictionary(x => x, x => new ComponentGraphNode(x));

            var processing = components;
            while (processing.Any())
            {
                _unprocessedRegistrations = new List<ComponentRegistration>();
                
                foreach (var reg in processing)
                {
                    var node = _nodeIndex[reg];
                    
                    var injects = GetTypeInjects(reg.Type);
                    node.Dependencies = injects.ToDictionary(x => x.MemberInfo, x => x.Dependencies.Select(y => ResolveDependency(x.MemberInfo, y)).ToArray());
                }
                processing = _unprocessedRegistrations.ToArray();
            }
            
        }

        private InjectRegistration[] GetTypeInjects(Type type)
        {
            InjectRegistration[] injects;
            if (!_injectIndex.TryGetValue(type, out injects))
            {
                if (type.IsGenericType)
                {
                    var translations = GenericUtils.CreateGenericTranslactions(type);

                    InjectRegistration[] genericInjects;
                    if (!_injectIndex.TryGetValue(type.GetGenericTypeDefinition(), out genericInjects))
                        genericInjects = new InjectRegistration[0];

                    genericInjects = genericInjects.Select(x =>
                    {
                        var resolvedMethod = GenericUtils.TranslateMemberGenericArguments(x.MemberInfo, translations);
                        return new InjectRegistration(resolvedMethod, x.Dependencies);
                    }).ToArray();

                    _injectIndex[type] = injects = genericInjects;
                }
                else
                    injects = new InjectRegistration[0];
            }


            ValidateInjects(type, injects);
            return injects;
        }

        private MethodInfo[] GetPostConstructs(Type type)
        {
            MethodInfo[] postConstructs;
            if (_postConstructs.TryGetValue(type, out postConstructs))
                return postConstructs;

            if (type.IsGenericType)
            {
                if (_postConstructs.TryGetValue(type, out postConstructs))
                {
                    var translations = GenericUtils.CreateGenericTranslactions(type);
                    postConstructs = postConstructs.Select(x=> GenericUtils.TranslateMemberGenericArguments(x, translations)).Cast<MethodInfo>().ToArray();
                    return postConstructs;
                }
            }

            return new MethodInfo[0];
        }

        private void ValidateInjects(Type type, InjectRegistration[] injects)
        {
            object[] ctors = injects.Select(x => x.MemberInfo).OfType<ConstructorInfo>().ToArray();
            if (ctors.Length > 2)
            {
                throw new InvalidComponentException(type, string.Format("Multiple [Inject] constructors: [{0}]", string.Join(",", ctors)));
            }
        }

        private DependencyLink ResolveDependency(MemberInfo member, SeekSpec spec)
        {
            var link = new DependencyLink
            {
                InjectionPoint = new InjectionPoint(member, spec.Type, spec.Qualifiers),
                AllowMultiple = spec.Multiple
            };

            ComponentRegistration[] registrations;
            
            if (!_regIndex.TryGetValue(spec.Type, out registrations))
            {
                if (spec.Type.IsGenericType)
                {
                    if (_regIndex.TryGetValue(spec.Type.GetGenericTypeDefinition(), out registrations))
                    {
                        registrations = registrations
                            .SelectMany(x => CloseRegistrationGenerics(x, spec.Type))
                            .ToArray();
                    }
                }
            }

            if (registrations == null || !registrations.Any())
            {
                if (!link.AllowMultiple)
                    throw new UnsatisfiedDependencyException(link.InjectionPoint);
            }
            else
            {
                var matched = registrations.Select(x => x.CanSatisfy(spec)).Where(x=> x!=null).ToArray();
                link.AddAll(matched.Select(x=> _nodeIndex[x]).ToArray());
            }

            return link;
        }

        private IEnumerable<ComponentRegistration> CloseRegistrationGenerics(ComponentRegistration reg, Type type)
        {
            var registration = reg.CanSatisfy(type);
            if (registration != null)
            {
                _unprocessedRegistrations.Add(registration);
                _nodeIndex.Add(registration, new ComponentGraphNode(registration));
                yield return registration;
            }
        }

        // \GRAPH

        private void Configure()
        {
            foreach (var config in _catalog.Configurations)
            {
                // Load Imports
                GetInstance(config);
            }
        }

        public object Execute(object target, MethodBase method, ComponentRegistration[][] registrations)
        {
            if (method.IsStatic)
            {
                target = null;
            }

            // TODO
            return null;
        }

        private readonly ConcurrentDictionary<ComponentRegistration, BuildPlan> _buildPlans = new ConcurrentDictionary<ComponentRegistration, BuildPlan>(); // temporary quick implementation
        
        private readonly ConcurrentDictionary<ComponentRegistration, object> _singletonInstances = new ConcurrentDictionary<ComponentRegistration, object>(); // temporary quick implementation
        public object GetInstance(ComponentRegistration registration)
        {
            return _singletonInstances.GetOrAdd(registration, BuildComponent);
        }

        private object BuildComponent(ComponentRegistration registration)
        {
            var buildPlan = _buildPlans.GetOrAdd(registration, r=> r.GetBuildPlan(this));
            return buildPlan();
        }

        public BuildPlan MakeConstructorBuildPlan(ComponentRegistration registration)
        {
            var node = _nodeIndex[registration];
            var injectCtors = node.Dependencies.Where(x => x.Key is ConstructorInfo).ToArray();
            var postConstructs = GetPostConstructs(registration.Type);

            BuildPlan construction;
            if (injectCtors.Any())
            {
                var inject = injectCtors.First();
                construction = ()=> MakeExecutionPlan((MethodBase)inject.Key, inject.Value)(null);
            }
            else
            {
                construction = () => Activator.CreateInstance(registration.Type, true);
            }

            var injectPlans = node.Dependencies.Except(injectCtors).Select(x=> MakeInjectionPlan(x.Key, x.Value)).ToArray();

            return () =>
            {
                var obj = construction();
                foreach (var inject in injectPlans)
                    inject(obj);
                foreach (var post in postConstructs)
                    post.Invoke(obj, null);
                return obj;
            };
        }

        public InjectPlan MakeInjectionPlan(MemberInfo member, DependencyLink[] dependencies)
        {
            return MemberInfoVisitor.VisitInject(member,
                method => MakeExecutionPlan(method, dependencies),
                field => MakeExecutionPlan(field, dependencies.First().Components.First().Registration),
                property => MakeExecutionPlan(property, dependencies.First().Components.First().Registration)
                );
        }

        public InjectPlan MakeExecutionPlan(FieldInfo field, ComponentRegistration reg)
        {
            return o =>
            {
                var val = GetInstance(reg);
                field.SetValue(o, val);
                return val;
            };
        }

        public InjectPlan MakeExecutionPlan(PropertyInfo property, ComponentRegistration reg)
        {
            return o =>
            {
                var val = GetInstance(reg);
                property.SetValue(o, val);
                return val;
            };
        }

        public InjectPlan MakeExecutionPlan(MethodBase method, DependencyLink[] dependencies)
        {
            var regs = dependencies.Select(x => x.Components.First().Registration).ToArray();
            return o =>
            {
                var args = regs.Select(GetInstance).ToArray();
                return method.Invoke(o, args);
            };
        }
    }
}