using UnityEngine;

namespace RTEditor
{
    public abstract class CameraViewVolumePointsCalculator
    {
        #region Public Abstract Methods
        public abstract Vector3[] CalculateWorldSpaceVolumePoints(Camera camera);
        #endregion
    }
}