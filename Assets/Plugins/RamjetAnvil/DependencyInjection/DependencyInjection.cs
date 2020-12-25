using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RamjetAnvil.DependencyInjection {

    public delegate bool Injection<in TDep>(object subject, InjectionPoint injectionPoint, TDep dependencies, bool overrideExisting);

    public class DependencyInjector {

        public static readonly DependencyInjector Default = new DependencyInjector(new List<IInjector> {
            new GameObjectInjector(traverseHierarchy: true),
            MonoBehaviourInjector.Default,
            ObjectInjector.Default
        });
        public static readonly DependencyInjector NoHierarchyTraversal = new DependencyInjector(new List<IInjector> {
            new GameObjectInjector(traverseHierarchy: false),
            MonoBehaviourInjector.Default,
            ObjectInjector.Default
        });

        private readonly IList<IInjector> _injectors;

        public DependencyInjector(IList<IInjector> injectors) {
            _injectors = injectors;
        }

        public void InjectSingle(object subject, object dependency, bool overrideExisting = false) {
            Inject(subject, SingleInjection, dependency, overrideExisting);
        }

        public void Inject(object subject, DependencyContainer diContainer, bool overrideExisting = false) {
            Inject(subject, ContainerInjection, diContainer, overrideExisting);
        }

        public static readonly Func<Type, IList<InjectionPoint>> GetInjectionPoints =
            Memoization.Memoize<Type, IList<InjectionPoint>>(GetInjectionPointsInternal);

        private void Inject<TDep>(object subject, 
            Injection<TDep> injection, 
            TDep dependencies,
            bool overrideExisting) {

            if (subject == null) {
                throw new ArgumentNullException("Cannot inject on a null object");
            }

            bool isInjectionFinished = false;
            for (int i = 0; i < _injectors.Count && !isInjectionFinished; i++) {
                var injector = _injectors[i];
                if (injector.IsTypeSupported(subject.GetType())) {
                    injector.Inject(subject, injection, dependencies, overrideExisting);
                    isInjectionFinished = true;
                }
            }
        }

        private static readonly Injection<object> SingleInjection =
            (subject, injectionPoint, dependency, overrideExisting) => {
                if (injectionPoint.Injector.Type.IsInstanceOfType(dependency)) {
                    return InjectInternal(injectionPoint, subject, new DependencyReference(name: null, instance: dependency), overrideExisting);
                }
                return false;
            };

        private static readonly Injection<DependencyContainer> ContainerInjection =
            (subject, injectionPoint, diContainer, overrideExisting) => {
                bool isSomethingInjected = false;
                IList<DependencyReference> candidates;
                if (diContainer.DepsByType.TryGetValue(injectionPoint.Injector.Type, out candidates)) {
                    for (int i = 0; i < candidates.Count; i++) {
                        var dependency = candidates[i];
                        var isInjectionSucceeded = InjectInternal(injectionPoint, subject, dependency, overrideExisting);
                        if (isInjectionSucceeded) {
                            isSomethingInjected = true;
                        }
                    }
                }
                return isSomethingInjected;
            };

        private static IList<InjectionPoint> GetInjectionPointsInternal(Type type) {
            var injectionPoints = new List<InjectionPoint>();
            var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.NonPublic |
                                                BindingFlags.Instance | BindingFlags.SetProperty |
                                                BindingFlags.DeclaredOnly);
            for (int i = 0; i < properties.Length; i++) {
                var propertyInfo = properties[i];
                var dependencyAttributes = propertyInfo.GetCustomAttributes(typeof (Dependency), inherit: true);
                if (dependencyAttributes.Length > 0) {
                    var dependencyInfo = (Dependency) dependencyAttributes.First();
                    var propertyMember = new PropertyMember(propertyInfo);
                    if (propertyMember.HasSetter && propertyMember.HasGetter) {
                        injectionPoints.Add(new InjectionPoint(dependencyInfo, propertyMember));
                    } else {
                        throw new Exception("Dependency properties must have a getter and a setter: " + propertyMember + " on " + type);
                    }
                }
            }
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                        BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            for (int i = 0; i < fields.Length; i++) {
                var fieldInfo = fields[i];
                var dependencyAttributes = fieldInfo.GetCustomAttributes(typeof (Dependency), inherit: true);
                if (dependencyAttributes.Length > 0) {
                    var dependencyInfo = (Dependency) dependencyAttributes.First();
                    injectionPoints.Add(new InjectionPoint(dependencyInfo, new FieldMember(fieldInfo)));
                }
            }

            return injectionPoints;
        }

        private static bool InjectInternal(InjectionPoint injectionPoint, object subject, DependencyReference dependency,
            bool overrideExisting) {

            var isNameMatch = injectionPoint.Info.Name == null ||
                              injectionPoint.Info.Name.Equals(dependency.Name);
            var isTypeMatch = injectionPoint.Injector.Type.IsInstanceOfType(dependency.Instance)
                && injectionPoint.Injector.Info.DeclaringType.IsInstanceOfType(subject);
            var currentValue = injectionPoint.Injector.GetValue(subject);
            bool isDependencyAlreadySet;
            if (currentValue is UnityEngine.Object) {
                isDependencyAlreadySet = (currentValue as UnityEngine.Object) != null;
            } else {
                isDependencyAlreadySet = currentValue != null;
            }

            if (isNameMatch && isTypeMatch && (!isDependencyAlreadySet || overrideExisting)) {
                injectionPoint.Injector.SetValue(subject, dependency.Instance);
                return true;
            }
            return false;
        }
    }
}
