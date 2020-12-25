using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RamjetAnvil.DependencyInjection {

    public class DependencyContainer {

        private readonly IDictionary<string, DependencyReference> _depsByString;
        private readonly IDictionary<Type, IList<DependencyReference>> _depsByType;

        public DependencyContainer() : this(Enumerable.Empty<KeyValuePair<string, object>>()) { }

        public DependencyContainer(IEnumerable<KeyValuePair<string, object>> dependencies) {
            _depsByString = new Dictionary<string, DependencyReference>();
            _depsByType = new Dictionary<Type, IList<DependencyReference>>();

            foreach (var d in dependencies) {
                AddDependency(new DependencyReference(d.Key, d.Value));
            }
        }

        public IDictionary<string, DependencyReference> DepsByString {
            get { return _depsByString; }
        }

        public IDictionary<Type, IList<DependencyReference>> DepsByType {
            get { return _depsByType; }
        }

        public void AddDependency(string name, object instance) {
            AddDependency(new DependencyReference(name, instance));
        }

        public void AddDependency(DependencyReference dependency) {
            if (dependency.Instance == null) {
                throw new ArgumentNullException("Dependency '" + dependency.Name + "' cannot be null");
            }

            _depsByString.Add(dependency.Name, dependency);

            // Fill the container with each possible type of the 
            // dependency, i.g. the instance type and all of its parents.
            foreach (var type in dependency.Instance.GetType().GetAllTypes())
            {
                IList<DependencyReference> deps;
                if (!_depsByType.TryGetValue(type, out deps))
                {
                    deps = new List<DependencyReference>();
                    _depsByType.Add(type, deps);
                }
                deps.Add(dependency);
            }
        }

        public DependencyContainer Copy() {
            var newContainer = new DependencyContainer();
            foreach (var dependencyReference in DepsByString.Values) {
                newContainer.AddDependency(dependencyReference);
            }
            return newContainer;
        }

    }
}
