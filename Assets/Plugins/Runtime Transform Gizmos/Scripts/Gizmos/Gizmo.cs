using System;
using UnityEngine;
using System.Collections.Generic;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.InputModule;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTEditor
{
    /// <summary>
    /// This class acts as a base class for all gizmo types. It holds properties which
    /// are common for all gizmo types and common methods that are used by some of the 
    /// derived gizmo classes.
    /// </summary>
    public abstract class Gizmo : MonoBehaviour, IHighlightable
    {
        #region Events
        public event Action OnHighlight;
        public event Action OnUnHighlight;

        public delegate void GizmoDragStart(Gizmo gizmo);
        public delegate void GizmoDragUpdate(Gizmo gizmo);
        public delegate void GizmoDragEnd(Gizmo gizmo);

        public event GizmoDragStart OnGizmoDragStart;
        public event GizmoDragUpdate OnGizmoDragUpdate;
        public event GizmoDragEnd OnGizmoDragEnd;
        #endregion

        #region Protected Variables
        /// <summary>
        /// This collection is used to hold objects which can not be transformed by the gizmo.
        /// </summary>
        protected HashSet<GameObject> _maskedObjects = new HashSet<GameObject>();

        /// <summary>
        /// Serves the same purpose as '_maskedObjects', but it operates on object layers. A bit
        /// with a value of 1 indicates a masked object layer.
        /// </summary>
        protected int _maskedObjectLayers = 0;

        /// <summary>
        /// This is the camera that will be used to render the gizmo at runtime. We will
        /// set this in the 'Start' method to point to the 'EditorCamera' singleton instance.
        /// </summary>
        [Dependency, SerializeField] protected Camera _camera;

        public Camera Camera
        {
            set { _camera = value; }
        }

        /// <summary>
        /// Every gizmo will have 3 axes associated with it: X, Y and Z. This is an array
        /// which describes the color used to draw each axis. The word 'axes' can mean 
        /// different things depending on the type of gizmo that we are dealing with. For
        /// example, for the translation and scale gizmos, the axes are the 3 lines which
        /// are perpendicualr to each other. For a rotation gizmo, the axes are actually 
        /// the 3 circles which allow the user to perform rotations.
        /// </summary>
        [SerializeField]
        protected Color[] _axesColors = new Color[] { DefaultXAxisColor, DefaultYAxisColor, DefaultZAxisColor };

        /// <summary>
        /// When the user hovers one of the axes, that axis will be marked as selected. This
        /// is the color that must be used to draw an axis when it is selected.
        /// </summary>
        [SerializeField]
        protected Color _selectedAxisColor = DefaultSelectedAxisColor;

        /// <summary>
        /// This represents the gizmo scale on all 3 axes. If '_preserveGizmoScreenSize' is set to true,
        /// the real scale of the gizmo will be set to a scaled version of this value. Otherwise, this
        /// value directly controls the size of the gizmo.
        /// </summary>
        [SerializeField]
        protected float _gizmoBaseScale = 0.77f;

        /// <summary>
        /// When this variable is set to true, the gizmo's scale will always be adjusted such that it
        /// maintains roughly the same screen size regardless of its distance from the camera. This is 
        /// available for both orthographic and perspective cameras. When the variable is set to false, 
        /// the scale of the gizmo will be set to '_gizmoBaseScale'.
        /// </summary>
        [SerializeField]
        protected bool _preserveGizmoScreenSize = true;

        /// <summary>
        /// Cached gizmo transform for easy access.
        /// </summary>
        protected Transform _gizmoTransform;

        /// <summary>
        /// We will need this to get access to mouse button states and cursor move offsets.
        /// </summary>
        protected Mouse _mouse = new Mouse();

        /// <summary>
        /// Holds the currently selected gizmo axis. A selected gizmo axis is the axis which is being hovered 
        /// by the mouse cursor. The selected axis can be different things based on the type of gizmo we are 
        /// dealing with. For example, for a translation gizmo, the axis is made up of an axis line and the 
        /// arrow cone which sits at the end of it. For a rotation gizmo, an axis is actually the circle which
        /// allows the user to perform a rotation around a certain axis.
        /// </summary>
        private GizmoAxis _selectedAxis = GizmoAxis.None;

        /// <summary>
        /// This value holds the intersection point between the gizmo and the mouse cursor in different 
        /// types of situations. This value is used differently based on the type of gizmo that needs it.
        /// For example, this value will be used when implementing the functionality of the translation 
        /// gizmo because it wil allow us to calculate the translation amount that needs to be applied to 
        /// the controlled objects.
        /// </summary>
        protected Vector3 _lastGizmoPickPoint;

        /// <summary>
        /// It is sometimes necessary to use the stencil buffer to avoid having the axis lines draw over
        /// other gizmo parts. For example, when drawing the translation gizmo axis lines, some of those 
        /// lines may draw over the axis cones when the cones are facing the camera. Using these stencil
        /// values we can make sure that this does not happen. The array holds stencil values for each 
        /// axis. For example, when the X axis cone object will be rendered, its shader will write the 
        /// first value in this array inside the stencil buffer. When the X axis line will be drawn, its 
        /// stencil reference value will be set to the one used to render the cone. Pixels inside the axis 
        /// line whose corresponding stencil entries have been set to the same value, will be discarded.
        /// </summary>
        /// <remarks>
        /// If you use these stencil reference values in other areas of your application (i.e. for rendering  
        /// other objects), make sure to modify these values to something else. Otherwise, you might get
        /// incorrect rendering results.
        /// </remarks>
        protected int[] _axesStencilRefValues = new int[] { 252, 253, 254 };

        /// <summary>
        /// Sometimes, we want to let the lines draw over other gizmo parts. Following the same example
        /// with the translation gizmo, when the X axis cone faces away from the camera, its axis line
        /// will be drawn over its bottom cap. In that case we want to disable the stencil buffer and 
        /// we will instruct the line rendering shader to use this reference value which will make sure 
        /// that no pixels will be discarded since no gizmo parts will ever write this value inside the
        /// stencil buffer. 
        /// </summary>
        /// <remarks>
        /// If you use this stencil reference value in other areas of your application (i.e. for rendering  
        /// other objects), make sure to modify this value to something else.  Otherwise, you might get
        /// incorrect rendering results.
        /// </remarks>
        protected int _doNotUseStencil = 255;

        /// <summary>
        /// The user is allowed to choose the transform pivot point that is used when transforming
        /// objects and the pivot point is stored in this variable.
        /// </summary>
        protected TransformPivotPoint _transformPivotPoint = TransformPivotPoint.Center;

        /// <summary>
        /// The following 3 variables allow us to execute a post gizmo transformed objects action
        /// after the left mouse button is released. For example, when the user presses, the left
        /// mouse button, '_preTransformObjectSnapshots' will be adjusted to contain the snapshots
        /// of all objects that can be transformed by the gizmo. When the left mouse button is released,
        /// if '_objectsWereTransformedSinceLeftMouseButtonWasPressed' is true, snapshots will be
        /// taken again for the objects' tramsforms and the 2 snapshot collections will be used to
        /// execute a post gizmo transformed game objects action.
        /// </summary>
        protected List<ObjectTransformSnapshot> _preTransformObjectSnapshots;
        protected List<ObjectTransformSnapshot> _postTransformObjectSnapshots;
        protected bool _objectsWereTransformedSinceLeftMouseButtonWasPressed;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the minimum value that the gizmo base scale can have.
        /// </summary>
        public static float MinGizmoBaseScale { get { return 0.01f; } }
        public static Color DefaultXAxisColor { get { return new Color(219.0f / 255.0f, 62.0f / 255.0f, 29.0f / 255.0f, 1.0f); } }
        public static Color DefaultYAxisColor { get { return new Color(154.0f / 255.0f, 243.0f / 255.0f, 72.0f / 255.0f, 1.0f); } }
        public static Color DefaultZAxisColor { get { return new Color(58.0f / 255.0f, 122.0f / 255.0f, 248.0f / 255.0f, 1.0f);} }
        public static Color DefaultSelectedAxisColor { get { return new Color(246.0f / 255.0f, 242.0f / 255.0f, 50.0f / 255.0f, 1.0f); } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the selected axis color.
        /// </summary>
        public Color SelectedAxisColor { get { return _selectedAxisColor; } set { _selectedAxisColor = value; } }

        /// <summary>
        /// Gets/sets the gizmo base scale. The minimum value that the gizmo base scale can have is
        /// given by the 'MinGizmoBaseScale' property. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float GizmoBaseScale
        {
            get { return _gizmoBaseScale; }
            set
            {
                // Set the base scale and make sure that the gizmo's transform scale is adjusted accordingly
                _gizmoBaseScale = Mathf.Max(MinGizmoBaseScale, value);
                AdjustGizmoScale();
            }
        }

        /// <summary>
        /// Lets the client code specify/know whether or not the gizmo should be scaled automatically
        /// so that it will remain roughly same size regardless of the distance from the camera.
        /// </summary>
        public bool PreserveGizmoScreenSize { get { return _preserveGizmoScreenSize; } set { _preserveGizmoScreenSize = value; } }

        /// <summary>
        /// Gets/sets the transform pivot point.
        /// </summary>
        public TransformPivotPoint TransformPivotPoint { get { return _transformPivotPoint; } set { _transformPivotPoint = value; } }

        /// <summary>
        /// Gets/sets the list of objects which are controlled by the gizmo. When setting this property,
        /// no copy will be made of the specified collection. The gizmo object will maintain a direct
        /// reference to the specified collection.
        /// </summary>
        public IEnumerable<GameObject> ControlledObjects { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the color for the specified gizmo axis. If 'GizmoAxis.None' is
        /// specified, black will be returned.
        /// </summary>
        public Color GetAxisColor(GizmoAxis axis)
        {
            if (axis == GizmoAxis.None) return Color.black;
            return _axesColors[(int)axis];
        }

        /// <summary>
        /// Sets the color for the specified gizmo axis. If 'GizmoAxis.None' is
        /// specified, the method will have no effect.
        /// </summary>
        public void SetAxisColor(GizmoAxis axis, Color color)
        {
            if (axis == GizmoAxis.None) return;
            _axesColors[(int)axis] = color;
        }

        /// <summary>
        /// Masks the specified object. Masked objects can not be manipulated by the gizmo.
        /// </summary>
        public void MaskObject(GameObject gameObject)
        {
            _maskedObjects.Add(gameObject);
        }

        /// <summary>
        /// Unmasks the specified object.
        /// </summary>
        public void UnmaskObject(GameObject gameObject)
        {
            _maskedObjects.Remove(gameObject);
        }

        /// <summary>
        /// Masks the specified object collection. Masked objects can not be manipulated by the gizmo.
        /// </summary>
        public void MaskObjectCollection(IEnumerable<GameObject> objectCollection)
        {
            foreach(GameObject gameObject in objectCollection)
            {
                MaskObject(gameObject);
            }
        }

        /// <summary>
        /// Unmasks the specified object collection.
        /// </summary>
        public void UnmaskObjectCollection(IEnumerable<GameObject> objectCollection)
        {
            foreach (GameObject gameObject in objectCollection)
            {
                UnmaskObject(gameObject);
            }
        }

        /// <summary>
        /// Returns true if the specified object is masked.
        /// </summary>
        public bool IsGameObjectMasked(GameObject gameObject)
        {
            return _maskedObjects.Contains(gameObject);
        }

        /// <summary>
        /// Masks the specified object layer. Objects which belong to masked layers can not be 
        /// manipulated by the gizmo.
        /// </summary>
        public void MaskObjectLayer(int objectLayer)
        {
            _maskedObjectLayers = LayerHelper.SetLayerBit(_maskedObjectLayers, objectLayer);
        }

        /// <summary>
        /// Unmasks the specified object layer.
        /// </summary>
        public void UnmaskObjectkLayer(int objectLayer)
        {
            _maskedObjectLayers = LayerHelper.ClearLayerBit(_maskedObjectLayers, objectLayer);
        }

        /// <summary>
        /// Can be used to check if the specified layer is masked. Objects which belong to masked layers
        /// can not be manipulated by the gizmo.
        /// </summary>
        public bool IsObjectLayerMasked(int objectLayer)
        {
            return LayerHelper.IsLayerBitSet(_maskedObjectLayers, objectLayer);
        }

        /// <summary>
        /// Can be used to check if a certain game object can be manipulated by the gizmo.
        /// </summary>
        public bool CanObjectBeManipulated(GameObject gameObject)
        {
            return !IsGameObjectMasked(gameObject) && !IsObjectLayerMasked(gameObject.layer);
        }

        /// <summary>
        /// Can be used to check if all controlled objects can be manipulated by the gizmo.
        /// </summary>
        public bool CanAllControlledObjectsBeManipulated()
        {
            if (ControlledObjects != null)
            {
                foreach(GameObject gameObject in ControlledObjects)
                {
                    if (!CanObjectBeManipulated(gameObject)) return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Can be used to check if at least one controlled object can be manipulated by the gizmo.
        /// </summary>
        public bool CanAnyControlledObjectBeManipulated()
        {
            if (ControlledObjects != null)
            {
                foreach (GameObject gameObject in ControlledObjects)
                {
                    if (CanObjectBeManipulated(gameObject)) return true;
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Returns a list of all objects which can be manipulated.
        /// </summary>
        public List<GameObject> GetControlledObjectsWhichCanBeManipulated()
        {
            if (ControlledObjects == null) return new List<GameObject>();

            List<GameObject> objectWhichCanBeManipulated = new List<GameObject>(ControlledObjects);
            objectWhichCanBeManipulated.RemoveAll(item => !CanObjectBeManipulated(item));

            return objectWhichCanBeManipulated;
        }
        #endregion

        #region Public Abstract Methods
        /// <summary>
        /// Checks if the gizmo is ready to manipulate game objects. This method must be implemented
        /// by all derived gizmo objects.
        /// </summary>
        public abstract bool IsReadyForObjectManipulation();

        /// <summary>
        /// This method can be used to check if the gizmo is being used to transform game objects.
        /// </summary>
        public bool IsTransformingObjects()
        {
            return IsReadyForObjectManipulation() && _mouse.IsLeftMouseButtonDown;
        }

        /// <summary>
        /// Returns the gizmo's type. Must be implemented by all derived classes.
        /// </summary>
        public abstract GizmoType GetGizmoType();
        #endregion

        #region Protected Methods
        /// <summary>
        /// Performs any necessary initializations.
        /// </summary>
        protected virtual void Start()
        {
            _gizmoTransform = transform;

            // Make sure that the line rendering material doesn't use the stencil by default. This is necessary
            // because not all gizmo types will be updating this value and if we don't set it here, we might get
            // incorrect rendering results. For example, the rotation gizmo doesn't use this value and if we don't
            // initialize it here, the rotation circle lines will be rendered incorrectly.
            MaterialPool.Instance.GizmoLine.SetInt("_StencilRefValue", _doNotUseStencil);
        }

        /// <summary>
        /// Performs any necesary updates for the current frame.
        /// </summary>
        protected virtual void Update()
        {
            // Update the mouse data for the current frame update
            _mouse.UpdateInfoForCurrentFrame();

            // Throw any necessary mouse button input events
            if (_mouse.WasLeftMouseButtonPressedInCurrentFrame) OnLeftMouseButtonDown();
            if (_mouse.WasLeftMouseButtonReleasedInCurrentFrame) OnLeftMouseButtonUp();

            // If the mouse has moved, call 'OnMouseMoved' to let the derived class instance handle the mouse movement.
            // In order to check if the mouse has moved, we will check if the magnitude of the mouse cursor move offset
            // since the last frame is greater than 0.
            if (_mouse.CursorOffsetSinceLastFrame.magnitude > 0.0f) OnMouseMoved();

            // We always need to make sure that the gizmo scale is set up properly. This is necessary especially when
            // the '_preserveGizmoScreenSize' variable is set to true because then we need to make sure that the gizmo
            // has roughly the same size regardless of its distance from the camera position.
            AdjustGizmoScale();
        }

        public GizmoAxis SelectedAxis {
            get { return _selectedAxis; }
        }

        protected void SelectAxis(GizmoAxis axis) {
            if (axis != _selectedAxis) {
                _selectedAxis = axis;
                if (axis != GizmoAxis.None) {
                    if (OnHighlight != null) {
                        OnHighlight();
                    }
                } else {
                    if (OnUnHighlight != null) {
                        OnUnHighlight();
                    }   
                }
            }
        }

        /// <summary>
        /// Performs any necessary adjustments on the gizmo scale. This method is necessary due to
        /// the presence of the '_preserveGizmoScreenSize' variable. When this is set to true, the
        /// gizmo has to always have its scale adjusted such that it has roughly the same size 
        /// regardless of its distance from the camera position. 
        /// </summary>
        protected void AdjustGizmoScale()
        {
            // Note: This code can be called from the Unity Editor when setting the gizmo base scale via
            //       the Inspector GUI. When operating in the editor, the camera and its transform will
            //       not have been initialized and that is why we need to check for null here.
            if (_camera != null)
            {
                // Calculate the scale that the gizmo should have and then change the gizmo's transform scale
                float gizmoScale = CalculateGizmoScale();
                this.transform.localScale = new Vector3(gizmoScale, gizmoScale, gizmoScale);
            }
        }

        /// <summary>
        /// This method can be called to calculate the scale the gizmo must have based on its
        /// properties and its distance from the camera position. For example, when the 
        /// '_preserveGizmoScreenSize' variable is true, this method will calculate the scale
        /// in such a way that the gizmo will always be roughly the same size regardless of its
        /// distance from the camera position.
        /// </summary>
        protected float CalculateGizmoScale()
        {
            // If the screen size must not be preserved, we can just return the gizmo base scale
            if (!_preserveGizmoScreenSize || _camera == null) return _gizmoBaseScale;

            // Note: The following code uses some scale constants which were setup so that acceptable results
            //       are obtained for the default value of '_gizmoBaseScale'. These values were determined
            //       by pure experiment.
            if (_camera.orthographic)
            {
                // If an orthographic camera is used we will scale the '_gizmoBaseScale' variable
                // by the ratio between the camera orthographic size and the viewport's height.
                float scaleConstant = 0.02f;
                return _gizmoBaseScale * _camera.orthographicSize / (_camera.pixelRect.height * scaleConstant);
            }
            else
            {
                // For a perspective camera, we will calculate the gizmo scale by scaling '_gizmoBaseScale' by 
                // the ratio between the gizmo distance from the camera to the viewport's height. This seems to 
                // produce a good enough scaling ratio.
                const float scaleConstant = 0.045f;
                Vector3 cameraPositionToGizmoOrigin = (this.transform.position - _camera.transform.position);
                return _gizmoBaseScale * cameraPositionToGizmoOrigin.magnitude / (_camera.pixelRect.height * scaleConstant);
            }
        }

        /// <summary>
        /// We will need to draw a set of perpendicular axes for the translation and scale gizmos.
        /// The problem is that depending on how the gizmos are rotated, some axis lines may obscure
        /// other axis lines when in fact they should be behind. This happens if we draw the gizmo 
        /// axis lines in the same order each frame. The method analyzes the relationship between the
        /// camera look vector and the gizmo axis lines and returns an array of 3 indices which control
        /// the order in which these lines should be drawn without any unwanted effects. For example,
        /// if the returned index array contains the values {2, 0, 1}, it means that the axes should
        /// be drawn in the following order: Z, X, Y.
        /// </summary>
        protected int[] GetSortedGizmoAxesIndices()
        {
            // Sort the gizmo axis lines based on their relationship with the camera.
            // Note: The reason we need to do this is that sometimes, depending on how the gizmo is rotated, we will
            //       get some unwanted effects like an axis line being drawn over another axis line when in fact it should
            //       be behind it. This is not very pleasant. In order to eliminate this effect, we will identify the gizmo
            //       axis that could potentially produce such an effect and we will draw it first. Doing this, elimimates
            //       the danger of having this axis overlapping another axis because all the other axes will be drawn over it.
            Vector3[] gizmoLocalAxes = GetGizmoLocalAxes();
            int indexOfMostAlignedAxis = 0;
            float bestDotProduct = Vector3.Dot(gizmoLocalAxes[0], _camera.transform.forward);        // Assume that the X axis must be drawn first
            int[] axisIndices = new int[] { 0, 1, 2 };
            for (int axisIndex = 1; axisIndex < 3; ++axisIndex)
            {
                // The axis that we are looking for is the axis which has its axis vector pointing in the same
                // direction as the camera look vector. That axis should be behind all others, but if we don't
                // perform any sorting, it may be drawn on top of another axis which sits in front of it. So,
                // we wil perform the dot product between the axis and the camera look vector. If the dot product
                // is smaller than 0, it means the axis is pointing at the camera and there is no danger present.
                // However, if the dot product is greater than 0, we will store the dot product result if it is
                // bigger than the one we have found so far. The best dot product result is the one which is closest
                // to 1.0f since that is when an axis is most aligned with the camera look vector.
                float dotProduct = Vector3.Dot(gizmoLocalAxes[axisIndex], _camera.transform.forward);
                if (dotProduct < 0.0f) continue;

                // Is this dot product result better?
                if (dotProduct > bestDotProduct)
                {
                    bestDotProduct = dotProduct;
                    indexOfMostAlignedAxis = axisIndex;

                    // If the dot product is really close to 1.0f, there is no need to go any further
                    if (Mathf.Abs(1.0f - bestDotProduct) < 1e-4f) break;
                }
            }

            // Now we have to rearrange the axis indices such that the most aligned axis is rendered first
            axisIndices[0] = indexOfMostAlignedAxis;
            axisIndices[indexOfMostAlignedAxis] = 0;

            // Return the axis indices
            return axisIndices;
        }

        /// <summary>
        /// For some types of gizmos (translation and scale gizmos) we will need to make sure that the axis lines
        /// do not overdraw the object that sits at the tip of the axes (cone for translation gizmo and box for the
        /// scale gizmo). This function will make sure that the stencil reference values inside the line rendering
        /// shader are updated accordingly in order to achieve the desired result.
        /// </summary>
        /// <param name="axisIndex">
        /// This is the index of the axis which must be drawn.
        /// </param>
        /// <param name="startPoint">
        /// The start point of the axis.
        /// </param>
        /// <param name="endPoint">
        /// The end point of the axis.
        /// </param>
        /// <param name="gizmoScale">
        /// The gizmo scale (can be retrieved via a call to 'CalculateGizmoScale'.
        /// </param>
        protected void UpdateShaderStencilRefValuesForGizmoAxisLineDraw(int axisIndex, Vector3 startPoint, Vector3 endPoint, float gizmoScale)
        {
            // We will need the normalized axis direction vector and the axis length
            Vector3 axisDirectionVector = endPoint - startPoint;
            float axisLength = axisDirectionVector.magnitude;
            axisDirectionVector.Normalize();

            // We will use the stencil buffer of the line's material to make sure that the line will not overlap whatever it is at the
            // tip of the axis (arrow cone, scale box etc). For example, when the axis is pointing at the camera, the lines will overlap 
            // the cone/box (from now on we will be referring to a cone) in screen space and it will be drawn over it because the Z tests 
            // are turned of in the line rendering shader. If this is the case, we will set the stencil reference value to the reference 
            // value which corresponds to the current axis we are drawing. 
            // However, if the bottom cap of the cone is visible from the camera's position, it means the line will overlap the bottom
            // cap and in that case we want to turn the stencil off so that all line pixels can be drawn.
            // In order to check if the stencil should be used or not, we will cast a ray from the tip of the axis line (right where the 
            // axis cone sits only just a little bit behind to make sure that we don't always get positive results with a t value of 0) 
            // to the camera position. If this ray intersects the bottom cap plane, it means the bottom cap is invisible to the camera 
            // and the line might overlap the cone in screen space in which case we activate the stencil buffer. Otherwise, if the ray 
            // does not intersect the bottom cap plane, we will deactivate the stencil buffer and that will make sure the entire line will 
            // be drawn.
            // Note: The ray direction is calculated differently based on the type of camera we are dealing with. For a perspective camera,
            //       the ray direction is a vector which goes from the ray origin to the camera position. For an orthographic camera, the
            //       direction vector is the inverse camera look vector. This difference exists because of the way in which each camera type
            //       shapes the view volume.
            Vector3 rayOrigin = startPoint + axisDirectionVector * 0.97f * axisLength;
            Vector3 rayDirection = _camera.orthographic ? - _camera.transform.forward : _camera.transform.position - rayOrigin;
            Ray rayAimingAtCameraPosition = new Ray(rayOrigin, rayDirection);

            // For a translation gizmo this is the bottom cap plane of the arrow cone. For a scale gizmo, this is the scale box's face which is perpendicular to the axis.
            Plane axisCapPlane = new Plane(axisDirectionVector, endPoint);

            // Activate or deactivate the stencil buffer
            float t;
            if (axisCapPlane.Raycast(rayAimingAtCameraPosition, out t)) MaterialPool.Instance.GizmoLine.SetInt("_StencilRefValue", _axesStencilRefValues[axisIndex]);
            else MaterialPool.Instance.GizmoLine.SetInt("_StencilRefValue", _doNotUseStencil);
        }

        /// <summary>
        /// Returns an array which holds the gizmo local axes.
        /// </summary>
        protected Vector3[] GetGizmoLocalAxes()
        {
            return new Vector3[] { _gizmoTransform.right, _gizmoTransform.up, _gizmoTransform.forward };
        }

        /// <summary>
        /// When implementing the translation and scale gizmos, we need this method to return
        /// to us the plane which contains the currently selected gizmo axis. The method uses
        /// the 'GetAxisPlaneNormalMostAlignedWithCameraLook' in order to choose the best plane
        /// with regards to the current camera orientation.
        /// </summary>
        protected Plane GetCoordinateSystemPlaneFromSelectedAxis()
        {
            switch (_selectedAxis)
            {
                case GizmoAxis.X:

                    return new Plane(GetAxisPlaneNormalMostAlignedWithCameraLook(GizmoAxis.X), _gizmoTransform.position);

                case GizmoAxis.Y:

                    return new Plane(GetAxisPlaneNormalMostAlignedWithCameraLook(GizmoAxis.Y), _gizmoTransform.position);

                case GizmoAxis.Z:

                    return new Plane(GetAxisPlaneNormalMostAlignedWithCameraLook(GizmoAxis.Z), _gizmoTransform.position);

                default:

                    return new Plane();
            }
        }

        public InputRange InputRange { get; set; }
        
        /// <summary>
        /// Given a specified axis, the method will return its corresponding plane normal which is most
        /// aligned with the camera look vector. This is necessary when performing a translation or scale 
        /// along a certain gizmo axis. Normally, we could just use a single plane for each axis (e.g. for 
        /// the X axis we could always use the XY plane), but depending on the camera rotation, this can 
        /// produce some unwanted effects. The method allows us to eliminate them by choosing the plane
        /// normal most aligned with the camera look vector.
        /// </summary>
        protected Vector3 GetAxisPlaneNormalMostAlignedWithCameraLook(GizmoAxis gizmoAxis)
        {
            float bestAbsDotProduct = 0.0f;
            Vector3 bestPlaneNormal = _gizmoTransform.forward;

            // Each axis has a set of 2 possible plane normals that can be used. These are:
            //  a) X axis (XY and XZ plane normals);
            //  b) Y axis (XY and YZ plane normals);
            //  c) Z axis (YZ and XZ plane normals).
            Vector3[] planeNormals;
            if (gizmoAxis == GizmoAxis.X) planeNormals = new Vector3[] { _gizmoTransform.forward, _gizmoTransform.up };
            else if (gizmoAxis == GizmoAxis.Y) planeNormals = new Vector3[] { _gizmoTransform.forward, _gizmoTransform.right };
            else planeNormals = new Vector3[] { _gizmoTransform.right, _gizmoTransform.up };

            // Now loop through all possible plane normals for the specified input axis and choose the
            // one which is most aligned with the camera look vector. 
            for (int normalIndex = 0; normalIndex < 2; ++normalIndex)
            {
                // The most aligned axis is the one whose dot product with the camera look vector is
                // the closest to 1. We will use the absolute value of the dot product since the direction
                // doesn't matter.
                float absDotProduct = Mathf.Abs(Vector3.Dot(_camera.transform.forward, planeNormals[normalIndex]));
                if (absDotProduct > bestAbsDotProduct)
                {
                    bestAbsDotProduct = absDotProduct;
                    bestPlaneNormal = planeNormals[normalIndex];
                }
            }

            return bestPlaneNormal;
        }

        /// <summary>
        /// The translation and scale gizmos contain multi-axis components that allow the user to perform transformations
        /// on 2 axes at once. This will return a float array that will contain a series of sign values that can be used to
        /// scale the gizmo's local axes in such a way that it will allow for the correct positioning of the multi-axis
        /// components. 
        /// </summary>
        /// <param name="adjustForBetterVisibility">
        /// This parameter specifies whether or not the multi-axis components must be adjusted for better viibility.
        /// </param>
        protected float[] GetMultiAxisExtensionSigns(bool adjustForBetterVisibility)
        {
            // We need to make sure that we take the sign of the gizmo scale into account since that
            // has an influence on the direction in which the components are extending.
            // Note: This is not really necessary because the gizmo base scale will always be clamped
            //       to a positive value. But we will still perform this step for completeness.
            float gizmoScaleSign = Mathf.Sign(CalculateGizmoScale());

            // If 'adjustForBetterVisibility' is true, we have to return a sign array which takes 
            // into consideration the relationship between the camera look vector and the gizmo local axes.
            // Otherwise, we just return a series of 1.0f signs which are multiplied by the sign of the
            // gizmo scale.
            if (adjustForBetterVisibility)
            {
                // We will establish the sign values based on the dot product values between the
                // camera look vector and each of the gizmo's local vectors. 
                Vector3 cameraLookVector = _camera.transform.forward;
                float dotProductRight = Vector3.Dot(cameraLookVector, _gizmoTransform.right);
                float dotProductUp = Vector3.Dot(cameraLookVector, _gizmoTransform.up);
                float dotProductForward = Vector3.Dot(cameraLookVector, _gizmoTransform.forward);

                // Construct the array and return it.
                // Note: The sign is -1 or 1 depending on the sign of the dot product and we also multiply by the sign of 
                //       the gizmo scale because the scale (if negative) reverses the extension direction of the multi-axis 
                //       components. -1 is used when the camera forward vector points in the same direction as the gizmo axis
                //       because in that case, the standard position of the components would make them become more 'hidden' 
                //       (harder to reach).
                return new float[]
                {
                    dotProductRight > 0.0f ? -1.0f * gizmoScaleSign : 1.0f * gizmoScaleSign,
                    dotProductUp > 0.0f ? -1.0f * gizmoScaleSign : 1.0f * gizmoScaleSign, 
                    dotProductForward > 0.0f ? -1.0f * gizmoScaleSign : 1.0f * gizmoScaleSign
                };
            }
            else return new float[] { 1.0f * gizmoScaleSign, 1.0f * gizmoScaleSign, 1.0f * gizmoScaleSign };
        }

        /// <summary>
        /// Derived classes will need to destroy and recreate certain meshes when some gizmo properties
        /// are changed. This function is used to destroy a specified gizmo mesh. For example, this method
        /// will be called when the user changes the cone radius or length for a translation gizmo.
        /// </summary>
        protected void DestroyGizmoMesh(Mesh gizmoMesh)
        {
            if (gizmoMesh == null) return;

            // When inside the editor, we will use 'DestroyImmediate'. Otherwise, we use 'Destroy'.
            if (Application.isEditor && Application.isPlaying) DestroyImmediate(gizmoMesh);
            else
            if (!Application.isEditor && Application.isPlaying) Destroy(gizmoMesh);
        }

        /// <summary>
        /// When the collection of controlled objects must be transformed by the gizmo, we will
        /// only apply the transformation to those objects that don't have a parent which resides
        /// inside the controlled object collection. If we don't do this, results are not very
        /// pleasing because we would apply the same transformation twice to the children: once
        /// by transforming the child itself, and a second time, by transforming its parent. This
        /// function returns a list with all the top parents (objects that don't have a parent 
        /// inside the controlled object collection) that reside in the controlled object collection.
        /// </summary>
        /// <remarks>
        /// If the controlled objects collection hasn't been setup properly, the method will return 
        /// an empty list of objects.
        /// </remarks>
        protected List<GameObject> GetParentsFromControlledObjects(bool filterOnlyCanBeManipulated)
        {
            // When the controlled game object collection hasn't been setup properly, return an empty list
            if (ControlledObjects == null) return new List<GameObject>();

            if(!filterOnlyCanBeManipulated) return GameObjectExtensions.GetParentsFromObjectCollection(ControlledObjects);
            else
            {
                List<GameObject> objectsWhichCanBeManipulated = GetControlledObjectsWhichCanBeManipulated();
                return GameObjectExtensions.GetParentsFromObjectCollection(objectsWhichCanBeManipulated);
            }
        }

        /// <summary>
        /// Called whenever the left mouse button is pressed. Derived classes will
        /// provide their own implementation but they will always call the base class
        /// version of the method to adjust any necessary states.
        /// </summary>
        protected virtual void OnLeftMouseButtonDown()
        {
            // When the left mouse button is pressed, we will take a snapshot of all game objects
            // which can be transformed by the gizmo. This will allow us to execute a post gizmo
            // transformed game objects action if necessary when the left mouse button is released.
            TakeObjectTransformSnapshots(out _preTransformObjectSnapshots);

            if (OnGizmoDragStart != null && IsReadyForObjectManipulation()) OnGizmoDragStart(this);
        }

        /// <summary>
        /// Called whenever the left mouse button si released. Derived classes will
        /// provide their own implementation but they will always call the base class
        /// version of the method to adjust any necessary states.
        /// </summary>
        protected virtual void OnLeftMouseButtonUp()
        {
            // If the objects were transformed since the left mouse button was pressed, we will
            // take a snapshot of all game objects which were transformed and then use the pre
            // and post transform snapshots to execute a post gizmo transformed objects action.
            if (_objectsWereTransformedSinceLeftMouseButtonWasPressed)
            {
                // Create post transform snapshots
                //TakeObjectTransformSnapshots(out _postTransformObjectSnapshots);

                // Execute a post gizmo transformed objects action
//                var action = new PostGizmoTransformedObjectsAction(_preTransformObjectSnapshots, _postTransformObjectSnapshots, this);
//                action.Execute();

                // Reset for the next transform session
                _objectsWereTransformedSinceLeftMouseButtonWasPressed = false;
            }

            if (OnGizmoDragEnd != null && IsReadyForObjectManipulation()) OnGizmoDragEnd(this);
        }

        /// <summary>
        /// Called whenever the mouse is moved. Derived classes will provide their own 
        /// implementation but they will always call the base class version of the method 
        /// to adjust any necessary states.
        /// </summary>
        protected virtual void OnMouseMoved()
        {
            if (OnGizmoDragUpdate != null && _mouse.IsLeftMouseButtonDown && IsReadyForObjectManipulation()) OnGizmoDragUpdate(this);
        }

        /// <summary>
        /// Called after the camera has finished rendering the scene. Derived classes will also 
        /// provide their own implementation but they will always call the base class version of 
        /// the method to adjust any necessary states.
        /// </summary>
        protected virtual void OnRenderObject()
        {
            //if (Camera.current != EditorCamera.Instance.Camera) return;

            // In the derived classes we will usually perform any necessary line drawing in this method. 
            // Depending on how the gizmo objects are activated and deactivated in the scene, we will get 
            // some unwanted effects when a gizmo is activated, but its 'Update' method was not called. If
            // only 'OnRenderObject' is called, the lines will not be drawn properly because the scale of
            // the gizmo is not up to date. This results in the gizmo object being rendered at a different
            // scale for a frame which yields an unwanted visual effect. You can actually see the gizmo being
            // snapped to the correct scale. Calling this method here, allows us to be sure that the gizmos 
            // are rendered properly.
            AdjustGizmoScale();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This method can be called to take a snapshot of all game objects which can be transformed
        /// and store all snapshots in the specified output list. If no objects can be transformed, the
        /// method will set the output list to null.
        /// </summary>
        private void TakeObjectTransformSnapshots(out List<ObjectTransformSnapshot> objectTransformSnapshots)
        {
            objectTransformSnapshots = null;

            // Retrieve all game objects which can be transformed. If no such object exists, we will exit the method.
            List<GameObject> objectWhichCanBeTransformed = GetParentsFromControlledObjects(true);
            if (objectWhichCanBeTransformed.Count == 0) return;

            // Create the list which can be used to hold the snapshot data
            objectTransformSnapshots = new List<ObjectTransformSnapshot>(objectWhichCanBeTransformed.Count);

            // Loop through all game objects which can be transformed
            foreach(GameObject gameObject in objectWhichCanBeTransformed)
            {
                // Create a snapshot for the current object
                var objectTransformSnapshot = new ObjectTransformSnapshot();
                objectTransformSnapshot.TakeSnapshot(gameObject);

                // Add the snapshot to the snapshot list
                objectTransformSnapshots.Add(objectTransformSnapshot);
            }
        }
        #endregion


    }
}
