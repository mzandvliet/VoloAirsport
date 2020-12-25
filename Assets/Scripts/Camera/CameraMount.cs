using RamjetAnvil.Unity.Utility;
using UnityEngine;

public interface ICameraMount : IComponent {
    void OnMount(ICameraRig rig);
    void OnDismount(ICameraRig rig);
}

// Stupid dummy class for dumb transforms that don't do anything special but want to be a camera mount
public class CameraMount : MonoBehaviour, ICameraMount {
    public void OnMount(ICameraRig rig) {
        
    }

    public void OnDismount(ICameraRig rig) {
        
    }
}