using System;

namespace RamjetAnvil.DependencyInjection {

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class Dependency : Attribute {
        private readonly string _name;

        public Dependency() {
            _name = null;
        }

        public Dependency(string name) {
            _name = name;
        }

        public string Name {
            get { return _name; }
        }

        public override string ToString() {
            return _name ?? "";
        }
    }
}
