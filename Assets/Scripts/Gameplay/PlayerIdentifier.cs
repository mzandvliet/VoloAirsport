using UnityEngine;
using System.Collections;

namespace RamjetAnvil.Volo {
    public class PlayerIdentifier : MonoBehaviour {
        [SerializeField]
        private Wingsuit _root;

        public Wingsuit Root {
            get { return _root; }
        }
    }
}
