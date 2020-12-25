using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    public class CameraViewVolume
    {
        #region Private Variables
        /// <summary>
        /// Holds the camera view volume points in world space. This array can be accessed
        /// using members of the 'CameraViewVolumePoint' as indices.
        /// </summary>
        private Vector3[] _worldSpaceVolumePoints;

        /// <summary>
        /// This array holds the view volume's planes in world space. The elements in this array 
        /// can be accessed using members of the 'CameraViewVolumePlane' enum as indices. The 
        /// planes are stored in the following order: left, right, bottom, top, near, far. All
        /// planes are pointing inside the volume.
        /// </summary>
        private Plane[] _worldSpacePlanes;

        /// <summary>
        /// Holds a collection of rays which unite the view volume points. For example, one of these
        /// rays could be the ray which starts from the top left point on the camera near plane and 
        /// aims towards the camera top left far clip plane point.
        /// </summary>
        private Ray3D[] _worldSpaceVolumeEdgeRays;

        private Vector2 _nearPlaneSize;
        private Vector2 _farPlaneSize;

        private float _farClipPlaneDistance;
        private float _nearClipPlaneDistance;

        private Bounds _aabb;
        #endregion

        #region Public Properties
        public Ray3D[] WorldSpaceVolumeEdgeRays { get { return _worldSpaceVolumeEdgeRays.Clone() as Ray3D[]; } }
        public Vector3 TopLeftPointOnNearPlane { get { return _worldSpaceVolumePoints[(int)CameraViewVolumePoint.TopLeftOnNearPlane]; } }
        public Vector3 TopRightPointOnNearPlane { get { return _worldSpaceVolumePoints[(int)CameraViewVolumePoint.TopRightOnNearPlane]; } }
        public Vector3 BottomRightPointOnNearPlane { get { return _worldSpaceVolumePoints[(int)CameraViewVolumePoint.BottomRightOnNearPlane]; } }
        public Vector3 BottomLeftPointOnNearPlane { get { return _worldSpaceVolumePoints[(int)CameraViewVolumePoint.BottomLeftOnNearPlane]; } }
        public Vector3 TopLeftPointOnFarPlane { get { return _worldSpaceVolumePoints[(int)CameraViewVolumePoint.TopLeftOnFarPlane]; } }
        public Vector3 TopRightPointOnFarPlane { get { return _worldSpaceVolumePoints[(int)CameraViewVolumePoint.TopRightOnFarPlane]; } }
        public Vector3 BottomRightPointOnFarPlane { get { return _worldSpaceVolumePoints[(int)CameraViewVolumePoint.BottomRightOnFarPlane]; } }
        public Vector3 BottomLeftPointOnFarPlane { get { return _worldSpaceVolumePoints[(int)CameraViewVolumePoint.BottomLeftOnFarPlane]; } }
        public float FarClipPlaneDistance { get { return _farClipPlaneDistance; } }
        public float NearClipPlaneDistance { get { return _nearClipPlaneDistance; } }
        public Vector2 NearPlaneSize { get { return _nearPlaneSize; } }
        public Vector2 FarPlaneSize { get { return _farPlaneSize; } }
        public Bounds AABB { get { return _aabb; } }
        #endregion

        #region Constructors
        public CameraViewVolume()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="camera">
        /// The camera whose view volume data is necessary to construct the 'CameraViewVolume' instance.
        /// </param>
        /// <param name="desiredCameraFarClipPlane">
        /// This can be used to in cases when it's useful to have a view volume that has a smaller or bigger
        /// far clip plane distance. This value will override the actual camera far clip plane distance and the
        /// view volume will be constructed using this value instead. Note that the far clip plane distance for
        /// 'camera' will not be modified.
        /// </param>
        public CameraViewVolume(Camera camera, float desiredCameraFarClipPlane)
        {
            BuildForCamera(camera, desiredCameraFarClipPlane);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Builds the camera view volume for the specified camera. Please see the comments for the
        /// class constructor in order to understand what the second parameter is about.
        /// </summary>
        public void BuildForCamera(Camera camera, float desiredCameraFarClipPlane)
        {
            // Store the old camera far clip plane distance. We need to do this because we will
            // temporarily modify the camera far clip plane to 'desiredCameraFarClipPlane' in order
            // to perform all the necessary calculations.
            float oldCameraFarClipPlane = camera.farClipPlane;
            AdjustCameraFarClipPlane(camera, desiredCameraFarClipPlane);

            // Calculate the view volume data
            CalculateWorldSpacePoints(camera);
            CalculateWorldSpacePlanes(camera);
            CalculateWorldSpaceVolumeEdgeRays();

            // Restore the camera far clip plane to what it was before
            camera.farClipPlane = oldCameraFarClipPlane;

            // Store clip plane distances
            _farClipPlaneDistance = desiredCameraFarClipPlane;
            _nearClipPlaneDistance = camera.nearClipPlane;

            // Calculate near and far plane dimensions
            Transform cameraTransform = camera.transform;
            Vector3 cornerVector = TopLeftPointOnNearPlane - BottomRightPointOnNearPlane;
            float width = Mathf.Abs(Vector3.Dot(cornerVector, cameraTransform.right));
            float height = Mathf.Abs(Vector3.Dot(cornerVector, cameraTransform.up));
            _nearPlaneSize = new Vector2(width, height);

            cornerVector = TopLeftPointOnFarPlane - BottomRightPointOnFarPlane;
            width = Mathf.Abs(Vector3.Dot(cornerVector, cameraTransform.right));
            height = Mathf.Abs(Vector3.Dot(cornerVector, cameraTransform.up));
            _farPlaneSize = new Vector2(width, height);

            // Calculate the volume AABB
            _aabb = BoundsExtensions.FromPointCloud(new List<Vector3>(_worldSpaceVolumePoints));
        }

        public bool ContainsWorldSpaceAABB(Bounds worldSpaceAABB)
        {
            return GeometryUtility.TestPlanesAABB(_worldSpacePlanes, worldSpaceAABB);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Calculates the view volume world space points using the specified camera.
        /// </summary>
        private void CalculateWorldSpacePoints(Camera camera)
        {
            CameraViewVolumePointsCalculator volumePointsCalculator = CameraViewVolumePointsCalculatorFactory.Create(camera);
            _worldSpaceVolumePoints = volumePointsCalculator.CalculateWorldSpaceVolumePoints(camera);
        }

        /// <summary>
        /// Calculates the view volume world space planes using the specified camera.
        /// </summary>
        private void CalculateWorldSpacePlanes(Camera camera)
        {
            _worldSpacePlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        }

        /// <summary>
        /// Calculates the view volume world space edge rays.
        /// </summary>
        private void CalculateWorldSpaceVolumeEdgeRays()
        {
            _worldSpaceVolumeEdgeRays = CameraViewVolumeEdgeRaysCalculator.CalculateWorldSpaceVolumeEdgeRays(this);
        }

        /// <summary>
        /// Adjusts the far clip plane distance of 'camera' to 'desiredFarClipPlane'.
        /// </summary>
        /// <remarks>
        /// The method will ensure that the far clip plane always sits in front of the
        /// near plane.
        /// </remarks>
        private void AdjustCameraFarClipPlane(Camera camera, float desiredFarClipPlane)
        {
            // Set the far clip plane distance and make sure it sits in front of the near plane.
            camera.farClipPlane = desiredFarClipPlane;
            EnsureFarClipPlaneSitsInFrontOfNearPlane(camera);
        }

        /// <summary>
        /// Ensures the the far clip plane of 'camera' always sits in front of the
        /// camera's near plane.
        /// </summary>
        private void EnsureFarClipPlaneSitsInFrontOfNearPlane(Camera camera)
        {
            if (camera.farClipPlane <= camera.nearClipPlane) camera.farClipPlane = camera.nearClipPlane + 0.1f;
        }
        #endregion
    }
}
