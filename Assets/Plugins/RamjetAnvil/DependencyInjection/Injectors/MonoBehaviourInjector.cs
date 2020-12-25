using System;
using UnityEngine;

namespace RamjetAnvil.DependencyInjection {

    public class MonoBehaviourInjector : IInjector {

        public static readonly MonoBehaviourInjector Default = new MonoBehaviourInjector();

        private MonoBehaviourInjector() {}

        public void Inject<TContext>(object subject, Injection<TContext> injection, TContext dependencies, bool overrideExisting) {
            var injectionPoints = DependencyInjector.GetInjectionPoints(subject.GetType());
            var allDependenciesResolved = true;
            for (int i = 0; i < injectionPoints.Count; i++) {
                var injectionPoint = injectionPoints[i];
                // Search dependency for each injection point and inject it
                injection(subject, injectionPoint, dependencies, overrideExisting);
                allDependenciesResolved = allDependenciesResolved && IsDependencySet(injectionPoint, subject);
            }

            (subject as MonoBehaviour).enabled = allDependenciesResolved;
        }

        public bool IsTypeSupported(Type t) {
            return t.IsInstanceOfType(typeof(MonoBehaviour));
        }

        private static bool IsDependencySet(InjectionPoint injectionPoint, object subject) {
            object storedValue = injectionPoint.Injector.GetValue(subject);
            if (storedValue is UnityEngine.Object) {
                return (storedValue as UnityEngine.Object) != null;
            }
            return storedValue != null;
        }
    }
}
