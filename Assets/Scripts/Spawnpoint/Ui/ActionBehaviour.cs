using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ActionBehaviour : MonoBehaviour
{
    private Action _action;

    public Action Action
    {
        set { _action = value; }
    }

    void Update()
    {
        _action();
    }
}
