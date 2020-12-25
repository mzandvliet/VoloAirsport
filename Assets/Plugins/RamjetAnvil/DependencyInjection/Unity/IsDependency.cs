using UnityEngine;

namespace RamjetAnvil.DependencyInjection.Unity {
    public class IsDependency : MonoBehaviour {
        [SerializeField] private string _name = "";
        [SerializeField] private Component _reference;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public Component Reference {
            get { return _reference; }
            set { _reference = value; }
        }
    }
}