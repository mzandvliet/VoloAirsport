using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This is the object selection mechanism which handles scene object selection.
    /// </summary>
    [Serializable]
    public class EditorObjectSelection : MonoSingletonBase<EditorObjectSelection>, IMessageListener
    {
        #region Private Variables
        /// <summary>
        /// Holds all object related settings.
        /// </summary>
        [SerializeField]
        private ObjectSelectionSettings _objectSelectionSettings = new ObjectSelectionSettings();
        
        /// <summary>
        /// Holds all currently selected objects.
        /// </summary>
        private HashSet<GameObject> _selectedObjects = new HashSet<GameObject>();

        /// <summary>
        /// Holds all objects which have been added to the selection mask. Objects that
        /// are added to the selection mask, can not be selected until they are removed
        /// from the mask.
        /// </summary>
        private HashSet<GameObject> _maskedObjects = new HashSet<GameObject>();
 
        /// <summary>
        /// This is the last game object which was selected. 
        /// </summary>
        /// <remarks>
        /// When objects are removed from the selection, a random object will be retrieve
        /// from the selected objects list.
        /// </remarks>
        private GameObject _lastSelectedGameObject;

        /// <summary>
        /// When this is true, when an object is clicked, it will be appended to the selection if
        /// not already selected. If already selected, it will be removed from the selection.
        /// When objects are selected using the selection shape, if this is set to true, objects
        /// will be appended to the selection and the current selection will not be cleared.
        /// </summary>
        private bool _appendOrDeselectOnClick = false;

        /// <summary>
        /// When this is true, all objects which reside inside the selection shape, will be
        /// deselected. When false, those objects will be selected.
        /// </summary>
        private bool _multiDeselect = false;

        /// <summary>
        /// These 2 will be used to take snapshots before and after the selection changes
        /// by clicking on a game object.
        /// </summary>
        private ObjectSelectionSnapshot _singleSelectPreChangeSnapshot;
        private ObjectSelectionSnapshot _singleSelectPostChangeSnapshot;

        /// <summary>
        /// These 3 will be used to take snapshots before and after the selection is changed
        /// by objects entering or exiting the selection shape.
        /// </summary>
        private ObjectSelectionSnapshot _multiSelectPreChangeSnapshot;
        private ObjectSelectionSnapshot _multiSelectPostChangeSnapshot;
        private bool _wasSelectionChangedWithSelectionShape;

        /// <summary>
        /// When the selection mode is set to 'Custom', these 3 will have to be set by the
        /// client code in order for the selection mechanism to be fully functional.
        /// </summary>
        private ObjectSelectionGameObjectClickedHandler _customObjectClickedHandler;
        private ObjectSelectionGameObjectsEnteredSelectionShapeHandler _customObjectsEnteredSelectionShapeHandler;
        private ObjectSelectionBoxCalculator _customObjectSelectionBoxCalculator;

        /// <summary>
        /// Specifies whether or not the selection mechanism is enabled or disabled.
        /// </summary>
        private bool _isEnabled = true;

        /// <summary>
        /// This is the object selection rectangle which can be used to select/deselect
        /// multiple objects at once.
        /// </summary>
        [SerializeField]
        private ObjectSelectionRectangle _objectSelectionRectangle = new ObjectSelectionRectangle();

        /// <summary>
        /// Holds useful mouse information between frame updates.
        /// </summary>
        private Mouse _mouse = new Mouse();
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the number of selected game objects.
        /// </summary>
        public int NumberOfSelectedObjects { get { return _selectedObjects.Count; } }

        /// <summary>
        /// Returns the last selected game object.
        /// </summary>
        public GameObject LastSelectedGameObject { get { return _lastSelectedGameObject; } }

        /// <summary>
        /// Get/set custom handlers which are used when the selection mode is set to 'Custom'.
        /// </summary>
        public ObjectSelectionGameObjectClickedHandler CustomObjectClickedHandler { get { return _customObjectClickedHandler; } set { _customObjectClickedHandler = value; } }
        public ObjectSelectionGameObjectsEnteredSelectionShapeHandler CustomObjectsEnteredSelectionShapeHandler { get { return _customObjectsEnteredSelectionShapeHandler; } set { _customObjectsEnteredSelectionShapeHandler = value; } }
        public ObjectSelectionBoxCalculator CustomObjectSelectionBoxCalculator { get { return _customObjectSelectionBoxCalculator; } set { _customObjectSelectionBoxCalculator = value; } }

        /// <summary>
        /// Returns a copy of the internal selected objects collection.
        /// </summary>
        public HashSet<GameObject> SelectedGameObjects { get { return new HashSet<GameObject>(_selectedObjects); } }

        /// <summary>
        /// Returns a copy of the internal object selection mask.
        /// </summary>
        public HashSet<GameObject> MaskedObjects { get { return new HashSet<GameObject>(_maskedObjects); } }

        /// <summary>
        /// Gets/sets the boolean flag which specifies if objects must be appended to the selection
        /// or deselected for single object clicks when those objects are already selected.
        /// </summary>
        public bool AppendOrDeselectOnClick { get { return _appendOrDeselectOnClick; } set { _appendOrDeselectOnClick = value; } }

        /// <summary>
        /// Gets/sets the boolean flag which specifies if objects must be deselected when entering
        /// the area of the selection shape.
        /// </summary>
        public bool MultiDeselect { get { return _multiDeselect; } set { _multiDeselect = value; } }

        /// <summary>
        /// Returns the object selection settings.
        /// </summary>
        public ObjectSelectionSettings ObjectSelectionSettings { get { return _objectSelectionSettings; } }

        /// <summary>
        /// Returns the object selection rectangle render settings.
        /// </summary>
        public ObjectSelectionRectangleRenderSettings ObjectSelectionRectangleRenderSettings { get { return _objectSelectionRectangle.RenderSettings; } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Enables/disables the object selection mechanism. When disabled,
        /// all input will be ignored and no object selection or deselection
        /// can be performed.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;

            // If the object selection mechanism was disabled and if the 'DeselectObjectsWhenDisabled'
            // is set to true, we will have to deselect all objects which are currently selected.
            if (!_isEnabled && _objectSelectionSettings.DeselectObjectsWhenDisabled)
            {
                // If there are any objects selected, deselect them and send a selection changed message
                // to all interested listeners.
                if(NumberOfSelectedObjects != 0)
                {
                    _selectedObjects.Clear();
                    ObjectSelectionChangedMessage.SendToInterestedListeners();
                }
            }
        }

        /// <summary>
        /// Selects a single game object. Returns true if the selection
        /// was changed and false otherwise.
        /// </summary>
        /// <remarks>
        /// You should not call this method directly. If you need to customize the object
        /// selection mechansim, please refer to the documentation.
        /// This method only has effect if:
        ///     -the selection mechanism is allowed to operate;
        ///     -the game object is not already selected;
        ///     -the game object is not masked.
        /// </remarks>
        public bool SelectGameObject(GameObject gameObject)
        {
            // Make sure the game object can be selected
            if(CanOperate() && !IsGameObjectSelected(gameObject) && !IsMasked(gameObject))
            {
                // Select the game object and adjust the last selected game object reference
                _selectedObjects.Add(gameObject);
                _lastSelectedGameObject = gameObject;

                // Selection was changed
                return true;
            }

            // The selection wasn't changed
            return false;
        }

        /// <summary>
        /// Selects the game objects which reside in 'gameObjects'. Returns true if the selection
        /// was changed and false otherwise.
        /// </summary>
        /// <remarks>
        /// You should not call this method directly. If you need to customize the object
        /// selection mechansim, please refer to the documentation.
        /// This method only has effect if:
        ///     -the selection mechanism is allowed to operate;
        ///     -'gameObjects' is not empty.
        /// The method will select only those game objects which:
        ///     -are not already selected;
        ///     -are not masked.
        /// </remarks>
        public bool SelectGameObjectCollection(List<GameObject> gameObjects)
        {
            // Make sure we can continue
            if(CanOperate() && gameObjects.Count != 0)
            {
                // Loop through all game objects which must be selected
                bool wasSelectionChanged = false;
                foreach (GameObject objectToSelect in gameObjects)
                {
                    // Can the game object be selected?
                    if (!IsGameObjectSelected(objectToSelect) && !IsMasked(objectToSelect))
                    {
                        // It can, so add it to the selection list and update the last selected game object reference
                        _selectedObjects.Add(objectToSelect);
                        _lastSelectedGameObject = objectToSelect;

                        // The selection was changed. Set this to true so that we know what to return from the function later.
                        wasSelectionChanged = true;
                    }
                }

                // Return true or false based on whether or not the selection was changed
                return wasSelectionChanged;
            }

            // The selection wasn't changed
            return false;
        }

        /// <summary>
        /// Deselects the specified game object. Returns true if the selection
        /// was changed and false otherwise.
        /// </summary>
        /// <remarks>
        /// You should not call this method directly. If you need to customize the object
        /// selection mechansim, please refer to the documentation.
        /// This method only has effect if:
        ///     -the selection mechanism is allowed to operate;
        ///     -the specified game object is selected.
        /// </remarks>
        public bool DeselectGameObject(GameObject gameObject)
        {
            // Make sure the game object can be deselected
            if(CanOperate() && IsGameObjectSelected(gameObject))
            {
                // Deselect the game object and adjust the last selected game object reference by 
                // retrieving a random object from the set.
                _selectedObjects.Remove(gameObject);
                _lastSelectedGameObject = RetrieveAGameObjectFromObjectSelectionCollection();

                // The selection has changed
                return true;
            }

            // The selection hasn't changed
            return false;
        }

        /// <summary>
        /// Deselects the game objects which reside in 'gameObjects'. Returns true if the selection
        /// was changed and false otherwise.
        /// </summary>
        /// <remarks>
        /// You should not call this method directly. If you need to customize the object
        /// selection mechansim, please refer to the documentation.
        /// This method only has effect if:
        ///     -the selection mechanism is allowed to operate;
        ///     -'gameObjects' is not empty.
        /// The method will deselect only those game objects which are selected.
        /// </remarks>
        public bool DeselectGameObjectCollection(List<GameObject> gameObjects)
        {
            // Make sure the game objects can be deselected
            if (CanOperate() && gameObjects.Count != 0)
            {
                // Loop through all game objects which must be deselected
                bool wasSelectionChanged = false;
                foreach(GameObject objectToDeselect in gameObjects)
                {
                    // Is the game object selected?
                    if(IsGameObjectSelected(objectToDeselect))
                    {
                        // It is, so remove it
                        _selectedObjects.Remove(objectToDeselect);

                        // The selection was changed. Set this to true so that we know what to return from the function later.
                        wasSelectionChanged = true;
                    }
                }

                // Was the selection changed?
                if(wasSelectionChanged)
                {
                    // Adjust the last selected game object reference by retreiving a random object from the set
                    // and return true to inform the client code that the selection has changed.
                    _lastSelectedGameObject = RetrieveAGameObjectFromObjectSelectionCollection();
                    return true;
                }
            }

            // The selection hasn't changed
            return false;
        }

        /// <summary>
        /// Clears the object selection. The parameter allows you to specify whether or
        /// not the clear operation can be undone and redone. This may be useful when 
        /// customizing the object selection mechanism. When using this method inside
        /// a custom handler, you should set the parameter to false because the handler
        /// will be called by the object selection system and the system will take care
        /// of performing the necessary steps to allow for Undo/Redo.
        /// </summary>
        /// <returns>
        /// Returns true if the selection was changed and false otherwise.
        /// </returns>
        public bool ClearSelection(bool allowUndoRedo = true)
        {
            // Only clear the selection if necessary
            if(_selectedObjects.Count != 0)
            {
                if(allowUndoRedo)
                {
                    // Take a pre-change snapshot
                    var preChangeSnapshot = new ObjectSelectionSnapshot();
                    preChangeSnapshot.TakeSnapshot();

                    // Clear the selection
                    _selectedObjects.Clear();
                    _lastSelectedGameObject = null;

                    // Take a post-change snapshot
                    var postChangeSnapshot = new ObjectSelectionSnapshot();
                    postChangeSnapshot.TakeSnapshot();

                    // Execute the post-change action to allow for undo/redo
                    var action = new PostObjectSelectionChangedAction(preChangeSnapshot, postChangeSnapshot);
                    action.Execute();

                    // The selection has changed
                    return true;
                }
                else
                {
                    // Clear the selection
                    _selectedObjects.Clear();
                    _lastSelectedGameObject = null;

                    // The selection has changed
                    return true;
                }
            }

            // The selection hasn't changed
            return false;
        }

        /// <summary>
        /// Clears the selection and selects the specified game object. Returns true if the selection
        /// was changed and false otherwise.
        /// </summary>
        /// <remarks>
        /// You should not call this method directly. If you need to customize the object
        /// selection mechansim, please refer to the documentation.
        /// This method only has effect if:
        ///     -the selection mechanism is allowed to operate;
        ///     -clearing and selecting the object does not result in the same selection
        ///      as the one which was available before the method was called.
        /// The method will only select the specified game object if it is not masked.
        /// </remarks>
        public bool ClearAndSelectGameObject(GameObject gameObject)
        {
            // Make sure we are allowed to continue.
            // Note: Calling 'IsSelectionExactMatch' ensures that we don't execute the operation
            //       if it gets us right back where we started.
            if (CanOperate() && !IsSelectionExactMatch(new List<GameObject> { gameObject }))
            {
                // Is the object masked?
                if (!IsMasked(gameObject))
                {
                    // Clear the selection
                    _selectedObjects.Clear();

                    // Select the game object and adjust the last selected game object reference
                    _selectedObjects.Add(gameObject);
                    _lastSelectedGameObject = gameObject;

                    // The selection has changed
                    return true;
                }
                else
                {
                    // If the object is masked, it means that the selection will only change if there are
                    // objects currently selected. If there aren't, no change would occur because the
                    // objects is masked and it can not be added to the selection.
                    if(NumberOfSelectedObjects != 0)
                    {
                        // Clear the selection
                        _selectedObjects.Clear();

                        // The selection has changed
                        return true;
                    }
                }
            }

            // The selection hasn't changed
            return false;
        }

        /// <summary>
        /// Clears the selection and selects the game objects which reside in 'gameObjects'. Returns true 
        /// if the selection was changed and false otherwise.
        /// </summary>
        /// <remarks>
        /// You should not call this method directly. If you need to customize the object
        /// selection mechansim, please refer to the documentation.
        /// This method only has effect if:
        ///     -the selection mechanism is allowed to operate;
        ///     -'gameObjects' is not empty;
        ///     -clearing and selecting the objects does not result in the same selection
        ///      as the one which was available before the method was called.
        /// The method will select only those game objects which are not masked.
        /// </remarks>
        public bool ClearAndSelectGameObjectCollection(List<GameObject> gameObjects)
        {
            // Make sure we are allowed to continue.
            // Note: Calling 'IsSelectionExactMatch' ensures that we don't execute the operation
            //       if it gets us right back where we started.
            if (CanOperate() && gameObjects.Count != 0 && !IsSelectionExactMatch(gameObjects))
            {
                // Store the current number of selected objects (we will need this later) and then clear the selection
                int oldNumberOfSelectedObjects = NumberOfSelectedObjects;
                _selectedObjects.Clear();

                // Loop through all game objects
                bool wasSelectionChanged = false;
                foreach (GameObject objectToSelect in gameObjects)
                {
                    // Is the object masked?
                    if (!IsMasked(objectToSelect))
                    {
                        // The object is not masked. Select it and adjust the last selected game object reference.
                        _selectedObjects.Add(objectToSelect);
                        _lastSelectedGameObject = objectToSelect;

                        // The selection has changed.
                        wasSelectionChanged = true;
                    }
                }

                // If the selection has changed we can return true
                if (wasSelectionChanged) return true;
                else
                {
                    // If the selection hasn't changed it means all objects were masked and in this
                    // case we will set the last selected game object reference to null and return
                    // true only if the number of selected objects before the clear operation was
                    // different from 0.
                    _lastSelectedGameObject = null;
                    return oldNumberOfSelectedObjects != 0;
                }
            }

            // The selection hasn't changed
            return false;
        }

        /// <summary>
        /// Adds the specified game object collection to the selection mask. The method returns true if adding 
        /// the game objects to the mask causes the selection to change. For example, if the collection contains
        /// the objects A and B, and A is currently selected, the method will deselect it. This will cause a 
        /// selection change.
        /// </summary>
        public bool AddGameObjectCollectionToSelectionMask(List<GameObject> gameObjects)
        {
            // Loop through all objects
            bool selectionWasChanged = false;
            foreach (GameObject gameObject in gameObjects)
            {
                // If the game object is selected, we have to remove it from the current selection
                if (IsGameObjectSelected(gameObject))
                {
                    // Remove the object from the selection and set the boolean to true
                    _selectedObjects.Remove(gameObject);
                    selectionWasChanged = true;
                }

                // Add the game object to the selection mask
                _maskedObjects.Add(gameObject);
            }

            // If the selection was changed, inform all interested listeners
            if (selectionWasChanged) ObjectSelectionChangedMessage.SendToInterestedListeners();

            // Inform the client code about whether or not the selection has changed
            return selectionWasChanged;
        }

        /// <summary>
        /// Removes the specified game object collection from the selection mask.
        /// </summary>
        public void RemoveGameObjectCollectionFromSelectionMask(List<GameObject> gameObjects)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                _maskedObjects.Remove(gameObject);
            }
        }

        /// <summary>
        /// This method can be used to check if the current object selection matches
        /// the specified object collection exactly.
        /// </summary>
        public bool IsSelectionExactMatch(List<GameObject> gameObjectsToMatch)
        {
            // If the number of elements don't match, we can return false
            if (_selectedObjects.Count != gameObjectsToMatch.Count) return false;

            // Make sure that every game object in 'gameObjectsToMatch' is selected.
            // If we find at least one object which is not selected, we can return false.
            foreach(GameObject objectToMatch in gameObjectsToMatch)
            {
                if (!IsGameObjectSelected(objectToMatch)) return false;
            }

            // We have a match
            return true;
        }

        /// <summary>
        /// Can be used to check if the specified game object is masked.
        /// </summary>
        public bool IsMasked(GameObject gameObject)
        {
            bool objectHasMesh = gameObject.HasMesh();
            if (!_objectSelectionSettings.CanSelectTerrainObjects && gameObject.HasTerrain()) return true;
            if (!objectHasMesh && (!_objectSelectionSettings.CanSelectLightObjects && gameObject.HasLight()) ||
               (!_objectSelectionSettings.CanSelectParticleSystemObjects && gameObject.HasParticleSystem()) ||
               (!_objectSelectionSettings.CanSelectEmptyObjects && gameObject.IsEmpty()) ||
               (!_objectSelectionSettings.CanSelectSpriteObjects && gameObject.IsSprite())) return true;

            return _maskedObjects.Contains(gameObject) || !LayerHelper.IsLayerBitSet(_objectSelectionSettings.SelectableLayers, gameObject.layer);
        }

        /// <summary>
        /// Applies the specified object selection snapshot to the object selection.
        /// </summary>
        public void ApplySnapshot(ObjectSelectionSnapshot objectSelectionSnapshot)
        {
            // Apply the snapshot
            _selectedObjects = new HashSet<GameObject>(objectSelectionSnapshot.SelectedGameObjects);
            _lastSelectedGameObject = objectSelectionSnapshot.LastSelectedGameObject;

            // When the snapshot was applied the collection of selection objects was changed and
            // now the gizmos have to know about the new object collection.
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            //ConnectObjectSelectionToGizmo(gizmoSystem.TranslationGizmo);
            ConnectObjectSelectionToGizmo(gizmoSystem.RotationGizmo);
            //ConnectObjectSelectionToGizmo(gizmoSystem.ScaleGizmo);

            // Dispatch an object selection changed message
            ObjectSelectionChangedMessage.SendToInterestedListeners();
        }

        /// <summary>
        /// Applies the specified object selection mask snapshot. The method returns true if applying
        /// the mask snapshot causes the object selection to change. For example, if object A is selected,
        /// and the mask snapshot contains object A, the object A will have to be deselected. This will
        /// cause a selection change.
        /// </summary>
        public bool ApplyMaskSnapshot(ObjectSelectionMaskSnapshot objectSelectionMaskSnapshot)
        {
            // Apply the snapshot
            _maskedObjects = new HashSet<GameObject>(objectSelectionMaskSnapshot.MaskedObjects);

            // Now we have to make sure that if any of the masked objects are currently selected, we deselect them.
            bool selectionWasChanged = false;
            foreach (GameObject maskedObject in _maskedObjects)
            {
                // If selected, remove from selection
                if (IsGameObjectSelected(maskedObject))
                {
                    _selectedObjects.Remove(maskedObject);
                    selectionWasChanged = true;
                }
            }

            // If the selection has changed, send a message to all interested listeners
            if (selectionWasChanged) ObjectSelectionChangedMessage.SendToInterestedListeners();

            return selectionWasChanged;
        }

        /// <summary>
        /// This method is used internally by the object selection system to verify if it can perform
        /// any actions like selecting and deselecting objects etc. The method will always return false
        /// when the object selection system has been disabled via a call to 'SetEnabled'. Calling this
        /// method from other parts of the code may prove useful in certain circumstances. This is why
        /// it was made public.
        /// </summary>
        public bool CanOperate()
        {
            return _isEnabled && !EditorGizmoSystem.Instance.IsActiveGizmoReadyForObjectManipulation();
        }

        /// <summary>
        /// This method is used internally by the object selection system to verify if a game object can be picked 
        /// in the scene for the purposes of object selection/deselection. The method will always return false when
        /// the object selection system has been disabled via a call to 'SetEnabled'. Calling this method from other 
        /// parts of the code may prove useful in certain circumstances. This is why it was made public.
        /// </summary>
        public bool CanPickGameObject()
        {
            return CanOperate() && !_multiDeselect && GetGameObjectClickedHandler() != null;
        }

        /// <summary>
        /// This method is used internally by the object selection system to verify if game objects can be selected
        /// or deselected using the object selection shape. The method will always return false when the object 
        /// selection system has been disabled via a call to 'SetEnabled'. Calling this method from other parts of the 
        /// code may prove useful in certain circumstances. This is why it was made public.
        /// </summary>
        public bool CanPerformMultiSelect()
        {
            return CanOperate() & _mouse.IsLeftMouseButtonDown && GetGameObjectsEnteredSelectionShapeHandler() != null;
        }

        /// <summary>
        /// Checks if the specified game object is selected.
        /// </summary>
        public bool IsGameObjectSelected(GameObject gameObject)
        {
            return _selectedObjects.Contains(gameObject);
        }

        /// <summary>
        /// A gizmo needs access to the selected objects collection so that it can transform
        /// the selected objects accordingly. This method can be called to connect the game
        /// object selection to the specified gizmo.
        /// </summary>
        public void ConnectObjectSelectionToGizmo(Gizmo gizmo)
        {
            // TODO Connect gizmo to UI range
            gizmo.ControlledObjects = _selectedObjects;
        }

        /// <summary>
        /// Returns the world space AABB of the entire object selection. If no objects
        /// are selected, an invalid AABB will be returned.
        /// </summary>
        public Box GetWorldBox()
        {
            // If no objects are selected, return an invalid AABB
            if (_selectedObjects.Count == 0) return Box.GetInvalid();

            // Loop through all selected game objects
            Box selectionWorldBox = Box.GetInvalid();
            foreach (GameObject selectedObject in _selectedObjects)
            {
                // Get the object's world space AABB. If it is valid, add it to the current selection AABB.
                Box objectWorldBox = selectedObject.GetWorldBox();
                if (objectWorldBox.IsValid())
                {
                    // If the current selection AABB is valid, merge it with the object's AABB. Otherwise,
                    // perform an assignment to initialize it.
                    if (selectionWorldBox.IsValid()) selectionWorldBox.Encapsulate(objectWorldBox);
                    else selectionWorldBox = objectWorldBox;
                }
            }

            return selectionWorldBox;
        }

        /// <summary>
        /// Returns the selection's world center. This is the center of all selected objects'
        /// centers in world space.
        /// </summary>
        /// <remarks>
        /// If no object is currently selected, the method will return the zero vector.
        /// </remarks>
        public Vector3 GetSelectionWorldCenter()
        {
            return CalculateSelectionWorldCenter();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Performs any necessary initializations.
        /// </summary>
        private void Start()
        {
            MessageListenerDatabase listenerDatabase = MessageListenerDatabase.Instance;
            listenerDatabase.RegisterListenerForMessage(MessageType.ObjectSelectionModeChanged, this);
        }

        /// <summary>
        /// Performs any necessary updates.
        /// </summary>
        private void Update()
        {
            // Make sure we have access to the latest mouse info
            _mouse.UpdateInfoForCurrentFrame();

            // Make sure all null object references are removed. This is useful when
            // deleting objects from the scene.
            _selectedObjects.RemoveWhere(item => item == null);
            _maskedObjects.RemoveWhere(item => item == null);

            // Call the correct mouse event handlers if any important events have occured
            if (!WereAnyUIElementsHovered())
            {
                if (_mouse.WasLeftMouseButtonPressedInCurrentFrame) OnLeftMouseButtonDown();
                if (_mouse.WasMouseMovedSinceLastFrame) OnMouseMoved();
            }

            if (_mouse.WasLeftMouseButtonReleasedInCurrentFrame) OnLeftMouseButtonUp();
        }

        /// <summary>
        /// Called when the left mouse button is pressed during a frame update.
        /// </summary>
        private void OnLeftMouseButtonDown()
        {
            // When the left mouse button is pressed, we have to prepare for the possibility that the user
            // is trying to select objects using the object selection shape. So we will take a snapshot of
            // the current selection here just in case.
            _multiSelectPreChangeSnapshot = new ObjectSelectionSnapshot();
            _multiSelectPreChangeSnapshot.TakeSnapshot();

            // Check if a game object can be picked
            if (CanPickGameObject())
            {
                // Retrieve the picked game object
                GameObject pickedGameObject = GetPickedObject();
                if (pickedGameObject != null)
                {
                    // If a game object was picked, the selection may need to change. It all depends on how the click
                    // handler handles the pick operation. So we will take a snapshot of the current object selection
                    // in order to use it later if the selection has indeed changed.
                    _singleSelectPreChangeSnapshot = new ObjectSelectionSnapshot();
                    _singleSelectPreChangeSnapshot.TakeSnapshot();

                    // Call the handler and check if the selection was changed
                    if(GetGameObjectClickedHandler().Handle(pickedGameObject))
                    {
                        // The selection has changed. Take a snapshot post-change.
                        _singleSelectPostChangeSnapshot = new ObjectSelectionSnapshot();
                        _singleSelectPostChangeSnapshot.TakeSnapshot();

                        // Execute a post selection changed action to allow for undo/redo
                        var action = new PostObjectSelectionChangedAction(_singleSelectPreChangeSnapshot, _singleSelectPostChangeSnapshot);
                        action.Execute();
                    }
                }
                else
                // No game object was picked. When this is the case, it means that we have to clear the 
                // selection because the user has clicked thin air. We will only proceed if the selection
                // contains any objects. If it doesn't we will ignore this and move on because otherwise
                // we would be executing a post selection change action for no good reason.
                if(NumberOfSelectedObjects != 0)
                {
                    // The object selection contains objects, so we can clear it. Take a pre-change snapshot.
                    _singleSelectPreChangeSnapshot = new ObjectSelectionSnapshot();
                    _singleSelectPreChangeSnapshot.TakeSnapshot();

                    // Clear the selection
                    _selectedObjects.Clear();

                    // Take a post-change snapshot
                    _singleSelectPostChangeSnapshot = new ObjectSelectionSnapshot();
                    _singleSelectPostChangeSnapshot.TakeSnapshot();

                    // Execute a ost selection changed action to allow for undo/redo
                    var action = new PostObjectSelectionChangedAction(_singleSelectPreChangeSnapshot, _singleSelectPostChangeSnapshot);
                    action.Execute();
                }
            }
       
            // We will always update the coordinates of the selection shape when the left mouse button
            // is pressed to ensure that it is positioned correctly on the screeen if it later becomes
            // visible. Note that we set both corner points to the exact same value. This is because when
            // the left mouse button is pressed, the selection shape should have no area. Only when the
            // mouse is moved while holding the left mouse button down will the selection shape's area
            // grow as needed.
            ObjectSelectionShape objectSelectionShape = GetObjectSelectionShape();
            objectSelectionShape.SetEnclosingRectBottomRightPoint(Input.mousePosition);
            objectSelectionShape.SetEnclosingRectTopLeftPoint(Input.mousePosition);

            // Adjust the visibility of the selection shape
            if (ShouldMultiObjectSelectionShapeBeVisible()) objectSelectionShape.IsVisible = true;
            else objectSelectionShape.IsVisible = false;
        }

        /// <summary>
        /// Checks if the object selection shape should be visible.
        /// </summary>
        private bool ShouldMultiObjectSelectionShapeBeVisible()
        {
            // The object selection shape can be visible only when the object selection
            // system is allowed to operate.
            return CanOperate();
        }

        /// <summary>
        /// Called when the left mouse button is released during the current frame update.
        /// </summary>
        private void OnLeftMouseButtonUp()
        {
            GetObjectSelectionShape().IsVisible = false;
            CommitMultiObjectSelectionWithSelectionShape();
        }

        /// <summary>
        /// Commits any changes that were made using the object selection shape.
        /// </summary>
        private void CommitMultiObjectSelectionWithSelectionShape()
        {
            // If the selection has changed since the left mouse button was pressed (because the user
            // has selected/deselected objects using the selection shape), we will have to execute a
            // post selection changed action to allow for undo/redo of the multi-select operations.
            if (_wasSelectionChangedWithSelectionShape)
            {
                // Take post-change snapshot.
                // Note: The pre-change snapshot was taken when the left mouse button was pressed.
                _multiSelectPostChangeSnapshot = new ObjectSelectionSnapshot();
                _multiSelectPostChangeSnapshot.TakeSnapshot();

                // Execute the post seelection changed action to allow for undo/redo
                var action = new PostObjectSelectionChangedAction(_multiSelectPreChangeSnapshot, _multiSelectPostChangeSnapshot);
                action.Execute();

                // Prepare for the next multi-select sesssion
                _wasSelectionChangedWithSelectionShape = false;
            }
        }

        /// <summary>
        /// Called when the mouse was moved during the current frame update.
        /// </summary>
        private void OnMouseMoved()
        {
            // Handle object multi-selection using the object selection shape if we are allowed
            if (CanPerformMultiSelect())
            {
                // Adjust the corner of the selection shape's enclosing rectangle to the current mouse position.
                // This ensures that the selection shape is adjusted based on how the mouse is moved.
                GetObjectSelectionShape().SetEnclosingRectTopLeftPoint(Input.mousePosition);

                // Retrieve the game objects which are visible to the camera and identify the objects which are
                // intersected by the object selection shape. Store those objects in 'objectsInSelectionShape'.
                List<GameObject> objectsVisibleToCamera = EditorCamera.Instance.GetVisibleGameObjects();
                List<GameObject> objectsInSelectionShape = GetObjectSelectionShape().GetIntersectingGameObjects(objectsVisibleToCamera, EditorCamera.Instance.Camera);

                // Call the objects entered selection shape handler and if the selection was changed, set the
                // '_wasSelectionChangedWithSelectionShape' variabke to true. When the left mouse button is
                // released, we will check to see if this variable is true. If it is, we will perform a post
                // selection changed event.
                if (GetGameObjectsEnteredSelectionShapeHandler().Handle(objectsInSelectionShape)) _wasSelectionChangedWithSelectionShape = true;
            }
        }

        /// <summary>
        /// Casts a ray in the scene using the mouse cursor position and returns
        /// the picked game object. If no game object is picked, the method will
        /// return null.
        /// </summary>
        private GameObject GetPickedObject()
        {
            GameObject pickedObject = null;
            GameObjectRayHit gameObjectRayHit = null;
            bool requireFilter = !ObjectSelectionSettings.CanSelectTerrainObjects;
            if (requireFilter) MouseCursor.Instance.PushObjectPickMaskFlags(MouseCursorObjectPickFlags.ObjectTerrain);

            MouseCursorRayHit cursorRayHit = MouseCursor.Instance.GetRayHit(ObjectSelectionSettings.SelectableLayers);
            if (cursorRayHit.WasAnObjectHit)
            {
                List<GameObjectRayHit> objectRayHits = cursorRayHit.SortedObjectRayHits;
                if (!ObjectSelectionSettings.CanSelectLightObjects) objectRayHits.RemoveAll(item => !item.HitObject.HasMesh() && item.HitObject.HasLight());
                if (!ObjectSelectionSettings.CanSelectParticleSystemObjects) objectRayHits.RemoveAll(item => !item.HitObject.HasMesh() && item.HitObject.HasParticleSystem());
                if (!ObjectSelectionSettings.CanSelectSpriteObjects) objectRayHits.RemoveAll(item => item.HitObject.IsSprite());
                if (!ObjectSelectionSettings.CanSelectEmptyObjects) objectRayHits.RemoveAll(item => item.HitObject.IsEmpty());

                if (objectRayHits.Count != 0) 
                {
                    gameObjectRayHit = objectRayHits[0];
                    pickedObject = gameObjectRayHit.HitObject;
                }
            }

            /*if (!ObjectSelectionSettings.IgnoreUnityColliders)
            {
                Ray ray = EditorCamera.Instance.Camera.ScreenPointToRay(Input.mousePosition);
                List<RaycastHit> sortedColliderHits = PhysicsHelper.RaycastAllSorted(ray, ObjectSelectionSettings.SelectableLayers);
                if (!ObjectSelectionSettings.CanSelectTerrainObjects) sortedColliderHits.RemoveAll(item => item.collider.gameObject.HasTerrain());
                if (!ObjectSelectionSettings.CanSelectLightObjects) sortedColliderHits.RemoveAll(item => !item.collider.gameObject.HasMesh() && item.collider.gameObject.HasLight());
                if (!ObjectSelectionSettings.CanSelectParticleSystemObjects) sortedColliderHits.RemoveAll(item => !item.collider.gameObject.HasMesh() && item.collider.gameObject.HasParticleSystem());

                if(sortedColliderHits.Count != 0)
                {
                    if (gameObjectRayHit == null) pickedObject = sortedColliderHits[0].collider.gameObject;
                    else
                    {
                        Vector3 camPos = EditorCamera.Instance.Camera.transform.position;
                        RaycastHit closestColliderHit = sortedColliderHits[0];

                        float d0 = (gameObjectRayHit.HitPoint - camPos).magnitude;
                        float d1 = (closestColliderHit.point - camPos).magnitude;
                        if (d1 < d0) pickedObject = closestColliderHit.collider.gameObject;
                    }
                }
            }*/

            return pickedObject;
        }

        /// <summary>
        /// Returns the game object clicked handler which handles the event of an
        /// object being clicked by the mouse cursor.
        /// </summary>
        private ObjectSelectionGameObjectClickedHandler GetGameObjectClickedHandler()
        {
            return ObjectSelectionGameObjectClickedHandlerFactory.Create(_objectSelectionSettings.ObjectSelectionMode);
        }

        /// <summary>
        /// Returns the game objects entered selection shape handler which handles the 
        /// event of a group of objects entering the object selection shape area.
        /// </summary>
        private ObjectSelectionGameObjectsEnteredSelectionShapeHandler GetGameObjectsEnteredSelectionShapeHandler()
        {
            return ObjectSelectionGameObjectsEnteredSelectionShapeHandlerFactory.Create(_objectSelectionSettings.ObjectSelectionMode);
        }

        /// <summary>
        /// Returns the currently active selection shape.
        /// </summary>
        private ObjectSelectionShape GetObjectSelectionShape()
        {
            // Note: There is only one line of code here at the moment, but this may
            //       change if different selection shapes are to be supported in the
            //       future.
            return _objectSelectionRectangle;
        }

        /// <summary>
        /// Recalculates the object selection world center. If no objects are selected when this 
        /// method is called, the method will return the zero vector. The method also returns the 
        /// 0 vector if there are selected objects, but none of them are suitable for contributing 
        /// to the center calculation.
        /// </summary>
        private Vector3 CalculateSelectionWorldCenter()
        {
            // If no objects are selected, we will use the zero vector
            if (_selectedObjects.Count == 0) return Vector3.zero;
            else
            {
                // Calculate the sum of all objects' world centers
                int numberOfObjects = 0;
                Vector3 objectCenterSum = Vector3.zero;
                foreach (GameObject selectedObject in _selectedObjects)
                {
                    // Get the world AABB of the game objects and if it is valid, add its contribution to the center sum
                    Box worldAABB = selectedObject.GetWorldBox();
                    if (worldAABB.IsValid())
                    {
                        // Add to sum and increase the number of objects which contribute to the center calculation
                        objectCenterSum += worldAABB.Center;
                        ++numberOfObjects;
                    }
                }

                // If no objects were found suitable for contributing to the center calculation, we will return the 0 vector.
                if (numberOfObjects == 0) return Vector3.zero;

                // Now calculate the average and store it inside the '_selectionWorldCenter' variable
                return objectCenterSum / numberOfObjects;
            }
        }

        /// <summary>
        /// Returns a game object which resides inside the object selection collections. This method
        /// is necessary because we are using a hash-set to store the selected objects and we can not
        /// index it as we would a list or array. If no selected objects are available, the method will
        /// return null.
        /// </summary>
        private GameObject RetrieveAGameObjectFromObjectSelectionCollection()
        {
            // Just get the first object from the set
            foreach (GameObject selectedGameObject in _selectedObjects)
            {
                return selectedGameObject;
            }

            // If there are no selected game objects, return null
            return null;
        }

        /// <summary>
        /// This method is called after the camera has finished rendering the scene. 
        /// It allows us to perform any necessary drawing.
        /// </summary>
        private void OnRenderObject()
        {
            if (Camera.current != EditorCamera.Instance.Camera) return;

            // Render the object selection boxes
            if(ObjectSelectionSettings.ObjectSelectionBoxRenderSettings.DrawBoxes)
            {
                ObjectSelectionRenderer objectSelectionRenderer = ObjectSelectionRendererFactory.Create(_objectSelectionSettings.ObjectSelectionRenderMode);
                objectSelectionRenderer.RenderObjectSelection(_selectedObjects, _objectSelectionSettings);
            }

            // Render the selection shape
            GetObjectSelectionShape().Render();
        }

        /// <summary>
        /// Checks if the mouse cursor hovers any UI elements. This is useful in order handle situations in 
        /// which the left mouse button is clicked while hovering a UI element but not an actual scene game 
        /// object. It helps us avoid deselecting the selected game objects when an UI element is clicked.
        /// </summary>
        private bool WereAnyUIElementsHovered()
        {
            if (EventSystem.current == null) return false;

            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return results.Count != 0;
        }
        #endregion

        #region Message Handlers
        /// <summary>
        /// 'IMessageListener' interface method implementation.
        /// </summary>
        public void RespondToMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageType.ObjectSelectionModeChanged:

                    RespondToMessage(message as ObjectSelectionModeChangedMessage);
                    break;
            }
        }

        /// <summary>
        /// This method is called in order to respond to an object selection mode changed message.
        /// </summary>
        private void RespondToMessage(ObjectSelectionModeChangedMessage message)
        {
            // When the selection mode is changed, we have to make sure that the selection is refreshed
            // in order to ensure that the object selection is compatible with the new selection mode. 
            // In order to achieve this, we will pretend that the current selection is empty and call
            // the corresponding objects entered selection shape handler on the currently selected
            // objects.
            var selectedObjectsList = new List<GameObject>(_selectedObjects);
            _selectedObjects.Clear();

            // We want the handler to treat the normal case where no append or multi-deselect
            // is available. So we will store their states here, set them to false and restore 
            // them later after the handler has finished doing its job.
            bool oldAppendOnDeselectOnClick = AppendOrDeselectOnClick;
            bool oldMultiDeselect = MultiDeselect;

            AppendOrDeselectOnClick = false;
            MultiDeselect = false;

            // Call the handler
            ObjectSelectionGameObjectsEnteredSelectionShapeHandler handler = GetGameObjectsEnteredSelectionShapeHandler();
            if (handler != null) handler.Handle(selectedObjectsList);

            // The selection has changed, so send a message to all interested listeners
            ObjectSelectionChangedMessage.SendToInterestedListeners();

            // Restore append and multi-deselect states
            AppendOrDeselectOnClick = oldAppendOnDeselectOnClick;
            MultiDeselect = oldMultiDeselect;
        }
        #endregion
    }
}
