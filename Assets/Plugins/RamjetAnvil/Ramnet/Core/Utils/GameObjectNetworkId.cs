using System.Collections.Generic;
using UnityEngine;
using Guid = RamjetAnvil.Unity.Utility.Guid;

[ExecuteInEditMode]
public class GameObjectNetworkId : MonoBehaviour {
        
    [SerializeField] private Guid _id;

    public Guid Id {
        get { return _id; }
    }

    void Start() {
        if (Id == null || Id.Equals(Guid.Empty)) {
            Debug.LogError("Failed to initialize network id for object " + gameObject.name);
        }
    }

#if UNITY_EDITOR
    public void GenerateGuid() {
        // Bug: manual changes to byte field are picked up and saved by Unity, but the below changes are not.
        // Can't call editor stuff apparently, because it all fails silently.
        _id = Guid.RandomGuid();
        Debug.Log("Creating new id for '" + name + "', " + GetInstanceID() + ", " + _id);
    }
#endif
}
