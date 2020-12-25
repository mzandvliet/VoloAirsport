using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This class can be used to store a snapshot of an object's transform data.
    /// </summary>
    public class ObjectTransformSnapshot
    {
        #region Private Variables
        /// <summary>
        /// This is the game object to which the snapshot data applies.
        /// </summary>
        private GameObject _gameObject;

        /// <summary>
        /// This is the snapshot of the object's absolute position.
        /// </summary>
        private Vector3 _absolutePosition;

        /// <summary>
        /// This is the snapshot of the object's absolute rotation.
        /// </summary>
        private Quaternion _absoluteRotation;

        /// <summary>
        /// This is the snapshot of the object's absolute scale.
        /// </summary>
        private Vector3 _absoluteScale;
        #endregion

        #region Public Methods
        /// <summary>
        /// Takes a snapshot of the specified object's transform data.
        /// </summary>
        public void TakeSnapshot(GameObject gameObject)
        {
            // Store the game object reference
            _gameObject = gameObject;

            // Take the snapshot
            Transform objectTransform = gameObject.transform;
            _absolutePosition = objectTransform.position;
            _absoluteRotation = objectTransform.rotation;
            _absoluteScale = objectTransform.lossyScale;
        }

        /// <summary>
        /// Applies the snapshot data to the last game object whose snapshot was taken. This
        /// method has no effect if no snapshot was taken before calling this method.
        /// </summary>
        public void ApplySnapshot()
        {
            // Is there any snapshot data available?
            if(_gameObject != null)
            {
                // Apply the snapshot to the game object's transform
                Transform objectTransform = _gameObject.transform;
                objectTransform.position = _absolutePosition;
                objectTransform.rotation = _absoluteRotation;
                _gameObject.SetAbsoluteScale(_absoluteScale);
            }
        }
        #endregion
    }
}
