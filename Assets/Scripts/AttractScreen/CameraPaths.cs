using System.Collections.Generic;
using RamjetAnvil.Volo;
using UnityEngine;

public class CameraPaths : MonoBehaviour {

    [SerializeField] private CameraPath[] _paths;

    public IList<ICameraPath> Paths {
        get { return _paths; }
    }
}