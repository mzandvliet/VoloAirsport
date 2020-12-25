using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Instances of this class can be used to hold a snaphot of the object
    /// selection mask.
    /// </summary>
    public class ObjectSelectionMaskSnapshot
    {
        #region Private Variables
        /// <summary>
        /// Holds all the objects which are assigned to the object selection mask at the
        /// moment the snapshot is taken.
        /// </summary>
        private HashSet<GameObject> _maskedObjects;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the objects which belong to the object selection mask at the moment
        /// the snapshot was taken.
        /// </summary>
        public HashSet<GameObject> MaskedObjects { get { return _maskedObjects; } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Takes a snapshot of the current object selection mask.
        /// </summary>
        public void TakeSnapshot()
        {
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;
            _maskedObjects = objectSelection.MaskedObjects;
        }
        #endregion
    }
}
