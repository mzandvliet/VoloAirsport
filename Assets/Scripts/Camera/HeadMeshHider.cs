using UnityEngine;

public class HeadMeshHider : MonoBehaviour, ICameraMount {
    [SerializeField] private MeshRenderer _mesh;

    public void OnMount(ICameraRig rig) {
        _mesh.enabled = false;
    }

    public void OnDismount(ICameraRig rig) {
        _mesh.enabled = true;
    }
}
