using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ComponentLifecycleEvents : MonoBehaviour {

    public event Action OnAwakeEvent;
    public event Action OnStartEvent;
    public event Action OnEnableEvent;
    public event Action OnDisableEvent;
    public event Action OnDestroyEvent;

    void Awake() {
        if (OnAwakeEvent != null) {
            OnAwakeEvent();
        }
        OnAwakeEvent = null;
    }

    void Start() {
        if (OnStartEvent != null) {
            OnStartEvent();
        }
        OnStartEvent = null;
    }

    void OnEnable() {
        if (OnEnableEvent != null) {
            OnEnableEvent();
        }
    }

    void OnDisable() {
        if (OnDisableEvent != null) {
            OnDisableEvent();
        }
    }

    void OnDestroy() {
        if (OnDestroyEvent != null) {
            OnDestroyEvent();
        }
        OnEnableEvent = null;
        OnDisableEvent = null;
        OnDestroyEvent = null;
    }

}
