using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using UnityEngine;

public abstract class MenuActionMapProvider : MonoBehaviour {

    public abstract IReadonlyRef<MenuActionMap> ActionMap { get; }
    public abstract void SetInputMappingSource(IObservable<ActionMapConfig<MenuAction>> actionMapConfigChanges);
}
