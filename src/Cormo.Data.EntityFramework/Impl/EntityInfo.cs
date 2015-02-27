using System;
using System.Linq;
using System.Reflection;
using Cormo.Data.Audits;
using Cormo.Impl.Utils;
using Cormo.Injects;
using Cormo.Injects.Exceptions;

namespace Cormo.Data.EntityFramework.Impl
{
    public class EntityInfo
    {
        private readonly AuditorSetter[] _createdBySetters;
        private readonly AuditDateSetter[] _createdDateSetters;
        private readonly AuditorSetter[] _lastModifiedBySetters;
        private readonly AuditDateSetter[] _lastModifiedDateSetters;

        public Type Type { get; private set; }

        public EntityInfo(IComponentManager manager, Type type)
        {
            Type = type;

            var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).ToArray();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).ToArray();

            _createdBySetters = 
                properties.Where(AttributeUtils.HasAttribute<CreatedByAttribute>)
                    .Select(x => new AuditorSetter(manager, x))
                    .Union(fields.Where(AttributeUtils.HasAttribute<CreatedByAttribute>)
                        .Select(x=> new AuditorSetter(manager, x)))
                    .ToArray();
            _createdDateSetters = properties.Where(AttributeUtils.HasAttribute<CreatedDateAttribute>)
                .Select(x => new AuditDateSetter(x))
                .Union(fields.Where(AttributeUtils.HasAttribute<CreatedDateAttribute>)
                    .Select(x => new AuditDateSetter(x)))
                .ToArray();
            _lastModifiedBySetters =
                properties.Where(AttributeUtils.HasAttribute<LastModifiedByAttribute>)
                    .Select(x => new AuditorSetter(manager, x))
                    .Union(fields.Where(AttributeUtils.HasAttribute<LastModifiedByAttribute>)
                        .Select(x => new AuditorSetter(manager, x)))
                    .ToArray();
            _lastModifiedDateSetters = properties.Where(AttributeUtils.HasAttribute<LastModifiedDateAttribute>)
                .Select(x => new AuditDateSetter(x))
                .Union(fields.Where(AttributeUtils.HasAttribute<LastModifiedDateAttribute>)
                    .Select(x => new AuditDateSetter(x)))
                .ToArray();

            HasModifyAudit = _lastModifiedBySetters.Any() ||
                             _lastModifiedDateSetters.Any();

            HasAudit = HasModifyAudit ||
                       _createdBySetters.Any() ||
                       _createdDateSetters.Any();
        }

        public bool HasModifyAudit { get; private set; }

        public void AuditCreation(object target)
        {
            foreach(var setter in _createdBySetters)
                setter.Set(target);
            foreach (var setter in _createdDateSetters)
                setter.Set(target);
        }

        public void AuditModified(object target)
        {
            foreach (var setter in _lastModifiedBySetters)
                setter.Set(target);
            foreach (var setter in _lastModifiedDateSetters)
                setter.Set(target);
        }

        public bool HasAudit { get; private set; }

        public class AuditorSetter
        {
            private readonly IComponentManager _manager;
            private readonly MemberInfo _member;
            private Func<object> _auditorFunc;
            private Action<object, object> _setterFunc;

            public AuditorSetter(IComponentManager manager, PropertyInfo property)
            {
                _manager = manager;
                _member = property;
                if (!property.CanWrite)
                    throw new InvalidComponentException(_member.DeclaringType,
                        string.Format("Audit property {0} needs to have a setter", property));
                _auditorFunc = GetAuditorFunc(property.PropertyType);
                _setterFunc = property.SetValue;
            }

            public AuditorSetter(IComponentManager manager, FieldInfo field)
            {
                _manager = manager;
                _member = field;
                _auditorFunc = GetAuditorFunc(field.FieldType);
                _setterFunc = field.SetValue;
            }

            private Func<object> GetAuditorFunc(Type type)
            {
                var component = _manager.GetComponent(type, new CurrentAuditorAttribute());
                return () => _manager.GetReference(component, _manager.CreateCreationalContext(component));
            }

            public void Set(object target)
            {
                _setterFunc(target, _auditorFunc());
            }
        }

        public class AuditDateSetter
        {
            private readonly MemberInfo _member;
            private readonly Func<object> _nowFunc;
            private Action<object, object> _setterFunc;

            public AuditDateSetter(PropertyInfo property)
            {
                _member = property;
                if(!property.CanWrite)
                    throw new InvalidComponentException(_member.DeclaringType, 
                        string.Format("Audit property {0} needs to have a setter", property));
                _nowFunc = GetNowFunc(property.PropertyType);
                _setterFunc = property.SetValue;
            }

            public AuditDateSetter(FieldInfo field)
            {
                _member = field;
                _nowFunc = GetNowFunc(field.FieldType);
                _setterFunc = field.SetValue;
            }

            private Func<object> GetNowFunc(Type type)
            {
                if (type == typeof (DateTime))
                    return () => DateTime.UtcNow;
                if (type == typeof (DateTimeOffset))
                    return () => DateTimeOffset.UtcNow;

                throw new InvalidComponentException(_member.DeclaringType, 
                    string.Format("{0} needs to be of DateTime or DateTimeOffset type", _member));
            }

            public void Set(object target)
            {
                _setterFunc(target, _nowFunc());
            }
        }
    }
}