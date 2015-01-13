using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld
{
    public abstract class ComponentRegistration
    {
        protected ComponentRegistration(Type type, IEnumerable<object> qualifiers)
        {
            Type = type;
            Qualifiers = new HashSet<object>(qualifiers);
        }

        public Type Type { get; set; }
        public HashSet<object> Qualifiers { get; set; }

        public virtual ComponentRegistration CanSatisfy(SeekSpec spec)
        {
            if (!CanSatisfy(spec.Qualifiers))
                return null;
            return CanSatisfy(spec.Type);
        }

        private bool CanSatisfy(IEnumerable<object> qualifiers)
        {
            return qualifiers.All(Qualifiers.Contains);
        }

        public abstract ComponentRegistration CanSatisfy(Type requestedType);

        public abstract BuildPlan GetBuildPlan(WeldEngine engine);
    }

    public class InstanceComponentRegistration : ComponentRegistration
    {
        private readonly object _instance;

        public InstanceComponentRegistration(object instance, Type type, IEnumerable<object> qualifiers)
            : base(type, qualifiers)
        {
            _instance = instance;
        }

        public override ComponentRegistration CanSatisfy(Type requestedType)
        {
            if (requestedType.IsInstanceOfType(_instance))
                return this;
            return null;
        }

        public override BuildPlan GetBuildPlan(WeldEngine engine)
        {
            return () => _instance;
        }
    }

    public class ClassComponentRegistration : ComponentRegistration
    {
        private readonly bool _containsGenericParameters;

        public ClassComponentRegistration(Type type, IEnumerable<object> qualifiers)
            : base(type, qualifiers)
        {
            _containsGenericParameters = Type.ContainsGenericParameters;
        }

        public override ComponentRegistration CanSatisfy(Type requestedType)
        {
            if (!_containsGenericParameters)
                return this;

            var resolution = GenericUtils.ResolveGenericType(Type, requestedType);
            if (resolution == null || resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
                return null;

            return new ClassComponentRegistration(resolution.ResolvedType, Qualifiers);
        }

        public override BuildPlan GetBuildPlan(WeldEngine engine)
        {
            return engine.MakeConstructorBuildPlan(this);
        }
    }

    public class ProducerRegistration : ComponentRegistration
    {
        private readonly bool _containsGenericParameters;

        public ProducerRegistration(Type type, IEnumerable<object> qualifiers, MemberInfo producer)
            : base(type, qualifiers)
        {
            Producer = producer;
            _containsGenericParameters = GenericUtils.MemberContainsGenericArguments(producer);
        }

        public MemberInfo Producer { get; set; }

        public override ComponentRegistration CanSatisfy(Type requestedType)
        {
            if (!_containsGenericParameters)
                return this;

            var typeResolution = GenericUtils.ResolveGenericType(Type, requestedType);
            if (typeResolution == null || typeResolution.ResolvedType == null || typeResolution.ResolvedType.ContainsGenericParameters)
                return null;

            var resolvedProducer = GenericUtils.TranslateMemberGenericArguments(Producer, typeResolution.GenericParameterTranslations);
            if (GenericUtils.MemberContainsGenericArguments(resolvedProducer))
                return null;

            var method = resolvedProducer as MethodInfo;
            if (method != null)
                return new ProducerRegistration(typeResolution.ResolvedType, Qualifiers, resolvedProducer);

            return null;
        }

        public override BuildPlan GetBuildPlan(WeldEngine engine)
        {
            // TODO
            return null;
            //return engine.MakeExecutionPlan(Producer);
        }
    }
}