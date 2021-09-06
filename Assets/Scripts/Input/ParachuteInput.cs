using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using InControl;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Ui;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {

    public enum ParachuteAction {
        //ParachuteConfigToggle,

        WeightShiftLeft,
        WeightShiftRight,
        WeightShiftFront,
        WeightShiftBack,

        // Hold-line mode
        PullLeftLines, PullRightLines,
        PullBothLines,
        HoldFrontLines, HoldRearLines,

        // Direct-line mode
        PullFrontLineLeft, PullFrontLineRight, 
        PullRearLineLeft, PullRearLineRight, 
        PullBrakeLineLeft, PullBrakeLineRight, 
    }


    [Serializable]
    public struct ParachuteInputConfig {
        public float BrakeSmoothingSpeed;
        public float RearRisersSmoothingSpeed;
        public float FrontRisersSmoothingSpeed;
        public float WeightShiftSmoothingSpeed;
    }
}
