using System;
using System.Reflection;

namespace RamjetAnvil.DependencyInjection {

    public struct InjectionPoint
    {
        private readonly Dependency _info;
        private readonly IMember _injector;

        public InjectionPoint(Dependency info, IMember injector)
        {
            _info = info;
            _injector = injector;
        }

        public Dependency Info
        {
            get { return _info; }
        }

        public IMember Injector
        {
            get { return _injector; }
        }

        public override string ToString() {
            return _injector.ToString();
        }
    }

    
    public interface IMember {
        object GetValue(object @this);

        void SetValue(object @this, object value);

        Type Type { get; }
        MemberInfo Info { get; }
    }

    public struct FieldMember : IMember {

        private readonly FieldInfo _field;

        public FieldMember(FieldInfo field) : this() {
            _field = field;
        }

        public object GetValue(object @this) {
            return _field.GetValue(@this);
        }

        public void SetValue(object @this, object value) {
            _field.SetValue(@this, value);
        }

        public Type Type {
            get {
                return _field.FieldType;
            }
        }

        public MemberInfo Info {
            get {
                return _field;
            }
        }

        public override string ToString() {
            return String.Format("Field({0} {1})", Type, _field.Name);
        }
    }

    public struct PropertyMember : IMember {

        private readonly PropertyInfo _property;

        public PropertyMember(PropertyInfo property) {
            _property = property;
        }

        public object GetValue(object @this) {
            return _property.GetGetMethod().Invoke(@this, new object[0]);
        }

        public void SetValue(object @this, object value) {
            _property.GetSetMethod().Invoke(@this, new[] { value });
        }

        public Type Type {
            get {
                return _property.PropertyType;
            }
        }

        public MemberInfo Info {
            get {
                return _property;
            }
        }

        public bool HasGetter {
            get { return _property.GetGetMethod() != null; }
        }

        public bool HasSetter {
            get { return _property.GetSetMethod() != null; }
        }

        public override string ToString() {
            return string.Format("Property({0} {1})", Type, _property.Name);
        }
    }
}
