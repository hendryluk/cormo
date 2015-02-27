using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld;
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
        IAnnotatedType Resolve(Type type);
    }

    public interface IAnnotated
    {
        IAnnotations Annotations { get; }
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
}