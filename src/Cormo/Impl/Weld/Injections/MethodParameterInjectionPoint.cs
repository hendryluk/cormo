using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Injections
{
    public class MethodParameterInjectionPoint : AbstractInjectionPoint, IMethodParameterInjectionPoint
    {
        private readonly ParameterInfo _param;

        public ParameterInfo ParameterInfo { get { return _param; } }
        
        public MethodParameterInjectionPoint(IComponent declaringComponent, ParameterInfo paramInfo, IAnnotations annotations) 
            : base(declaringComponent, paramInfo.Member, paramInfo.ParameterType, annotations)
        {
            _param = paramInfo;
            IsConstructor = _param.Member is ConstructorInfo;
            
        }

        public bool IsConstructor { get; private set; }
        public int Position { get { return _param.Position; } }

        public override IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            throw new NotSupportedException();
        }

        protected override InjectPlan BuildInjectPlan(IComponent component)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return string.Format("parameter [{0}] of {1}", Formatters.Parameter(_param), Formatters.DescribeMethodBase((MethodBase) _param.Member));
        }
    }
}