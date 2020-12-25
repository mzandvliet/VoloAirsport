using UnityEngine;
using System.Collections;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Volo.Input;

public static class ViveActionMap {

    public static PilotActionMap CreatePilotActionMap(ViveController viveController) {
        InputMap<WingsuitAction, float> allAxisInput = new InputMap<WingsuitAction, float>();
        allAxisInput = allAxisInput.Update(WingsuitAction.Pitch, () => viveController.Pitch);
        allAxisInput = allAxisInput.Update(WingsuitAction.Roll, () => viveController.Roll);
        InputMap<WingsuitAction, float> allMouseInput = new InputMap<WingsuitAction, float>();
        InputMap<WingsuitAction, ButtonState> allButtonInput = new InputMap<WingsuitAction, ButtonState>();

        allAxisInput = allAxisInput.FillEmptyValues(PilotInput.PilotActionDetails.AxisActions,
            () => 0.0f);
        allMouseInput = allMouseInput.FillEmptyValues(PilotInput.PilotActionDetails.AxisActions,
            () => 0.0f);
        allButtonInput = allButtonInput.FillEmptyValues(PilotInput.PilotActionDetails.ButtonActions,
            () => ButtonState.Released);

        return new PilotActionMap(allButtonInput, allAxisInput, allMouseInput);
    }

//    public static MenuActionMap CreateMenuActionMap() {
//        
//    }
}
