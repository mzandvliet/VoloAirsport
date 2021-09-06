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

    public enum WingsuitAction {
        Respawn, ToStartSelection, UnfoldParachute,

        // Player Movement
        Cannonball, CloseArms, CloseLeftArm, CloseRightArm,
        PitchUp, PitchDown, RollLeft, RollRight, YawLeft, YawRight,

        // Camera Movement
        LookUp, LookDown, LookLeft, LookRight, ChangeCamera,

        // Combined movement
        Pitch, Roll, Yaw, LookVertical, LookHorizontal,

        ActivateSlowMo,
        ActivateMouseLook, ToggleSpectatorView
    }
}
