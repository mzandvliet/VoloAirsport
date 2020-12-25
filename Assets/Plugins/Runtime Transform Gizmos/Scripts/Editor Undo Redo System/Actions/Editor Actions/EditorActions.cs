using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    public class NoOpAction : IUndoableAndRedoableAction, IAction
    {
        #region Public Methods
        public void Execute()
        {
            EditorUndoRedoSystem.Instance.RegisterAction(this);
        }

        public void Undo()
        {
        }

        public void Redo()
        {
        }
        #endregion
    }

    public class ObjectDuplicationAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        private List<GameObject> _sourceParents = new List<GameObject>();
        private List<GameObject> _duplicateObjectParents = new List<GameObject>();
        private ObjectSelectionSnapshot _preDuplicateSelectionSnapshot;
        #endregion

        #region Constructors
        public ObjectDuplicationAction(List<GameObject> sourceObjects)
        {
            if(sourceObjects != null && sourceObjects.Count != 0)
            {
                _sourceParents = GameObjectExtensions.GetParentsFromObjectCollection(sourceObjects);
            }
        }
        #endregion

        #region Public Methods
        public void Execute()
        {
            if(_sourceParents.Count != 0)
            {
                foreach(var parent in _sourceParents)
                {
                    Transform parentTransform = parent.transform;
                    GameObject clone = parent.Clone(parentTransform.position, parentTransform.rotation, parentTransform.lossyScale, parentTransform.parent);
                    _duplicateObjectParents.Add(clone);
                }

                EditorUndoRedoSystem.Instance.RegisterAction(this);
            }
        }

        public void Undo()
        {
            if(_duplicateObjectParents.Count != 0)
            {
                foreach(var parent in _duplicateObjectParents)
                {
                    MonoBehaviour.Destroy(parent);
                }
                _duplicateObjectParents.Clear();
            }
        }

        public void Redo()
        {
            if (_sourceParents.Count != 0)
            {
                _duplicateObjectParents.Clear();
                foreach (var parent in _sourceParents)
                {
                    Transform parentTransform = parent.transform;
                    GameObject clone = parent.Clone(parentTransform.position, parentTransform.rotation, parentTransform.lossyScale, parentTransform.parent);
                    _duplicateObjectParents.Add(clone);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// This is the action that is executed when the transform space is changed.
    /// </summary>
    public class TransformSpaceChangeAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// This is the transform space which is active before the action is executed.
        /// </summary>
        private TransformSpace _oldTransformSpace;

        /// <summary>
        /// This is the new transform space.
        /// </summary>
        private TransformSpace _newTransformSpace;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="oldTransformSpace">
        /// This is the transform space which is active before the action is executed.
        /// </param>
        /// <param name="newTransformSpace">
        /// The new transform space.
        /// </param>
        public TransformSpaceChangeAction(TransformSpace oldTransformSpace, TransformSpace newTransformSpace)
        {
            _oldTransformSpace = oldTransformSpace;
            _newTransformSpace = newTransformSpace;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            // Execute the action only if the transform spaces differ
            if (_oldTransformSpace != _newTransformSpace)
            {
                EditorGizmoSystem.Instance.TransformSpace = _newTransformSpace;
                TransformSpaceChangedMessage.SendToInterestedListeners(_oldTransformSpace, _newTransformSpace);
                EditorUndoRedoSystem.Instance.RegisterAction(this);
            }
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            gizmoSystem.TransformSpace = _oldTransformSpace;
            TransformSpaceChangedMessage.SendToInterestedListeners(_newTransformSpace, _oldTransformSpace);
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            gizmoSystem.TransformSpace = _newTransformSpace;
            TransformSpaceChangedMessage.SendToInterestedListeners(_oldTransformSpace, _newTransformSpace);
        }
        #endregion
    }

    /// <summary>
    /// This is the action that is executed when the gizmos are turned off.
    /// </summary>
    public class GizmosTurnOffAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// This is the type of gizmo that is active before the gizmos are turned off.
        /// </summary>
        private GizmoType _activeGizmoType;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activeGizmoType">
        /// This is the type of gizmo that is active before the gizmos are turned off.
        /// </param>
        public GizmosTurnOffAction(GizmoType activeGizmoType)
        {
            _activeGizmoType = activeGizmoType;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            // Execute the action if the gizmos are not already turned off
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            if (!gizmoSystem.AreGizmosTurnedOff)
            {
                gizmoSystem.TurnOffGizmos();
                GizmosTurnedOffMessage.SendToInterestedListeners(_activeGizmoType);
                EditorUndoRedoSystem.Instance.RegisterAction(this);
            }
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;

            GizmoType oldActiveGizmoType = gizmoSystem.ActiveGizmoType;
            gizmoSystem.ActiveGizmoType = _activeGizmoType;
            ActiveGizmoTypeChangedMessage.SendToInterestedListeners(oldActiveGizmoType, _activeGizmoType);
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            gizmoSystem.TurnOffGizmos();
            GizmosTurnedOffMessage.SendToInterestedListeners(_activeGizmoType);
        }
        #endregion
    }

    /// <summary>
    /// This is the action that is executed when the transform pivot point changes.
    /// </summary>
    public class TransformPivotPointChangeAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// This is the pivot point that is active before the action is executed.
        /// </summary>
        private TransformPivotPoint _oldPivotPoint;

        /// <summary>
        /// This is the new pivot point.
        /// </summary>
        private TransformPivotPoint _newPivotPoint;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="oldPivotPoint">
        /// This is the pivot point that is active before the action is executed.
        /// </param>
        /// <param name="newPivotPoint">
        /// This is the new pivot point.
        /// </param>
        public TransformPivotPointChangeAction(TransformPivotPoint oldPivotPoint, TransformPivotPoint newPivotPoint)
        {
            _oldPivotPoint = oldPivotPoint;
            _newPivotPoint = newPivotPoint;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            // If the pivot points differ, execute the action
            if(_oldPivotPoint != _newPivotPoint)
            {
                EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
                gizmoSystem.TransformPivotPoint = _newPivotPoint;
                TransformPivotPointChangedMessage.SendToInterestedListeners(_oldPivotPoint, _newPivotPoint);
                EditorUndoRedoSystem.Instance.RegisterAction(this);
            }
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            gizmoSystem.TransformPivotPoint = _oldPivotPoint;
            TransformPivotPointChangedMessage.SendToInterestedListeners(_newPivotPoint, _oldPivotPoint);
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            gizmoSystem.TransformPivotPoint = _newPivotPoint;
            TransformPivotPointChangedMessage.SendToInterestedListeners(_oldPivotPoint, _newPivotPoint);
        }
        #endregion
    }

    /// <summary>
    /// This is the action that is executed when the active gizmo type is changed.
    /// </summary>
    public class ActiveGizmoTypeChangeAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// This is the active gizmo type that is active before the action is executed.
        /// </summary>
        private GizmoType _oldActiveGizmoType;

        /// <summary>
        /// This is the new active gizmo type.
        /// </summary>
        private GizmoType _newActiveGizmoType;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="oldActiveGizmoType">
        /// This is the active gizmo type that is active before the action is executed.
        /// </param>
        /// <param name="newActiveGizmoType">
        /// This is the new active gizmo type.
        /// </param>
        public ActiveGizmoTypeChangeAction(GizmoType oldActiveGizmoType, GizmoType newActiveGizmoType)
        {
            _oldActiveGizmoType = oldActiveGizmoType;
            _newActiveGizmoType = newActiveGizmoType;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            // If the gizmo types differ, execute the action
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            if (_oldActiveGizmoType != _newActiveGizmoType || gizmoSystem.AreGizmosTurnedOff)
            {
                gizmoSystem.ActiveGizmoType = _newActiveGizmoType;
                ActiveGizmoTypeChangedMessage.SendToInterestedListeners(_oldActiveGizmoType, _newActiveGizmoType);
                EditorUndoRedoSystem.Instance.RegisterAction(this);
            }
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            gizmoSystem.ActiveGizmoType = _oldActiveGizmoType;
            ActiveGizmoTypeChangedMessage.SendToInterestedListeners(_newActiveGizmoType, _oldActiveGizmoType);
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            gizmoSystem.ActiveGizmoType = _newActiveGizmoType;
            ActiveGizmoTypeChangedMessage.SendToInterestedListeners(_oldActiveGizmoType, _newActiveGizmoType);
        }
        #endregion
    }

    /// <summary>
    /// This action is executed after the object selection has changed.
    /// </summary>
    public class PostObjectSelectionChangedAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// The object selection snapshot which was taken before the object selection was changed.
        /// </summary>
        private ObjectSelectionSnapshot _preChangeSelectionSnapshot;

        /// <summary>
        /// The object selection snapshot which was taken after the object selection was changed.
        /// </summary>
        private ObjectSelectionSnapshot _postChangeSelectionSnapshot;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="preChangeSelectionSnapshot">
        /// The object selection snapshot taken before the selection was changed.
        /// </param>
        /// <param name="postChangeSelectionSnapshot">
        /// The object selection snapshot taken after the selection was changed.
        /// </param>
        public PostObjectSelectionChangedAction(ObjectSelectionSnapshot preChangeSelectionSnapshot, 
                                                ObjectSelectionSnapshot postChangeSelectionSnapshot)
        {
            _preChangeSelectionSnapshot = preChangeSelectionSnapshot;
            _postChangeSelectionSnapshot = postChangeSelectionSnapshot;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            ObjectSelectionChangedMessage.SendToInterestedListeners();
            EditorUndoRedoSystem.Instance.RegisterAction(this);
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            EditorObjectSelection.Instance.ApplySnapshot(_preChangeSelectionSnapshot);
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            EditorObjectSelection.Instance.ApplySnapshot(_postChangeSelectionSnapshot);
        }
        #endregion
    }

    /// <summary>
    /// This action can be executed to change the object selection mode.
    /// </summary>
    public class ObjectSelectionModeChangeAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// This is the object selection mode before it was changed.
        /// </summary>
        private ObjectSelectionMode _oldObjectSelectionMode;

        /// <summary>
        /// This is the object selection mode after it was changed.
        /// </summary>
        private ObjectSelectionMode _newObjectSelectionMode;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="newObjectSelectionMode">
        /// This is the new object selection mode which must be activated when the action is executed.
        /// </param>
        public ObjectSelectionModeChangeAction(ObjectSelectionMode newObjectSelectionMode)
        {
            _oldObjectSelectionMode = newObjectSelectionMode;
            _newObjectSelectionMode = newObjectSelectionMode;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            // Store data for easy access
            EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;
            ObjectSelectionSettings objectSelectionSettings = editorObjectSelection.ObjectSelectionSettings;

            // Only change the selection mode if it differs from the curent one
            _oldObjectSelectionMode = objectSelectionSettings.ObjectSelectionMode;
            if (_newObjectSelectionMode != _oldObjectSelectionMode)
            {
                // Change the selection mode
                objectSelectionSettings.ObjectSelectionMode = _newObjectSelectionMode;

                // Send a message to all interested listeners
                ObjectSelectionModeChangedMessage.SendToInterestedListeners(_newObjectSelectionMode);

                // Register the action with the Undo/Redo system
                EditorUndoRedoSystem.Instance.RegisterAction(this);
            }
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            if (_newObjectSelectionMode != _oldObjectSelectionMode)
            {
                // Store data for easy access
                EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;
                ObjectSelectionSettings objectSelectionSettings = editorObjectSelection.ObjectSelectionSettings;

                // Restore the old selection mode
                objectSelectionSettings.ObjectSelectionMode = _oldObjectSelectionMode;
              
                // Send a message to all interested listeners
                ObjectSelectionModeChangedMessage.SendToInterestedListeners(_oldObjectSelectionMode);
            }
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            if (_newObjectSelectionMode != _oldObjectSelectionMode)
            {
                // Store data for easy access
                EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;
                ObjectSelectionSettings objectSelectionSettings = editorObjectSelection.ObjectSelectionSettings;

                // Reactivate the new selection mode
                objectSelectionSettings.ObjectSelectionMode = _newObjectSelectionMode;

                // Send a message to all interested listeners
                ObjectSelectionModeChangedMessage.SendToInterestedListeners(_newObjectSelectionMode);
            }
        }
        #endregion
    }

    /// <summary>
    /// This action can be executed to assign a collection of objects to the object selection mask.
    /// </summary>
    public class AssignObjectsToSelectionMaskAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// The object selection mask snapshot which was taken before the mask was updated.
        /// </summary>
        private ObjectSelectionMaskSnapshot _preChangeMaskSnapshot;

        /// <summary>
        /// The object selection mask snapshot which was taken after the mask was updated.
        /// </summary>
        private ObjectSelectionMaskSnapshot _postChangeMaskSnapshot;

        /// <summary>
        /// This is the collection of game objects which must be assigned to the object selection mask.
        /// </summary>
        private List<GameObject> _gameObjects;

        /// <summary>
        /// When the objects are assigned to the selection mask, it is possible for the selection
        /// to change. This boolean will be used to identify this situation.
        /// </summary>
        private bool _selectionWasChanged;

        /// <summary>
        /// The object selection snapshot which was taken before the object selection was changed.
        /// </summary>
        private ObjectSelectionSnapshot _preChangeSelectionSnapshot;

        /// <summary>
        /// The object selection snapshot which was taken after the object selection was changed.
        /// </summary>
        private ObjectSelectionSnapshot _postChangeSelectionSnapshot;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameObjects">
        /// These are the game objects which must be assigned to the object selection mask.
        /// </param>
        public AssignObjectsToSelectionMaskAction(List<GameObject> gameObjects)
        {
            _gameObjects = new List<GameObject>(gameObjects);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            if(_gameObjects.Count != 0)
            {
                // Take a snapshot to allow for undo
                _preChangeMaskSnapshot = new ObjectSelectionMaskSnapshot();
                _preChangeMaskSnapshot.TakeSnapshot();

                // Assign the objects to the selection mask.
                // Note: We will also perform a snapshot of the current object selection just in case
                //       the object selection changes after the objects are assigned to the mask.
                _preChangeSelectionSnapshot = new ObjectSelectionSnapshot();
                _preChangeSelectionSnapshot.TakeSnapshot();
                EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;
                _selectionWasChanged = editorObjectSelection.AddGameObjectCollectionToSelectionMask(_gameObjects);

                // If the selection was changed, we will perform a snapshot post change to make sure that we
                // can undo and redo the action accordingly. For example, if assigning the objects to the mask
                // caused a selection change, when the operation is undone, we want to restore both the mask
                // and the state that the selection was in before the mask was updated.
                if(_selectionWasChanged)
                {
                    _postChangeSelectionSnapshot = new ObjectSelectionSnapshot();
                    _postChangeSelectionSnapshot.TakeSnapshot();
                }

                // Take a snapshot to allow for redo
                _postChangeMaskSnapshot = new ObjectSelectionMaskSnapshot();
                _postChangeMaskSnapshot.TakeSnapshot();

                ObjectsAddedToSelectionMaskMessage.SendToInterestedListeners(_gameObjects);
                EditorUndoRedoSystem.Instance.RegisterAction(this);
            }
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            if (_preChangeMaskSnapshot != null)
            {
                EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;

                editorObjectSelection.ApplyMaskSnapshot(_preChangeMaskSnapshot);
                if (_selectionWasChanged) editorObjectSelection.ApplySnapshot(_preChangeSelectionSnapshot);
            }
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            if (_postChangeMaskSnapshot != null)
            {
                EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;

                editorObjectSelection.ApplyMaskSnapshot(_postChangeMaskSnapshot);
                if (_selectionWasChanged) editorObjectSelection.ApplySnapshot(_postChangeSelectionSnapshot);
            }
        }
        #endregion
    }

    /// <summary>
    /// This action can be executed to remove a collection of objects from the selection mask.
    /// </summary>
    public class RemoveObjectsFromSelectionMaskAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// This is the collection of game objects which must be removed from the object selection mask.
        /// </summary>
        private List<GameObject> _gameObjects;

        /// <summary>
        /// The object selection mask snapshot which was taken before the mask was updated.
        /// </summary>
        private ObjectSelectionMaskSnapshot _preChangeMaskSnapshot;

        /// <summary>
        /// The object selection mask snapshot which was taken after the mask was updated.
        /// </summary>
        private ObjectSelectionMaskSnapshot _postChangeMaskSnapshot;
        #endregion

        #region Contructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameObjects">
        /// These are the game objects which must be removed from the selection mask.
        /// </param>
        public RemoveObjectsFromSelectionMaskAction(List<GameObject> gameObjects)
        {
            _gameObjects = new List<GameObject>(gameObjects);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            if (_gameObjects.Count != 0)
            {
                // Take a snapshot to allow for undo
                _preChangeMaskSnapshot = new ObjectSelectionMaskSnapshot();
                _preChangeMaskSnapshot.TakeSnapshot();

                // Remove the objects from the selection mask.
                EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;
                editorObjectSelection.RemoveGameObjectCollectionFromSelectionMask(_gameObjects);

                // Take a snapshot to allow for redo
                _postChangeMaskSnapshot = new ObjectSelectionMaskSnapshot();
                _postChangeMaskSnapshot.TakeSnapshot();

                ObjectsRemovedFromSelectionMaskMessage.SendToInterestedListeners(_gameObjects);
                EditorUndoRedoSystem.Instance.RegisterAction(this);
            }
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            if (_preChangeMaskSnapshot != null)
            {
                EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;
                editorObjectSelection.ApplyMaskSnapshot(_preChangeMaskSnapshot);
            }
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            if (_postChangeMaskSnapshot != null)
            {
                EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;
                editorObjectSelection.ApplyMaskSnapshot(_postChangeMaskSnapshot);
            }
        }
        #endregion
    }

    /// <summary>
    /// This action is executed after a gizmo has finished transforming game objects.
    /// </summary>
    public class PostGizmoTransformedObjectsAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// This is the list of object trasnform snapshots which were taken before the objects were transformed.
        /// </summary>
        private List<ObjectTransformSnapshot> _preTransformObjectSnapshots;

        /// <summary>
        /// This is the list of object transform snapshots which were taken after the objects were transformed.
        /// </summary>
        private List<ObjectTransformSnapshot> _postTransformObjectSnapshot;

        /// <summary>
        /// This is the gizmo which transformed the game objects.
        /// </summary>
        private Gizmo _gizmoWhichTransformedObjects;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="preTransformObjectSnapshots">
        /// This is the list of object trasnform snapshots which were taken before the objects were transformed.
        /// </param>
        /// <param name="postTransformObjectSnapshot">
        /// This is the list of object transform snapshots which were taken after the objects were transformed.
        /// </param>
        /// <param name="gizmoWhichTransformedObjects">
        /// This is the gizmo which transformed the game objects.
        /// </param>
        public PostGizmoTransformedObjectsAction(List<ObjectTransformSnapshot> preTransformObjectSnapshots,
                                                 List<ObjectTransformSnapshot> postTransformObjectSnapshot,
                                                 Gizmo gizmoWhichTransformedObjects)
        {
            _preTransformObjectSnapshots = new List<ObjectTransformSnapshot>(preTransformObjectSnapshots);
            _postTransformObjectSnapshot = new List<ObjectTransformSnapshot>(postTransformObjectSnapshot);
            _gizmoWhichTransformedObjects = gizmoWhichTransformedObjects;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            GizmoTransformedObjectsMessage.SendToInterestedListeners(_gizmoWhichTransformedObjects);
            EditorUndoRedoSystem.Instance.RegisterAction(this);
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            // In order to undo this kind of action, we will loop through all pre transform object snapshots
            // and apply them to the corresponding objects.
            foreach(ObjectTransformSnapshot snapshot in _preTransformObjectSnapshots)
            {
                snapshot.ApplySnapshot();
            }

            // Send a gizmo transform operation undone message
            GizmoTransformOperationWasUndoneMessage.SendToInterestedListeners(_gizmoWhichTransformedObjects);
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            // In order to redo this kind of action, we will loop through all post transform object snapshots
            // and apply them to the corresponding objects.
            foreach (ObjectTransformSnapshot snapshot in _postTransformObjectSnapshot)
            {
                snapshot.ApplySnapshot();
            }

            // Send a gizmo transform operation redone message
            GizmoTransformOperationWasRedoneMessage.SendToInterestedListeners(_gizmoWhichTransformedObjects);
        }
        #endregion
    }
}
