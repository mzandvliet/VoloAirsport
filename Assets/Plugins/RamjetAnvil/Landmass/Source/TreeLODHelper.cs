using UnityEngine;

/* 
 * Temp class to help out with LOD configuration pre-5.1
 * Redundant when you can just use LODGroup.GetLOD/SetLOD
 */
public class TreeLODHelper : MonoBehaviour {
    [SerializeField] private LODGroup _lodGroup;
    [SerializeField] private Renderer[] _lodRenderers;

    public LODGroup LodGroup
    {
        get { return _lodGroup; }
    }
}
