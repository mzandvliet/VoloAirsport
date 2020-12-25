using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This is a static class that implements some useful 'Camera' extension methods.
    /// </summary>
    public static class CameraExtensions
    {
        #region Public Static Functions
        /// <summary>
        /// Returns the view volume for the specified camera.
        /// </summary>
        public static CameraViewVolume GetViewVolume(this Camera camera)
        {
            // Create the view volume
            var viewVolume = new CameraViewVolume();
            viewVolume.BuildForCamera(camera, camera.farClipPlane);

            // Return the view volume to the caller
            return viewVolume;
        }

        public static CameraViewVolume GetViewVolume(this Camera camera, float desiredFarClipPlane)
        {
            // Create the view volume
            var viewVolume = new CameraViewVolume();
            viewVolume.BuildForCamera(camera, desiredFarClipPlane);

            // Return the view volume to the caller
            return viewVolume;
        }

        /// <summary>
        /// Returns a list of game objects which are visible to the specified camera.
        /// </summary>
        /// <remarks>
        /// This function detects only objects which have a collider attached to them.
        /// </remarks>
        public static List<GameObject> GetVisibleGameObjects(this Camera camera)
        {
            // We need the camera view volume to detect the visible game objects
            var cameraViewVolume = new CameraViewVolume();
            cameraViewVolume.BuildForCamera(camera, camera.farClipPlane);

            // In order to detect the visible game objects, we will loop through all POTTENTIALLY visible
            // game objects and check if their AABB lies inside the camera frustum.
            List<GameObject> pottentiallyVisibleGameObjects = camera.GetPottentiallyVisibleGameObjects();
            var visibleGameObjects = new List<GameObject>(pottentiallyVisibleGameObjects.Count);        // Set initial capacity to avoid resize
            foreach (GameObject gameObject in pottentiallyVisibleGameObjects)
            {
                // If the game object's world space AABB intersects the camera frustum, it means it is visible
                Box worldSpaceAABB = gameObject.GetWorldBox();
                if (worldSpaceAABB.IsInvalid()) continue;
                if (cameraViewVolume.ContainsWorldSpaceAABB(worldSpaceAABB.ToBounds())) visibleGameObjects.Add(gameObject);
            }

            // Return the visible objects list to the caller
            return visibleGameObjects;
        }

        public static bool IsGameObjectVisible(this Camera camera, GameObject gameObject)
        {
            var cameraViewVolume = new CameraViewVolume();
            cameraViewVolume.BuildForCamera(camera, camera.farClipPlane);

            Box worldSpaceAABB = gameObject.GetWorldBox();
            if (worldSpaceAABB.IsInvalid()) return false;
            return cameraViewVolume.ContainsWorldSpaceAABB(worldSpaceAABB.ToBounds());
        }
        #endregion

        #region Private Static Functions
        /// <summary>
        /// Returns a list of game objects which COULD be visible to the specified camera. Essentially,
        /// the function gathers the game objects which sit around the camera and which might be visible
        /// when rendered.
        /// </summary>
        /// <remarks>
        /// This function detects only objects which have a collider attached to them.
        /// </remarks>
        private static List<GameObject> GetPottentiallyVisibleGameObjects(this Camera camera)
        {
            CameraViewVolume cameraViewVolume = camera.GetViewVolume();
            Box viewVolumeBox = Box.FromBounds(cameraViewVolume.AABB);

            return EditorScene.Instance.OverlapBox(viewVolumeBox);
        }
        #endregion
    }
}
