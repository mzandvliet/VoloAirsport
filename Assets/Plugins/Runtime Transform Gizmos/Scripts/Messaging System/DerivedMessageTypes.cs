using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This message is sent when a gizmo transforms (i.e. translates, rotates or scales) its controlled game objects.
    /// </summary>
    public class GizmoTransformedObjectsMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// This is the gizmo that had its controlled game objects transformed. The objects
        /// can be retrieved via a call to the 'Gizmo.ControlledObjects' property.
        /// </summary>
        private Gizmo _gizmo;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the gizmo that transformed its controlled game objects.
        /// </summary>
        public Gizmo Gizmo { get { return _gizmo; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public GizmoTransformedObjectsMessage(Gizmo gizmo)
            : base(MessageType.GizmoTransformedObjects)
        {
            _gizmo = gizmo;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a gizmo transformed objects message to 
        /// all interested listeners.
        /// </summary>
        /// <param name="gizmo">
        /// The gizmo which transformed objects.
        /// </param>
        public static void SendToInterestedListeners(Gizmo gizmo)
        {
            var message = new GizmoTransformedObjectsMessage(gizmo);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when the object selection changes.
    /// </summary>
    public class ObjectSelectionChangedMessage : Message
    {
        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectSelectionChangedMessage()
            : base(MessageType.ObjectSelectionChanged)
        {
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending an object selection changed message to 
        /// all interested listeners.
        /// </summary>
        public static void SendToInterestedListeners()
        {
            var message = new ObjectSelectionChangedMessage();
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when the object selection mode is changed.
    /// </summary>
    public class ObjectSelectionModeChangedMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// The new object selection mode which was activated.
        /// </summary>
        private ObjectSelectionMode _newObjectSelectionMode;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the new object selection mode which was activated.
        /// </summary>
        public ObjectSelectionMode NewObjectSelectionMode { get { return _newObjectSelectionMode; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="newObjectSelectionMode">
        /// The new object selection mode which was activated.
        /// </param>
        public ObjectSelectionModeChangedMessage(ObjectSelectionMode newObjectSelectionMode)
            : base(MessageType.ObjectSelectionModeChanged)
        {
            _newObjectSelectionMode = newObjectSelectionMode;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending an object mode selection changed message to 
        /// all interested listeners.
        /// </summary>
        public static void SendToInterestedListeners(ObjectSelectionMode newObjectSelectionMode)
        {
            var message = new ObjectSelectionModeChangedMessage(newObjectSelectionMode);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when objects are added to the object selection mask.
    /// </summary>
    public class ObjectsAddedToSelectionMaskMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// These are the game objects which were added to the selection mask.
        /// </summary>
        private List<GameObject> _gameObjects;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the game object collection which was added to the selection mask.
        /// </summary>
        public List<GameObject> GameObjects { get { return _gameObjects; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameObjects">
        /// These are the game objects which were added to the selection mask.
        /// </param>
        public ObjectsAddedToSelectionMaskMessage(List<GameObject> gameObjects)
            : base(MessageType.ObjectsAddedToSelectionMask)
        {
            _gameObjects = new List<GameObject>(gameObjects);
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending an objects added to selection mask message to
        /// all interested listeners.
        /// </summary>
        /// <param name="gameObjects">
        /// The game objects which were added to the selection mask.
        /// </param>
        public static void SendToInterestedListeners(List<GameObject> gameObjects)
        {
            var message = new ObjectsAddedToSelectionMaskMessage(gameObjects);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when objects are removed from the object selection mask.
    /// </summary>
    public class ObjectsRemovedFromSelectionMaskMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// These are the game objects which were removed from the selection mask.
        /// </summary>
        private List<GameObject> _gameObjects;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the game object collection which was removed from the selection mask.
        /// </summary>
        public List<GameObject> GameObjects { get { return _gameObjects; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameObjects">
        /// These are the game objects which were removed from the selection mask.
        /// </param>
        public ObjectsRemovedFromSelectionMaskMessage(List<GameObject> gameObjects)
            : base(MessageType.ObjectsRemovedFromSelectionMask)
        {
            _gameObjects = new List<GameObject>(gameObjects);
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending an objects removed from selection mask message to
        /// all interested listeners.
        /// </summary>
        /// <param name="gameObjects">
        /// The game objects which were removed from the selection mask.
        /// </param>
        public static void SendToInterestedListeners(List<GameObject> gameObjects)
        {
            var message = new ObjectsRemovedFromSelectionMaskMessage(gameObjects);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when the transform space changes.
    /// </summary>
    public class TransformSpaceChangedMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// This is the old transform space.
        /// </summary>
        private TransformSpace _oldTransformSpace;

        /// <summary>
        /// This is the new transform space.
        /// </summary>
        private TransformSpace _newTransformSpace;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the old transform space.
        /// </summary>
        public TransformSpace OldTransformSpace { get { return _oldTransformSpace; } }

        /// <summary>
        /// Returns the new transform space.
        /// </summary>
        public TransformSpace NewTransformSpace { get { return _newTransformSpace; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public TransformSpaceChangedMessage(TransformSpace oldTransformSpace, TransformSpace newTransformSpace)
            : base(MessageType.TransformSpaceChanged)
        {
            _oldTransformSpace = oldTransformSpace;
            _newTransformSpace = newTransformSpace;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a transform space changed message to 
        /// all interested listeners.
        /// </summary>
        /// <param name="oldTransformSpace">
        /// The old transform space before it was changed.
        /// </param>
        /// <param name="newTransformSpace">
        /// The new transform space.
        /// </param>
        public static void SendToInterestedListeners(TransformSpace oldTransformSpace, TransformSpace newTransformSpace)
        {
            var message = new TransformSpaceChangedMessage(oldTransformSpace, newTransformSpace);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when the gizmos are turned off.
    /// </summary>
    public class GizmosTurnedOffMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// This represents the type of gizmo that was active before the gizmos were turned off.
        /// </summary>
        private GizmoType _activeGizmoType;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the type of gizmo which was active before the gizmos were turned off.
        /// </summary>
        public GizmoType ActiveGizmoType { get { return _activeGizmoType; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activeGizmoType">
        /// This represents the type of gizmo that was active before the gizmos
        /// were turned off.
        /// </param>
        public GizmosTurnedOffMessage(GizmoType activeGizmoType) 
            : base(MessageType.GizmosTurnedOff)
        {
            _activeGizmoType = activeGizmoType;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a gizmos turned off message to 
        /// all interested listeners.
        /// </summary>
        /// <param name="activeGizmoType">
        /// This represents the type of gizmo that was active before the gizmos
        /// were turned off.
        /// </param>
        public static void SendToInterestedListeners(GizmoType activeGizmoType)
        {
            var message = new GizmosTurnedOffMessage(activeGizmoType);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when the transform pivot point is changed.
    /// </summary>
    public class TransformPivotPointChangedMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// This is the old pivot point before it was changed.
        /// </summary>
        private TransformPivotPoint _oldPivotPoint;

        /// <summary>
        /// This is the new pivot point.
        /// </summary>
        private TransformPivotPoint _newPivotPoint;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the old pivot point before it was changed.
        /// </summary>
        public TransformPivotPoint OldPivotPoint { get { return _oldPivotPoint; } }

        /// <summary>
        /// Returns the new pivot point.
        /// </summary>
        public TransformPivotPoint NewPivotPoint { get { return _newPivotPoint; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="oldPivotPoint">
        /// This is the old pivot point before it was changed.
        /// </param>
        /// <param name="newPivotPoint">
        /// This is the new pivot point.
        /// </param>
        public TransformPivotPointChangedMessage(TransformPivotPoint oldPivotPoint, TransformPivotPoint newPivotPoint)
            : base(MessageType.TransformPivotPointChanged)
        {
            _oldPivotPoint = oldPivotPoint;
            _newPivotPoint = newPivotPoint;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a transform pivot point changed message to 
        /// all interested listeners.
        /// </summary>
        /// <param name="oldPivotPoint">
        /// This is the old pivot point before it was changed.
        /// </param>
        /// <param name="newPivotPoint">
        /// This is the new pivot point.
        /// </param>
        public static void SendToInterestedListeners(TransformPivotPoint oldPivotPoint, TransformPivotPoint newPivotPoint)
        {
            var message = new TransformPivotPointChangedMessage(oldPivotPoint, newPivotPoint);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when the type of the active gizmo is changed (e.g. when switching
    /// from a translation to a rotation gizmo).
    /// </summary>
    public class ActiveGizmoTypeChangedMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// This is the old active gizmo type before it was changed.
        /// </summary>
        private GizmoType _oldActiveGizmoType;

        /// <summary>
        /// This is the new active gizmo type.
        /// </summary>
        private GizmoType _newActiveGizmoType;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the old active gizmo type before it was changed.
        /// </summary>
        public GizmoType OldActiveGizmoType { get { return _oldActiveGizmoType; } }

        /// <summary>
        /// Returns the new active gizmo type.
        /// </summary>
        public GizmoType NewActiveGizmoType { get { return _newActiveGizmoType; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="oldActiveGizmoType">
        /// This is the old active gizmo type before it was changed.
        /// </param>
        /// <param name="newActiveGizmoType">
        /// This is the new active gizmo type.
        /// </param>
        public ActiveGizmoTypeChangedMessage(GizmoType oldActiveGizmoType, GizmoType newActiveGizmoType)
            : base(MessageType.ActiveGizmoTypeChanged)
        {
            _oldActiveGizmoType = oldActiveGizmoType;
            _newActiveGizmoType = newActiveGizmoType;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending an active gizmo type changed message to 
        /// all interested listeners.
        /// </summary>
        /// <param name="oldActiveGizmoType">
        /// This is the old active gizmo type before it was changed.
        /// </param>
        /// <param name="newActiveGizmoType">
        /// This is the new active gizmo type.
        /// </param>
        public static void SendToInterestedListeners(GizmoType oldActiveGizmoType, GizmoType newActiveGizmoType)
        {
            var message = new ActiveGizmoTypeChangedMessage(oldActiveGizmoType, newActiveGizmoType);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when a gizmo transform operation is undone.
    /// </summary>
    public class GizmoTransformOperationWasUndoneMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// This is the gizmo which is involved in the transform operation which was undone.
        /// </summary>
        private Gizmo _gizmoInvolvedInTransformOperation;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the gizmo which is involved in the transform operation which was undone.
        /// </summary>
        public Gizmo GizmoInvolvedInTransformOperation { get { return _gizmoInvolvedInTransformOperation; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gizmoInvolvedInTransformOperation">
        /// This is the gizmo which is involved in the transform operation which was undone.
        /// </param>
        public GizmoTransformOperationWasUndoneMessage(Gizmo gizmoInvolvedInTransformOperation)
            : base(MessageType.GizmoTransformOperationWasUndone)
        {
            _gizmoInvolvedInTransformOperation = gizmoInvolvedInTransformOperation;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a gizmo transform operation undone message to
        /// all interested listeners.
        /// </summary>
        /// <param name="gizmoInvolvedInTransformOperation">
        /// This is the gizmo which is involved in the transform operation which was undone.
        /// </param>
        public static void SendToInterestedListeners(Gizmo gizmoInvolvedInTransformOperation)
        {
            var message = new GizmoTransformOperationWasUndoneMessage(gizmoInvolvedInTransformOperation);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when a gizmo transform operation is redone.
    /// </summary>
    public class GizmoTransformOperationWasRedoneMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// This is the gizmo which is involved in the transform operation which was redone.
        /// </summary>
        private Gizmo _gizmoInvolvedInTransformOperation;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the gizmo which is involved in the transform operation which was redone.
        /// </summary>
        public Gizmo GizmoInvolvedInTransformOperation { get { return _gizmoInvolvedInTransformOperation; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gizmoInvolvedInTransformOperation">
        /// This is the gizmo which is involved in the transform operation which was redone.
        /// </param>
        public GizmoTransformOperationWasRedoneMessage(Gizmo gizmoInvolvedInTransformOperation)
            : base(MessageType.GizmoTransformOperationWasRedone)
        {
            _gizmoInvolvedInTransformOperation = gizmoInvolvedInTransformOperation;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a gizmo transform operation redone message to
        /// all interested listeners.
        /// </summary>
        /// <param name="gizmoInvolvedInTransformOperation">
        /// This is the gizmo which is involved in the transform operation which was redone.
        /// </param>
        public static void SendToInterestedListeners(Gizmo gizmoInvolvedInTransformOperation)
        {
            var message = new GizmoTransformOperationWasRedoneMessage(gizmoInvolvedInTransformOperation);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when vertex snapping is enabled.
    /// </summary>
    public class VertexSnappingEnabledMessage : Message
    {
        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public VertexSnappingEnabledMessage()
            : base(MessageType.VertexSnappingEnabled)
        {
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a vertex snapping enabled message to
        /// all interested listeners.
        /// </summary>
        public static void SendToInterestedListeners()
        {
            var message = new VertexSnappingEnabledMessage();
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when vertex snapping is disabled.
    /// </summary>
    public class VertexSnappingDisabledMessage : Message
    {
        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public VertexSnappingDisabledMessage()
            : base(MessageType.VertexSnappingDisabled)
        {
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a vertex snapping disabled message to
        /// all interested listeners.
        /// </summary>
        public static void SendToInterestedListeners()
        {
            var message = new VertexSnappingDisabledMessage();
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }
}