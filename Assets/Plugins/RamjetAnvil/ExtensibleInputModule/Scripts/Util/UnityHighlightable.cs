using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.InputModule {
    public abstract class UnityHighlightable : MonoBehaviour, IHighlightable {
        public abstract event Action OnHighlight;
        public abstract event Action OnUnHighlight;
    }
}
