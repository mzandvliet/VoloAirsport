using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTEditor
{
    /// <summary>
    /// The gizmo system manages all the gizmos. It controls their position and orientation
    /// based on what happens in the scene (i.e. how the user moves the mouse, what objects
    /// are selected etc). 
    /// </summary>
    public class EditorGizmoSystem : MonoSingletonBase<EditorGizmoSystem>, IMessageListener
    {
        #region Private Variables
//        /// <summary>
//        /// The translation gizmo which is used to move objects in the scene.
//        /// </summary>
//        [SerializeField]
//        private TranslationGizmo _translationGizmo;

        /// <summary>
        /// The rotation gizmo which is used to rotate objects in the scene.
        /// </summary>
        [SerializeField]
        private RotationGizmo _rotationGizmo;

//        /// <summary>
//        /// The scale gizmo which is used to scale objects in the scene.
//        /// </summary>
//        [SerializeField]
//        private ScaleGizmo _scaleGizmo;

        /// <summary>
        /// This is the gizmo that is currently being used to transform objects in the scene.
        /// </summary>
        /// <remarks>
        /// By 'active' it is meant that it was selected by the user to be used for object manipulation.
        /// It does not necessarily mean that it is active in the scene. For example, when there is no
        /// object selected in the scene, '_activeGizmo' will reference a gizmo object which is inactive
        /// in the scene.
        /// </remarks>
        private Gizmo _activeGizmo;

        /// <summary>
        /// This is the transform space in which the gizmos will be transforming their controlled objects. You
        /// can change this from the Inspector GUI to establish the initial transform space.
        /// </summary>
        [SerializeField]
        private TransformSpace _transformSpace = TransformSpace.Global;

        /// <summary>
        /// Stores the type of the currently active gizmo. You can change this in the Inspector GUI to establish
        /// the initial transform gizmo that must be activated on the first object selection operation.
        /// </summary>
        [SerializeField]
        private GizmoType _activeGizmoType = GizmoType.Translation;

        /// <summary>
        /// This is the transform pivot point which must be used by all gizmos to transform their objects.
        /// </summary>
        [SerializeField]
        private TransformPivotPoint _transformPivotPoint = TransformPivotPoint.Center;

        /// <summary>
        /// If this variable is set to true, gizmos are turned off. This means that when objects are selected,
        /// no gizmo will be shown. This mode is useful when the user would like to perform simple object selections
        /// without having to worry about the gizmos.
        /// </summary>
        private bool _areGizmosTurnedOff = false;
        #endregion

        #region Public Properties
//        /// <summary>
//        /// Gets/sets the associated translation gizmo. Settings the gizmo to null or to a prefab,
//        /// will have no effect. Only non-null scene object instances are allowed.
//        /// </summary>
//        public TranslationGizmo TranslationGizmo
//        {
//            get { return _translationGizmo; }
//            set
//            {
//                if (value == null) return;
//
//                // Allow only scene objects
//                #if UNITY_EDITOR
//                if (value.gameObject.IsSceneObject()) _translationGizmo = value;
//                else Debug.LogWarning("RTEditorGizmoSystem.TranslationGizmo: Only scene gizmo object instances are allowed.");
//                #else
//                _translationGizmo = value;
//                #endif
//            }
//        }

        /// <summary>
        /// Gets/sets the associated rotation gizmo. Settings the gizmo to null or to a prefab,
        /// will have no effect. Only non-null scene object instances are allowed.
        /// </summary>
        public RotationGizmo RotationGizmo
        {
            get { return _rotationGizmo; }
            set
            {
                if (value == null) return;

                // Allow only scene objects
                #if UNITY_EDITOR
                if (value.gameObject.IsSceneObject()) _rotationGizmo = value;
                else Debug.LogWarning("EditorGizmoSystem.RotationGizmo: Only scene gizmo object instances are allowed.");
                #else
                _rotationGizmo = value;
                #endif
            }
        }

//        /// <summary>
//        /// Gets/sets the associated scale gizmo. Settings the gizmo to null or to a prefab,
//        /// will have no effect. Only non-null scene object instances are allowed.
//        /// </summary>
//        public ScaleGizmo ScaleGizmo
//        {
//            get { return _scaleGizmo; }
//            set
//            {
//                if (value == null) return;
//
//                // Allow only scene objects
//                #if UNITY_EDITOR
//                if (value.gameObject.IsSceneObject()) _scaleGizmo = value;
//                else Debug.LogWarning("EditorGizmoSystem.ScaleGizmo: Only scene gizmo object instances are allowed.");
//                #else
//                _scaleGizmo = value;
//                #endif
//            }
//        }

        /// <summary>
        /// Gets/sets the transform space in which the gizmos are transforming their controlled objects.
        /// </summary>
        public TransformSpace TransformSpace
        {
            get { return _transformSpace; }
            set { ChangeTransformSpace(value); }
        }

        /// <summary>
        /// Gets/sets the active gizmo type. Changing this property will automatically change the curent gizmo
        /// that is used to transform the selected objects. 
        /// </summary>
        public GizmoType ActiveGizmoType
        {
            get { return _activeGizmoType; }
            set { ChangeActiveGizmo(value); }
        }

        /// <summary>
        /// Returns the active gizmo as indicated by the 'ActiveGizmoType' property.
        /// </summary>
        /// <remarks>
        /// The gizmo object might not be active in the scene if gizmos were turned off.
        /// </remarks>
        public Gizmo ActiveGizmo { get { return _activeGizmo; } }

        /// <summary>
        /// Gets/sets the transform pivot point which must be used by all gizmos to transform their objects.
        /// </summary>
        public TransformPivotPoint TransformPivotPoint
        {
            get { return _transformPivotPoint; }
            set { ChangeTransformPivotPoint(value); }
        }

        /// <summary>
        /// Can be used to check whether or not gizmos are turned off.
        /// </summary>
        public bool AreGizmosTurnedOff { get { return _areGizmosTurnedOff; } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks if the active gizmo is ready for object manipulation.
        /// </summary>
        /// <remarks>
        /// If the active gizmo is not active in the scene, the method returns false.
        /// </remarks>
        public bool IsActiveGizmoReadyForObjectManipulation()
        {
            if (_activeGizmo == null || !_activeGizmo.gameObject.activeSelf) return false;
            return _activeGizmo.IsReadyForObjectManipulation();
        }

        /// <summary>
        /// This method will turn of all gizmo objects. After calling this method, no gizmo
        /// will be active in the scene, not even when the user is selecting objects.
        /// </summary>
        /// <remarks>
        /// Gizmos can be turned on again by setting the 'ActiveGizmoType' property.
        /// </remarks>
        public void TurnOffGizmos()
        {
            _areGizmosTurnedOff = true;
            DeactivateAllGizmoObjects();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Performs any necessary initializations.
        /// </summary>
        private void Start()
        {
            // Make sure all properties are valid
            ValidatePropertiesForRuntime();

            DeactivateAllGizmoObjects();                    // Initially, all gizmo objects are deactivated. Whenever the user selects the first object (or group of objects), the correct gizmo will be activated).
            ConnectObjectSelectionToGizmos();               // Make sure the gizmos know which objects they control
            ActiveGizmoType = _activeGizmoType;             // Make sure we are using the correct gizmo initially 
            TransformPivotPoint = _transformPivotPoint;     // Make sure the transform pivot point is setup correctly
           
            // Register as listener
            MessageListenerDatabase listenerDatabase = MessageListenerDatabase.Instance;
            listenerDatabase.RegisterListenerForMessage(MessageType.GizmoTransformedObjects, this);
            listenerDatabase.RegisterListenerForMessage(MessageType.ObjectSelectionChanged, this);
            listenerDatabase.RegisterListenerForMessage(MessageType.GizmoTransformOperationWasUndone, this);
            listenerDatabase.RegisterListenerForMessage(MessageType.GizmoTransformOperationWasRedone, this);
            listenerDatabase.RegisterListenerForMessage(MessageType.VertexSnappingDisabled, this);
        }

        /// <summary>
        /// The method ensures that all properties are valid such that the gizmo system
        /// can be used at runtime.
        /// </summary>
        private void ValidatePropertiesForRuntime()
        {
            // Make sure all properties have been set up correctly
            bool allPropertiesAreValid = true;
//            if (_translationGizmo == null)
//            {
//                Debug.LogError("EditorGizmoSystem.Start: Missing translation gizmo. Please assign a game object with the 'TranslationGizmo' script attached to it.");
//                allPropertiesAreValid = false;
//            }

            if (_rotationGizmo == null)
            {
                Debug.LogError("EditorGizmoSystem.Start: Missing rotation gizmo. Please assign a game object with the 'RotationGizmo' script attached to it.");
                allPropertiesAreValid = false;
            }

//            if (_scaleGizmo == null)
//            {
//                Debug.LogError("EditorGizmoSystem.Start: Missing scale gizmo. Please assign a game object with the 'ScaleGizmo' script attached to it.");
//                allPropertiesAreValid = false;
//            }

            // If not all properties have been set up correctly, we will quit the application
            if (!allPropertiesAreValid) ApplicationHelper.Quit();
        }

        /// <summary>
        /// This method is called from 'Start' in order to connect the object selection collection
        /// to each of the 3 gizmos. We need to do this because the gizmos need to know about the
        /// objects that they control.
        /// </summary>
        private void ConnectObjectSelectionToGizmos()
        {
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;
            //objectSelection.ConnectObjectSelectionToGizmo(_translationGizmo);
            objectSelection.ConnectObjectSelectionToGizmo(_rotationGizmo);
            //objectSelection.ConnectObjectSelectionToGizmo(_scaleGizmo);
        }

        /// <summary>
        /// Deactivates all gizmo objects.
        /// </summary>
        private void DeactivateAllGizmoObjects()
        {
            //_translationGizmo.gameObject.SetActive(false);
            _rotationGizmo.gameObject.SetActive(false);
            //_scaleGizmo.gameObject.SetActive(false);
        }

        /// <summary>
        /// Changes the active gizmo to the gizmo which is identified by the specified type.
        /// </summary>
        /// <remarks>
        /// Calling this method will set the '_areGizmosTurnedOff' boolean to false (i.e. will
        /// reenable gizmos).
        /// </remarks>
        private void ChangeActiveGizmo(GizmoType gizmoType)
        {
            // Gizmos are no longer turned off
            _areGizmosTurnedOff = false;

            // We will need this later
            Gizmo oldActiveGizmo = _activeGizmo;

            // Change the active gizmo type
            _activeGizmoType = gizmoType;
            _activeGizmo = GetGizmoByType(gizmoType);

            // Deactivate the old active gizmo and make sure that the new gizmo has its position and
            // orientation updated accordingly.
            if (oldActiveGizmo != null)
            {
                // Deactivate the old gizmo
                oldActiveGizmo.gameObject.SetActive(false);

                // We will inherit the position so that when the new gizmo appears, it 
                // appears in the same place as the old active gizmo.
                _activeGizmo.transform.position = oldActiveGizmo.transform.position;

                // Establish the rotation of the new active gizmo
                UpdateActiveGizmoRotation();
            }

            // If there are any objects selected, we will make sure that the new active gizmo is active in the scene.
            // If no objects are selected, we will deactivate it. We do this because we only want to draw the active
            // gizmo when there are selected objects in the scene. If no objects are selected, there is nothing to
            // transform.
            if (EditorObjectSelection.Instance.NumberOfSelectedObjects != 0) _activeGizmo.gameObject.SetActive(true);
            else _activeGizmo.gameObject.SetActive(false);

            // When the active gizmo is changed, always make sure that vertex snapping is disabled for the translation gizmo.
            // Otherwise, if you change from translation to rotation while vertex snapping is enabled and then enable the
            // translation gizmo again, it will be activated with vertex snapping enabled, which is not really desirable.
            //_translationGizmo.SnapSettings.IsVertexSnappingEnabled = false;
        }

        /// <summary>
        /// Changes the active transform space to the specified value.
        /// </summary>
        private void ChangeTransformSpace(TransformSpace transformSpace)
        {
            // Set the new transform space and make sure the active gizmo has its rotation updated accordingly
            _transformSpace = transformSpace;
            UpdateActiveGizmoRotation();
        }

        /// <summary>
        /// Changes the transform pivot point to the specified value.
        /// </summary>
        private void ChangeTransformPivotPoint(TransformPivotPoint transformPivotPoint)
        {
            // Store the new pivot point
            _transformPivotPoint = transformPivotPoint;

            // Set the pivot point for each gizmo
            //_translationGizmo.TransformPivotPoint = _transformPivotPoint;
            _rotationGizmo.TransformPivotPoint = _transformPivotPoint;
            //_scaleGizmo.TransformPivotPoint = _transformPivotPoint;

            // Establish the position of the active gizmo
            EstablishActiveGizmoPosition();
        }

        /// <summary>
        /// The method will return one of the gizmos managed by the gizmo system which corresponds
        /// to the specified gizmo type.
        /// </summary>
        private Gizmo GetGizmoByType(GizmoType gizmoType)
        {
            //if (gizmoType == GizmoType.Translation) return _translationGizmo;
            /*else*/ if (gizmoType == GizmoType.Rotation) return _rotationGizmo;
            //return _scaleGizmo;
            return null;
        }

        /// <summary>
        /// This method is called whenever the position of the active gizmo needs to be updated.
        /// </summary>
        private void EstablishActiveGizmoPosition()
        {
            // We will only update the position of the gizmo when it is not used to transform objects
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;
            if (_activeGizmo != null && _activeGizmo.gameObject.activeSelf && !_activeGizmo.IsTransformingObjects())
            {
                // Update the position based on the specified transform pivot point. If the transform pivot
                // point is set to 'MeshPivot', we will set the position of the gizmo to the position of the
                // last selected game objects. Otherwise, we will set it to the center of the selection.
                if (_transformPivotPoint == TransformPivotPoint.MeshPivot && objectSelection.LastSelectedGameObject != null) _activeGizmo.transform.position = objectSelection.LastSelectedGameObject.transform.position;
                else _activeGizmo.transform.position = objectSelection.GetSelectionWorldCenter();
            }
        }

        /// <summary>
        /// Updates the rotation of the active gizmo by taking into consideration
        /// all necessary factors such as the active gizmo transform space.
        /// </summary>
        private void UpdateActiveGizmoRotation()
        {
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;

            // If the global transform space is used, we will set the gizmo's rotation to identity. Otherwise,
            // we will set the rotation to the rotation of the last object which was selected in the scene.
            // Note: The scale gizmo will always be oriented in the last selected object's local space because
            //       the scale gizmo always scales along the objects' local axes.
            if ((_transformSpace == TransformSpace.Global && _activeGizmoType != GizmoType.Scale) || objectSelection.LastSelectedGameObject == null) _activeGizmo.transform.rotation = Quaternion.identity;
            else _activeGizmo.transform.rotation = objectSelection.LastSelectedGameObject.transform.rotation;
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
                case MessageType.GizmoTransformedObjects:

                    RespondToMessage(message as GizmoTransformedObjectsMessage);
                    break;

                case MessageType.ObjectSelectionChanged:

                    RespondToMessage(message as ObjectSelectionChangedMessage);
                    break;

                case MessageType.GizmoTransformOperationWasUndone:

                    RespondToMessage(message as GizmoTransformOperationWasUndoneMessage);
                    break;
                
                case MessageType.GizmoTransformOperationWasRedone:

                    RespondToMessage(message as GizmoTransformOperationWasRedoneMessage);
                    break;

                case MessageType.VertexSnappingDisabled:

                    RespondToMessage(message as VertexSnappingDisabledMessage);
                    break;
            }
        }

        private void RespondToMessage(GizmoTransformedObjectsMessage message)
        {
            UpdateActiveGizmoRotation();
            EstablishActiveGizmoPosition();
        }

        /// <summary>
        /// This method is called to respond to an object selection changed message.
        /// </summary>
        private void RespondToMessage(ObjectSelectionChangedMessage message)
        {
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;

            // If no objects are selected, we will deactivate the active gizmo
            if (objectSelection.NumberOfSelectedObjects == 0) _activeGizmo.gameObject.SetActive(false);
            else
            // If the gizmos are not turned off, we may need to enable the active gizmo in the scene
            if (!_areGizmosTurnedOff)
            {
                // If there are objects selected, we will make sure the active gizmo is enabled in the scene.
                if (objectSelection.NumberOfSelectedObjects != 0 && !_activeGizmo.gameObject.activeSelf) _activeGizmo.gameObject.SetActive(true);

                // Make sure the position of the active gizmo is updated correctly
                EstablishActiveGizmoPosition();

                // Now we must make sure that the active gizmo is oriented accordingly
                UpdateActiveGizmoRotation();
            }
        }

        /// <summary>
        /// This method is called to respond to a gizmo transform operation undone message.
        /// </summary>
        private void RespondToMessage(GizmoTransformOperationWasUndoneMessage message)
        {
            // When the transform operation is undone, it means the objects position/rotation/scale
            // has changed, so we have to recalculate the position and orientation of the active gizmo.
            EstablishActiveGizmoPosition();
            UpdateActiveGizmoRotation();
        }

        /// <summary>
        /// This method is called to respond to a gizmo transform operation redone message.
        /// </summary>
        private void RespondToMessage(GizmoTransformOperationWasRedoneMessage message)
        {
            // When the transform operation is redone, it means the objects position/rotation/scale
            // has changed, so we have to recalculate the position and orientation of the active gizmo.
            EstablishActiveGizmoPosition();
            UpdateActiveGizmoRotation();
        }

        /// <summary>
        /// This method is called to respond to a vertex snapping disabled message.
        /// </summary>
        private void RespondToMessage(VertexSnappingDisabledMessage message)
        {
            // When vertex snapping is disabled, make sure that the active gizmo is positioned
            // accordingly because when vertex snapping is used, its position my change to that
            // of the object mesh vertices.
            EstablishActiveGizmoPosition();
        }
        #endregion
    }
}