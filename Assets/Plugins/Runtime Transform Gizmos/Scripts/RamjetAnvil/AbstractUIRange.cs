using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.InputModule;
using UnityEngine;

namespace RTEditor
{
    public abstract class AbstractUiRange : MonoBehaviour {
        public abstract InputRange InputRange { get; set; }

        public void SetRange(Vector3 v) {
            InputRange.SetValue(v);
        }
    }
}
