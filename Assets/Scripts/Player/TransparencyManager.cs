using UnityEngine;
using System.Collections;

public class TransparencyManager : MonoBehaviour {
    private Renderer[] _renderers;

    void Start() {
        _renderers = gameObject.GetComponentsInChildren<Renderer>();
    }

    public void SetVisible(bool visible) {
        for (int i = 0; i < _renderers.Length; i++) {
            _renderers[i].enabled = visible;
        }
    }
}
