using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Reflects
{
    public interface IAnnotatedType : IAnnotated
    {
        Type Type { get; }
        IEnumerable<IAnnotatedField> Fields { get; }
        IEnumerable<IAnnotatedMethod> Methods { get; }
        IEnumerable<IAnnotatedProperty> Properties { get; }
        IEnumerable<IAnnotatedConstructor> Constructors { get; }
        IAnnotatedType Resolve(Type resolvedType);
    }

    public interface IAnnotated
    {
        IBinders Binders { get; }
    }

    public interface IAnnotatedField : IAnnotated
    {
        FieldInfo Field { get; }
    }

    public interface IAnnotatedProperty : IAnnotated
    {
        PropertyInfo Property { get; }
    }

    public interface IAnnotatedMethod : IAnnotated
    {
        MethodInfo Method { get; }
        IEnumerable<IAnnotatedMethodParameter> Parameters { get; }
    }

    public interface IAnnotatedConstructor : IAnnotated
    {
        ConstructorInfo Constructor { get; }
        IEnumerable<IAnnotatedMethodParameter> Parameters { get; }
    }

    public interface IAnnotatedMethodParameter : IAnnotated
    {
        ParameterInfo Parameter { get; }
    }

    public class AnnotatedType : IAnnotatedType
    {
        private readonly Lazy<IEnumerable<IAnnotatedField>> _fieldsLazy;
        private readonly Lazy<IEnumerable<IAnnotatedMethod>> _methodsLazy;
        private readonly Lazy<IEnumerable<IAnnotatedProperty>> _propertiesLazy;
        private readonly Lazy<IEnumerable<IAnnotatedConstructor>> _constructorsLazy;

        public Type Type { get; private set; }
        public IBinders Binders { get; private set; }
        public IEnumerable<IAnnotatedField> Fields { get { return _fieldsLazy.Value; } }
        public IEnumerable<IAnnotatedMethod> Methods { get { return _methodsLazy.Value; } }
        public IEnumerable<IAnnotatedProperty> Properties { get { return _propertiesLazy.Value; }}
        public IEnumerable<IAnnotatedConstructor> Constructors { get { return _constructorsLazy.Value; } }
        public virtual IAnnotatedType Resolve(Type resolvedType)
        {
            return new AnnotatedType(resolvedType);
        }

        private const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        public AnnotatedType(Type type)
        {
            Type = type;
            Binders = type.GetBinders();
            _fieldsLazy = new Lazy<IEnumerable<IAnnotatedField>>(GetFields);
            _methodsLazy = new Lazy<IEnumerable<IAnnotatedMethod>>(GetMethods);
            _propertiesLazy = new Lazy<IEnumerable<IAnnotatedProperty>>(GetProperties);
            _constructorsLazy = new Lazy<IEnumerable<IAnnotatedConstructor>>(GetConstructors);
        }

        protected virtual IEnumerable<IAnnotatedField> GetFields()
        {
            return ScannerUtils.GetAllField(Type, AllBindingFlags).Select(x => new AnnotatedField(x)).ToArray();
        }

        protected virtual IEnumerable<IAnnotatedMethod> GetMethods()
        {
            return Type.GetMethods(AllBindingFlags).Select(x => new AnnotatedMethod(x)).ToArray();
        }

        protected virtual IEnumerable<IAnnotatedProperty> GetProperties()
        {
            return Type.GetProperties(AllBindingFlags).Select(x => new AnnotatedProperty(x)).ToArray();
        }

        protected virtual IEnumerable<IAnnotatedConstructor> GetConstructors()
        {
            return Type.GetConstructors(AllBindingFlags).Select(x => new AnnotatedConstructor(x)).ToArray();
        }
    }

    public class AnnotatedField : IAnnotatedField
    {
        public AnnotatedField(FieldInfo field)
        {
            Field = field;
            Binders = field.GetBinders();
        }

        public virtual FieldInfo Field { get; private set; }
        public virtual IBinders Binders { get; private set; }
    }

    public class AnnotatedMethod : IAnnotatedMethod
    {
        private readonly Lazy<IEnumerable<IAnnotatedMethodParameter>> _parametersLazy;
        
        public AnnotatedMethod(MethodInfo method)
        {
            Method = method;
            Binders = method.GetBinders();

            _parametersLazy = new Lazy<IEnumerable<IAnnotatedMethodParameter>>(GetParameters);
        }

        public virtual MethodInfo Method { get; private set; }
        public virtual IBinders Binders { get; private set; }
        public IEnumerable<IAnnotatedMethodParameter> Parameters { get { return _parametersLazy.Value; } }
        
        protected virtual IEnumerable<IAnnotatedMethodParameter> GetParameters()
        {
            return Method.GetParameters().Select(x => new AnnotatedMethodParameter(x)).ToArray();
        }
    }

    public class AnnotatedConstructor : IAnnotatedConstructor
    {
        private readonly Lazy<IEnumerable<IAnnotatedMethodParameter>> _parametersLazy;

        public AnnotatedConstructor(ConstructorInfo constructor)
        {
            Constructor = constructor;
            Binders = constructor.GetBinders();

            _parametersLazy = new Lazy<IEnumerable<IAnnotatedMethodParameter>>(GetParameters);
        }

        public virtual ConstructorInfo Constructor { get; private set; }
        public virtual IBinders Binders { get; private set; }
        public IEnumerable<IAnnotatedMethodParameter> Parameters { get { return _parametersLazy.Value; } }

        protected virtual IEnumerable<IAnnotatedMethodParameter> GetParameters()
        {
            return Constructor.GetParameters().Select(x => new AnnotatedMethodParameter(x)).ToArray();
        }
    }

    public class AnnotatedMethodParameter: IAnnotatedMethodParameter
    {
        public AnnotatedMethodParameter(ParameterInfo parameter)
        {
            Parameter = parameter;
            Binders = parameter.GetBinders();
        }

        public ParameterInfo Parameter { get; private set; }
        public virtual IBinders Binders { get; private set; }
    }

    public class AnnotatedProperty : IAnnotatedProperty
    {
        public AnnotatedProperty(PropertyInfo property)
        {
            Property = property;
            Binders = property.GetBinders();
        }

        public virtual PropertyInfo Property { get; private set; }
        public virtual IBinders Binders { get; private set; }
    }
}