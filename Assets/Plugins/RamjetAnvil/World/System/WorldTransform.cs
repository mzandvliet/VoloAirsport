using UnityEngine;

public class WorldTransform : MonoBehaviour {
    private WorldSystem _system;

    public DVector3 Position
    {
        get { return _system.LocalPointToWorld(transform.position); }
        set { transform.position = _system.WorldPointToLocal(value); }
    }

    private void Awake() {
        _system = WorldSystem.Instance;
        _system.Add(this);
    }

    private void OnDestroy() {
        _system.Remove(this);
    }
}
