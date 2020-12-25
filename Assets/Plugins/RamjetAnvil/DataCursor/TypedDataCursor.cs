using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Reflection.Emit;

namespace RamjetAnvil {

    public interface ITypedDataCursor<T> {
        Func<T> Get { get; }
        Action<T> Set { get; }
        ITypedDataCursor<TInner> To<TInner>(Expression<Func<T, TInner>> selectorExpr);
        IObservable<T> OnUpdate { get; }
    }
    
    public class TypedDataCursor<T> : ITypedDataCursor<T> {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;

        private readonly Action<IList<MemberInfo>, T> _internalSetter;
        private readonly IList<MemberInfo> _path;
        private readonly IObservable<UpdatedPath> _updateTracker;

        private readonly IDictionary<MemberInfo, object> _cachedCursors;

        private TypedDataCursor(Func<T> getter, Action<IList<MemberInfo>, T> setter, 
            IObservable<UpdatedPath> onUpdate, IList<MemberInfo> path) {

            _getter = getter;
            _path = path;
            _internalSetter = setter;
            _setter = value => setter(path, value);
            _updateTracker = onUpdate;
            _cachedCursors = new Dictionary<MemberInfo, object>();
        }

        public Func<T> Get {
            get { return _getter; }
        }

        public Action<T> Set {
            get { return _setter; }
        }

        public ITypedDataCursor<TInner> To<TInner>(Expression<Func<T, TInner>> selectorExpr) {
            Debug.Assert(selectorExpr.Body is MemberExpression);
            var memberExpr = selectorExpr.Body as MemberExpression;
            var member = memberExpr.Member;
            Debug.Assert((member.MemberType & (MemberTypes.Field | MemberTypes.Property)) != 0, 
                "Member must be field or property but was: " + member.MemberType);
            if (member is PropertyInfo) {
                var propertyInfo = member as PropertyInfo;
                Debug.Assert(propertyInfo.CanRead);    
                Debug.Assert(propertyInfo.CanWrite);
            }

            object cursor;
            var propertyMember = GetPropertyMember(selectorExpr);
            if (!_cachedCursors.TryGetValue(propertyMember, out cursor)) {
                var setInner = PropertySetter(selectorExpr);
                cursor = new TypedDataCursor<TInner>(
                    getter: PropertyGetter(selectorExpr),
                    setter: (path, value) => _internalSetter(path, setInner(value)),
                    onUpdate: _updateTracker,
                    path: new List<MemberInfo>(_path) {propertyMember});
                _cachedCursors[propertyMember] = cursor;
            }
            return (ITypedDataCursor<TInner>) cursor;
        }

        private bool IsCursorInPath(IList<MemberInfo> fullPath) {
            bool isInPath = true;
            for (int i = 0; i < fullPath.Count && i < _path.Count; i++) {
                if (_path[i] != fullPath[i]) {
                    isInPath = false;
                    break;
                }
            }
            return isInPath;
        }

        private Func<TProperty> PropertyGetter<TProperty>(
            Expression<Func<T, TProperty>> selectorExpr) {

            var selector = selectorExpr.Compile();
            return () => selector(_getter());
        }

        private Func<TProperty, T> PropertySetter<TProperty>(
            Expression<Func<T, TProperty>> selectorExpr) {

            var memberExpr = selectorExpr.Body as MemberExpression;
            var member = memberExpr.Member;

            switch (member.MemberType) {
                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo)member;
                    return newValue => {
                        // TODO Unfortunately boxing is necessary here
                        var parent = (object)_getter();
                        fieldInfo.SetValue(parent, newValue);
                        return (T)parent;
                    };
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)member;
                    return newValue => {
                        var parent = _getter();
                        propertyInfo.SetValue(parent, newValue, index: null);
                        return parent;
                    };
                default:
                    throw new ArgumentException("Member must be field or property but was: " + member.MemberType);
            }
        }

//        /// <summary>
//        ///    Derives the property-setter function from the supplied property-getter function.
//        /// </summary>
//        private static Action<TObject, TProperty> Setter<TObject, TProperty>(
//            Expression<Func<TObject, TProperty>> selectorExpr) {
//
//            Debug.Assert(selectorExpr.Body is MemberExpression);
//            var memberExpr = selectorExpr.Body as MemberExpression;
//            var member = memberExpr.Member;
//            Debug.Assert((member.MemberType & (MemberTypes.Field | MemberTypes.Property)) != 0, 
//                "Member must be field or property but was: " + member.MemberType);
//
//            var propertyMember = GetPropertyMember(selectorExpr);
//
//            var paramTargetExp = Expression.Parameter(typeof(TObject), propertyMember.Name);
//            var paramValueExp = Expression.Parameter(typeof(TProperty), propertyMember.Name);
//
//            // TODO Get the property, update it, set it
//            var propExpr = Expression.PropertyOrField(paramTargetExp, propertyMember.Name);
//
//            var callExpr = Expression.Call(paramTargetExp, propertyMember, paramValueExp);
//            var assignExp = Expression.Lambda(callExpr, paramTargetExp, paramValueExp);
//
//            var setter = Expression.Lambda<Action<TObject, TProperty>>(assignExp, paramTargetExp, paramValueExp);
//
//            return setter.Compile();
//        }

//        //https://stackoverflow.com/questions/321650/how-do-i-set-a-field-value-in-an-c-sharp-expression-tree
//        private static Action<TObject, TValue> MakeSetter<TObject, TValue>(FieldInfo field) {
//            DynamicMethod m = new DynamicMethod(
//                "setter", typeof(void), new [] { typeof(TObject), typeof(TValue) }, typeof(T));
//
//            ILGenerator cg = m.GetILGenerator();
//            // arg0.<field> = arg1
//            cg.Emit(OpCodes.Ldarg_0);
//            cg.Emit(OpCodes.Ldarg_1);
//            cg.Emit(OpCodes.Stfld, field);
//            cg.Emit(OpCodes.Ret);
//
//            return (Action<TObject, TValue>) m.CreateDelegate(typeof(Action<TObject, TValue>));
//        }

        /// <summary>
        ///   Get the name of the property mentioned in the property-getter function as a
        ///   string.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        private static MemberInfo GetPropertyMember<TObject, TProperty>(Expression<Func<TObject, TProperty>> exp) {
            return ((MemberExpression) exp.Body).Member;
        }

        public IObservable<T> OnUpdate {
            get {
                return _updateTracker
                    .Where(updatedPath => IsCursorInPath(updatedPath.Path))
                    .Select(updatePayload => _getter());
            }
        }

        public static ITypedDataCursor<T> Root(T initialValue) {
            var currentValue = initialValue;
            var initialPath = new List<MemberInfo>();
            var subject = new BehaviorSubject<UpdatedPath>(new UpdatedPath(initialPath));
            return new TypedDataCursor<T>(
                getter: () => currentValue,
                setter: (path, value) => {
                    currentValue = value;
                    subject.OnNext(new UpdatedPath(path));
                },
                onUpdate: subject,
                path: initialPath);
        }
    }

    public struct UpdatedPath {
        public IList<MemberInfo> Path;

        public UpdatedPath(IList<MemberInfo> path) {
            Path = path;
        }
    }

}
