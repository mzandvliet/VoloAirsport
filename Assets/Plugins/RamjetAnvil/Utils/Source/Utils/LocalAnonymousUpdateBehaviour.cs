using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LocalAnonymousUpdateBehaviour : MonoBehaviour {
    private Action _updateAction;

    void Update() {
        _updateAction();
    }

    public Action UpdateAction {
        set { _updateAction = value; }
    }
}
