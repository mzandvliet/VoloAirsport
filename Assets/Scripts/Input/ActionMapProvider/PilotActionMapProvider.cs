using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using UnityEngine;

public abstract class PilotActionMapProvider : MonoBehaviour {
    public abstract IReadonlyRef<PilotActionMap> ActionMapRef { get; }
    public abstract PilotActionMap ActionMap { get; }
    public abstract void SetInputMappingSource(IObservable<ActionMapConfig<WingsuitAction>> inputSourceMappingChanges);
}
