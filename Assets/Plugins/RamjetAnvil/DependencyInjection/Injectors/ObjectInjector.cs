using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.DependencyInjection {

    public class ObjectInjector : IInjector {

        public static readonly ObjectInjector Default = new ObjectInjector();

        private ObjectInjector() {}

        public void Inject<TContext>(object subject, Injection<TContext> injection, TContext dependencies, bool overrideExisting) {
            var injectionPoints = DependencyInjector.GetInjectionPoints(subject.GetType());
            for (int i = 0; i < injectionPoints.Count; i++) {
                injection(subject, injectionPoints[i], dependencies, overrideExisting);
            }
        }

        public bool IsTypeSupported(Type t) {
            return t.IsAssignableFrom(typeof(object));
        }
    }
}
