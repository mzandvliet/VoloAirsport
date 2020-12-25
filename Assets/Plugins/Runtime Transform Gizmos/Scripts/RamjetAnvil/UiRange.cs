using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.InputModule;
using UnityEngine;

namespace RTEditor
{
    public class UiRange : AbstractUiRange {

        private InputRange _inputRange;

        public override InputRange InputRange
        {
            get { return _inputRange; }
            set { _inputRange = value; }
        }
    }
}
