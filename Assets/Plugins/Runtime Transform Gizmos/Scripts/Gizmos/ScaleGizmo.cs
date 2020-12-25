//using UnityEngine;
//using System.Collections.Generic;
//using RamjetAnvil.InputModule;
//
//namespace RTEditor
//{
//    /// <summary>
//    /// This class implements the behaviour of a scale gizmo. A scale gizmo is composed of the following:
//    ///     a) 3 axes. When the user clicks on one of these axes and moves the mouse, they will perform a
//    ///        scale operation along that axis. It is important to note that the objects will always be
//    ///        scaled along their local axes;
//    ///     b) 3 boxes which sit at the end of the each of the 3 axes. Like the axes, these boxes can also
//    ///        be clicked and dragged to perform a scale operation;
//    ///     c) 3 multi-axis triangles which sit between each pair of 2 axis. Clicking on one of these triangles
//    ///        and then moving the mouse will scale along 2 axes simultaneously;
//    ///     d) an all-axes square which sits at the center of the gizmo in screen space. Clicking on this square
//    ///        and then moving the mouse, the user can perform a scale along all axes simultaneously. 
//    /// </summary>
//    /// <remarks>
//    /// The arrays which hold the gizmo multi-axis properties store the data in the following format:
//    ///     -[0] -> XY multi-axis;
//    ///     -[1] -> XZ multi-axis;
//    ///     -[2] -> YZ multi-axis.
//    /// </remarks>
//    public class ScaleGizmo : Gizmo
//    {
//        #region Private Variables
//        /// <summary>
//        /// This represents the length of a scale axis. All axes will share the same length.
//        /// </summary>
//        [SerializeField]
//        private float _axisLength = 5.0f;
//
//        /// <summary>
//        /// The following 3 variables represent the dimensions if the scale boxes which
//        /// sit at the tip of each of the 3 scale axes.
//        /// </summary>
//        [SerializeField]
//        private float _scaleBoxWidth = 0.5f;
//        [SerializeField]
//        private float _scaleBoxHeight = 0.5f;
//        [SerializeField]
//        private float _scaleBoxDepth = 0.5f;
//
//        /// <summary>
//        /// The client code can set this value to true when a scale along all axes at once
//        /// is needed. When this is the case, a square will be drawn at the center of the 
//        /// gizmo. Clicking this square and then moving the mouse around, the user can scale 
//        /// along all axes at once.
//        /// </summary>
//        [SerializeField]
//        private bool _scaleAlongAllAxes;
//
//        /// <summary>
//        /// When '_scaleAlongAllAxes' is true, this variable will be used to draw a square at the 
//        /// center of the gizmo which allows the user to scale along all axes simultaneously.
//        /// It represents the length of the square sides in screen units (both width and height).
//        /// </summary>
//        [SerializeField]
//        private float _screenSizeOfAllAxesSquare = 25.0f;
//
//        /// <summary>
//        /// This is the color that is used to draw the lines that make up the square used to scale
//        /// along all axes at once.
//        /// </summary>
//        [SerializeField]
//        private Color _colorOfAllAxesSquareLines = Color.white;
//
//        /// <summary>
//        /// Same as '_colorOfAllAxesSquareLines', but it applies when the square is selected.
//        /// </summary>
//        [SerializeField]
//        private Color _colorOfAllAxesSquareLinesWhenSelected = Color.yellow;
//
//        /// <summary>
//        /// If this variable is set to true, the all-axes scale square will have its size adjusted while
//        /// the user is performing an all-axes scale operation.
//        /// </summary>
//        [SerializeField]
//        private bool _adjustAllAxesScaleSquareWhileScalingObjects = true;
//
//        /// <summary>
//        /// When using the all-axes square to perform a scale along all axes, we will use the mouse cursor
//        /// offset to calculate the accumulated scale drag value. This variable holds the number of drag
//        /// units which correspond to one screen unit.
//        /// </summary>
//        private const float _allAxesSquareDragUnitsPerScreenUnit = 0.45f;
//
//        /// <summary>
//        /// Specifies whether or not the scale boxes must be lit. A different material
//        /// will be used to render the scale boxes based on the value of this variable.
//        /// </summary>
//        [SerializeField]
//        private bool _areScaleBoxesLit = true;
//
//        /// <summary>
//        /// If this variable is set to true, the scale axis will have their length adjusted while
//        /// the user is performing a scale operation (i.e. while they are dragging the scale axis
//        /// or its selection box).
//        /// </summary>
//        [SerializeField]
//        private bool _adjustAxisLengthWhileScalingObjects = true;
//
//        /// <summary>
//        /// Array which holds colors for each of the 3 multi-axis triangles. These
//        /// are the colors used to draw the multi-axis triangle meshes.
//        /// </summary>
//        [SerializeField]
//        private Color[] _multiAxisTriangleColors = new Color[3] 
//        {
//            new Color(DefaultXAxisColor.r, DefaultXAxisColor.g, DefaultXAxisColor.b, 0.2f),
//            new Color(DefaultYAxisColor.r, DefaultYAxisColor.g, DefaultYAxisColor.b, 0.2f),
//            new Color(DefaultZAxisColor.r, DefaultZAxisColor.g, DefaultZAxisColor.b, 0.2f)
//        };
//
//        /// <summary>
//        /// Array which holds colors used to draw the multi-axis lines that surround the 
//        /// multi-axis triangles.
//        /// </summary>
//        [SerializeField]
//        private Color[] _multiAxisTriangleLineColors = new Color[]
//        {
//            DefaultXAxisColor, DefaultYAxisColor, DefaultZAxisColor
//        };
//
//        /// <summary>
//        /// This is the color that is used to draw the selected multi-axis triangle.
//        /// </summary>
//        [SerializeField]
//        private Color _selectedMultiAxisTriangleColor = new Color(DefaultSelectedAxisColor.r, DefaultYAxisColor.g, DefaultYAxisColor.b, 0.2f);
//
//        /// <summary>
//        /// This is the color of the selected multi-axis triangle lines. These are the lines
//        /// that surround the triangles.
//        /// </summary>
//        [SerializeField]
//        private Color _selectedMultiAxisTriangleLineColor = DefaultSelectedAxisColor;
//
//        /// <summary>
//        /// This is the length that is used for both adjacent sides of the multi-axis triangles.
//        /// </summary>
//        [SerializeField]
//        private float _multiAxisTriangleSideLength = 1.3f;
//
//        /// <summary>
//        /// If this is set to true, the multi-axis triangles will be positioned in such a way that they
//        /// will always be visible to the user for easy manipulation. Otherwise, the multi-axis triangles
//        /// will always be positioned in the following manner: XY multi-axis -> sits at the corner which
//        /// is formed by the X and Y axes; XZ multi-axis-> sits at the corner which is formed by the X
//        /// and Z axes; YZ multi-axis -> sits at the corner which is formed by the Y and Z axes.
//        /// </summary>
//        [SerializeField]
//        private bool _adjustMultiAxisForBetterVisibility = true;
//
//        /// <summary>
//        /// This variable allows us to apply scale to the controlled objects. When the user
//        /// click one of the scale axes and then starts moving the mouse, this variable 
//        /// will be updated accordingly based on the mouse movement.
//        /// </summary>
//        private float _accumulatedScaleAxisDrag;
//
//        /// <summary>
//        /// This serves the same purpose as '_accumulatedScaleAxisDrag' does, but it applies to
//        /// the multi-axis triangles. 
//        /// </summary>
//        private float _accumulatedMultiAxisTriangleDrag;
//
//        /// <summary>
//        /// This serves the same purpose as '_accumulatedScaleAxisDrag' does, but it applies to
//        /// the all-axes square which can be used to scale along all axes at once. 
//        /// </summary>
//        /// <remarks>
//        /// This value is expressed in screen units.
//        /// </remarks>
//        private float _accumulatedAllAxesSquareDragInScreenUnits;
//
//        /// <summary>
//        /// This serves the same purpose as '_accumulatedScaleAxisDrag' does, but it applies to
//        /// the all-axes square which can be used to scale along all axes at once. 
//        /// </summary>
//        /// <remarks>
//        /// This value is expressed in world units. The purpose of this variable is to allow us
//        /// to handle snapping correctly when using the all-axes square.
//        /// </remarks>
//        private float _accumulatedAllAxesSquareDragInWorldUnits;
//
//        /// <summary>
//        /// If this variable is set to true, the multi-axis triangles will have their area adjusted while
//        /// the user is performing a scale operation (i.e. while they are dragging the triangle to perform
//        /// a scale operation).
//        /// </summary>
//        [SerializeField]
//        private bool _adjustMultiAxisTrianglesWhileScalingObjects = true;
//
//        /// <summary>
//        /// If this variable is set to true, the gizmo will draw the local coordinate
//        /// system axes of each object which is affected by a scale operation.
//        /// </summary>
//        [SerializeField]
//        private bool _drawObjectsLocalAxesWhileScaling = true;
//
//        /// <summary>
//        /// When '_drawObjectsLocalAxesWhileScaling' is true, this variable controls the length of the
//        /// objects' local coordinate system axes during a scale operation.
//        /// </summary>
//        [SerializeField]
//        private float _objectsLocalAxesLength = 1.0f;
//
//        /// <summary>
//        /// This is similar to the '_preserveGizmoScreenSize' declared in the base gizmo clas, but it applies
//        /// to the objects local axes when they need to be drawn.
//        /// </summary>
//        [SerializeField]
//        private bool _preserveObjectLocalAxesScreenSize = true;
//
//        /// <summary>
//        /// If this is set to true, the objects' local axes will be scaled along with the objects during a
//        /// scale operation.
//        /// </summary>
//        [SerializeField]
//        private bool _adjustObjectLocalAxesWhileScalingObjects = false;
//
//        /// <summary>
//        /// Holds the scale gizmo's snap settings.
//        /// </summary>
//        [SerializeField]
//        private ScaleGizmoSnapSettings _snapSettings = new ScaleGizmoSnapSettings();
//
//        /// <summary>
//        /// This dictionary is necessary because of the way in which the scale is applied to the objects. 
//        /// When the user moves the mouse around, the '_accumulatedScaleAxisDrag' will be updated accordingly. 
//        /// The scaling of the objects is performed by scaling the objects' local scale by a ratio which
//        /// involves the scale axis length and the '_accumulatedScaleAxisDrag' variable. This scale ratio 
//        /// is applied to the local scale that each object had when the user started the scale operation. 
//        /// For example, if an object's scale on the X axis was set to 1.0f at the time the scale operation 
//        /// was started and the calculated scale ratio is 2.0f, the new scale of the object will be set to
//        /// 1.0f * 2.0f = 2.0f and so on. The important thing to rememebr is that the scale ratio will be applied 
//        /// relative to the scale that the objects had at the time the scale operation is started. Otherwise, 
//        /// as we continue to drag the scale axis, the applied scale will increase exponentially. The dictionary
//        /// maps a game object instance to its local scale that is available when the scale operation starts.
//        /// </summary>
//        //private Dictionary<GameObject, Vector3> _gameObjectLocalScaleSnapshot = new Dictionary<GameObject, Vector3>();
//
//        private Vector3 _initialScale;
//
//        /// <summary>
//        /// The currently selected multi-axis triangle.
//        /// </summary>
//        private MultiAxisTriangle _selectedMultiAxisTriangle = MultiAxisTriangle.None;
//
//        /// <summary>
//        /// This will be set to true when the square which allows the user to scale along all axes is selected.
//        /// </summary>
//        private bool _isAllAxesSquareSelected;
//        #endregion
//
//        #region Public Static Properties
//        /// <summary>
//        /// Returns the minimum length that an axis can have.
//        /// </summary>
//        /// <remarks>
//        /// I recommend that you leave this value set to 0.1f. Really small axis values are not
//        /// desirable because when a scale operation needs to be performed, the axis length will
//        /// be involved in the calculations and really small values can cause the scale operation
//        /// to become very hard to control.
//        /// </remarks>
//        public static float MinAxisLength { get { return 0.1f; } }
//
//        /// <summary>
//        /// Returns the minimum size (width, height or depth) that a scale box can have.
//        /// </summary>
//        public static float MinScaleBoxSize { get { return 0.1f; } }
//
//        /// <summary>
//        /// Returns the minimum size (width and height) that the all axes scale square can have in screen space.
//        /// </summary>
//        public static float MinScreenSizeOfAllAxesSquare { get { return 2.0f; } }
//
//        /// <summary>
//        /// Returns the minimum value for the multi-axis triangle side length.
//        /// </summary>
//        public static float MinMultiAxisTriangleSideLength { get { return 0.001f; } }
//
//        /// <summary>
//        /// Returns the minimum value for the length of the object local axes. These are the local axes
//        /// that are drawn for the objects which are involved in a scale operation.
//        /// </summary>
//        public static float MinObjectsLocalAxesLength { get { return 0.1f; } }
//        #endregion
//
//        #region Public Properties
//        /// <summary>
//        /// Gets/sets the axis length. The minimum value that the length can have is given by the 'MinAxisLength'
//        /// property. Values smaller than that will be clamped accordingly.
//        /// </summary>
//        public float AxisLength { get { return _axisLength; } set { _axisLength = Mathf.Max(MinAxisLength, value); } }
//
//        /// <summary>
//        /// Gets/sets the scale box width. The minimum value for the scale box width is given by the 'MinScaleBoxSize'
//        /// property. Values smaller than that will be clamped accordingly.
//        /// </summary>
//        public float ScaleBoxWidth
//        {
//            get { return _scaleBoxWidth; }
//            set { _scaleBoxWidth = Mathf.Max(value, MinScaleBoxSize); }
//        }
//
//        /// <summary>
//        /// Gets/sets the scale box height. The minimum value for the scale box height is given by the 'MinScaleBoxSize'
//        /// property. Values smaller than that will be clamped accordingly.
//        /// </summary>
//        public float ScaleBoxHeight
//        {
//            get { return _scaleBoxHeight; }
//            set { _scaleBoxHeight = Mathf.Max(value, MinScaleBoxSize); }
//        }
//
//        /// <summary>
//        /// Gets/sets the scale box depth. The minimum value for the scale box depth is given by the 'MinScaleBoxSize'
//        /// property. Values smaller than that will be clamped accordingly.
//        /// </summary>
//        public float ScaleBoxDepth
//        {
//            get { return _scaleBoxDepth; }
//            set { _scaleBoxDepth = Mathf.Max(value, MinScaleBoxSize); }
//        }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the scale boxes must be lit.
//        /// </summary>
//        public bool AreScaleBoxesLit { get { return _areScaleBoxesLit; } set { _areScaleBoxesLit = value; } }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the gizmo is supposed to scale
//        /// objects along all axes at once.
//        /// </summary>
//        public bool ScaleAlongAllAxes
//        {
//            get { return _scaleAlongAllAxes; }
//            set
//            {
//                // It is possible that when the client code wants to enable the all-axes scale functionality,
//                // the user is already using the gizmo to transform objects. In that case we deny the request.
//                if (value == true && IsTransformingObjects()) return;
//                _scaleAlongAllAxes = value;
//            }
//        }
//
//
//        /// <summary>
//        /// Gets/sets the screen size (width and height) of the square which is visible when performing an 
//        /// all-axes scale operation. The minimum value for the size is given by the 'MinScreenSizeOfAllAxesSquare'.
//        /// Values smaller than that will be clamped accordingly.
//        /// </summary>
//        public float ScreenSizeOfAllAxesSquare { get { return _screenSizeOfAllAxesSquare; } set { _screenSizeOfAllAxesSquare = Mathf.Max(MinScreenSizeOfAllAxesSquare, value); } }
//
//        /// <summary>
//        /// Gets/sets the color of the all-axes scale square.
//        /// </summary>
//        public Color ColorOfAllAxesSquareLines { get { return _colorOfAllAxesSquareLines; } set { _colorOfAllAxesSquareLines = value; } }
//
//        /// <summary>
//        /// Gets/sets the color of the all-axes scale square when selected.
//        /// </summary>
//        public Color ColorOfAllAxesSquareLinesWhenSelected { get { return _colorOfAllAxesSquareLinesWhenSelected; } set { _colorOfAllAxesSquareLinesWhenSelected = value; } }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the all-axes scale square must have its
//        /// size adjusted while scaling objects.
//        /// </summary>
//        public bool AdjustAllAxesScaleSquareWhileScalingObjects { get { return _adjustAllAxesScaleSquareWhileScalingObjects; } set { _adjustAllAxesScaleSquareWhileScalingObjects = value; } }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the gizmo scale axes must have their
//        /// size adjusted while scaling objects.
//        /// </summary>
//        public bool AdjustAxisLengthWhileScalingObjects { get { return _adjustAxisLengthWhileScalingObjects; } set { _adjustAxisLengthWhileScalingObjects = value; } }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the multi-axis scale triangles must have
//        /// their size adjusted while scaling objects.
//        /// </summary>
//        public bool AdjustMultiAxisTrianglesWhileScalingObjects { get { return _adjustMultiAxisTrianglesWhileScalingObjects; } set { _adjustMultiAxisTrianglesWhileScalingObjects = value; } }
//
//        /// <summary>
//        /// Gets/sets the color for the multi-axis triangles when they are selected.
//        /// </summary>
//        public Color SelectedMultiAxisTriangleColor { get { return _selectedMultiAxisTriangleColor; } set { _selectedMultiAxisTriangleColor = value; } }
//
//        /// <summary>
//        /// Gets/sets the color for the multi-axis triangle lines when they are selected.
//        /// </summary>
//        public Color SelectedMultiAxisTriangleLineColor { get { return _selectedMultiAxisTriangleLineColor; } set { _selectedMultiAxisTriangleLineColor = value; } }
//
//        /// <summary>
//        /// Gets/sets the length of the side of the multi-axis triangles. The minimum value that the triangle
//        /// side can have is given by the 'MinMultiAxisTriangleSideLength' property. Values smaller than that
//        /// will be clamped accordingly.
//        /// </summary>
//        public float MultiAxisTriangleSideLength
//        {
//            get { return _multiAxisTriangleSideLength; }
//            set { _multiAxisTriangleSideLength = Mathf.Max(MinMultiAxisTriangleSideLength, value); }
//        }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the multi-axis triangles must have their positions adjusted
//        /// for better visibility.
//        /// </summary>
//        public bool AdjustMultiAxisForBetterVisibility { get { return _adjustMultiAxisForBetterVisibility; } set { _adjustMultiAxisForBetterVisibility = value; } }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the object local axes must be drawn during a scale operation
//        /// for all objects which are being scaled.
//        /// </summary>
//        public bool DrawObjectsLocalAxesWhileScaling { get { return _drawObjectsLocalAxesWhileScaling; } set { _drawObjectsLocalAxesWhileScaling = value; } }
//
//        /// <summary>
//        /// Gets/sets the length of the object local axes during a scale operation. The minimum value for the object local axis length
//        /// is given by the 'MinObjectsLocalAxesLength' property. Values smaller than that will be clamped accordingly.
//        /// </summary>
//        public float ObjectsLocalAxesLength { get { return _objectsLocalAxesLength; } set { _objectsLocalAxesLength = Mathf.Max(MinObjectsLocalAxesLength, value); } }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the size of the object local axes must be preserved during
//        /// a scale operation.
//        /// </summary>
//        public bool PreserveObjectLocalAxesScreenSize { get { return _preserveObjectLocalAxesScreenSize; } set { _preserveObjectLocalAxesScreenSize = value; } }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the object local axes must have their length adjusted during
//        /// a scale operation.
//        /// </summary>
//        public bool AdjustObjectLocalAxesWhileScalingObjects { get { return _adjustObjectLocalAxesWhileScalingObjects; } set { _adjustObjectLocalAxesWhileScalingObjects = value; } }
//
//        /// <summary>
//        /// Returns the scale snap settings associated with the gizmo.
//        /// </summary>
//        public ScaleGizmoSnapSettings SnapSettings { get { return _snapSettings; } }
//        #endregion
//
//        #region Public Methods
//        /// <summary>
//        /// Checks if the gizmo is ready for object manipulation.
//        /// </summary>
//        public override bool IsReadyForObjectManipulation()
//        {
//            // Return true if one of the following is true:
//            //  a) one of the scale axes; (i.e. _selectedAxis != GizmoAxis.None)
//            //  b) one of the scale boxes; (i.e. _selectedAxis != GizmoAxis.None)
//            //  c) one of the multi-axis triangles; (i.e. _selectedMultiAxisTriangle != MultiAxisTriangle.None)
//            //  d) the all-axes square. (i.e. _isAllAxesSquareSelected)
//            return SelectedAxis != GizmoAxis.None || _selectedMultiAxisTriangle != MultiAxisTriangle.None || _isAllAxesSquareSelected;
//        }
//
//        /// <summary>
//        /// Returns the gizmo type.
//        /// </summary>
//        public override GizmoType GetGizmoType()
//        {
//            return GizmoType.Scale;
//        }
//
//        /// <summary>
//        /// Returns the color of the specified multi-axis triangle. If 'multiAxisTriangle' is
//        /// set to 'MultiAxisTriangle.None', the method will return the color black.
//        /// </summary>
//        public Color GetMultiAxisTriangleColor(MultiAxisTriangle multiAxisTriangle)
//        {
//            if (multiAxisTriangle == MultiAxisTriangle.None) return Color.black;
//            return _multiAxisTriangleColors[(int)multiAxisTriangle];
//        }
//
//        /// <summary>
//        /// Sets the color of the specified multi-axis triangle. If 'multiAxisTriangle' is
//        /// set to 'MultiAxisTriangle.None', the method will have no effect.
//        /// </summary>
//        public void SetMultiAxisTriangleColor(MultiAxisTriangle multiAxisTriangle, Color color)
//        {
//            if (multiAxisTriangle == MultiAxisTriangle.None) return;
//            _multiAxisTriangleColors[(int)multiAxisTriangle] = color;
//        }
//
//        /// <summary>
//        /// Returns the color of the lines which surround the specified multi-axis triangle. 
//        /// If 'multiAxisTriangle' is set to 'MultiAxisTriangle.None', the method will return 
//        /// the color black.
//        /// </summary>
//        public Color GetMultiAxisTriangleLineColor(MultiAxisTriangle multiAxisTriangle)
//        {
//            if (multiAxisTriangle == MultiAxisTriangle.None) return Color.black;
//            return _multiAxisTriangleLineColors[(int)multiAxisTriangle];
//        }
//
//        /// <summary>
//        /// Sets the color of the lines which surround the specified multi-axis triangle. 
//        /// If 'multiAxisTriangle' is set to 'MultiAxisTriangle.None', the method will have
//        /// no effect.
//        /// </summary>
//        public void SetMultiAxisTriangleLineColor(MultiAxisTriangle multiAxisTriangle, Color color)
//        {
//            if (multiAxisTriangle == MultiAxisTriangle.None) return;
//            _multiAxisTriangleLineColors[(int)multiAxisTriangle] = color;
//        }
//        #endregion
//
//        #region Protected Methods
//        /// <summary>
//        /// Performs any necessary initializations.
//        /// </summary>
//        protected override void Start()
//        {
//            base.Start();
//        }
//
//        /// <summary>
//        /// Called every frame to perform any necessary updates. The main purpose of this
//        /// method is to identify the currently selected gizmo components.
//        /// </summary>
//        protected override void Update()
//        {
//            base.Update();
//
//            // If the left mouse button is down, we don't want to update the selections
//            // because the user may be moving the mouse around in order to perform a
//            // scale operating and we don't want to deselect any axes while that happens.
//            if (_mouse.IsLeftMouseButtonDown) return;
//
//            // Reset selection information. We will be updating these in the code which follows.
//            var selectedAxis = GizmoAxis.None;
//            _selectedMultiAxisTriangle = MultiAxisTriangle.None;
//            _isAllAxesSquareSelected = false;
//
//            // Cache needed variables
//            float minimumDistanceFromCamera = float.MaxValue;
//            Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//            float gizmoScale = CalculateGizmoScale();
//            float cylinderRadius = 0.2f * gizmoScale;           // We will need this to check the intersection between the ray and the axis lines which we will treat as really thin cylinders.
//            Vector3 cameraPosition = _camera.transform.position;
//            Vector3 gizmoPosition = _gizmoTransform.position;
//
//            // Loop through all gizmo axis lines and identify the one which is picked by the 
//            // mouse cursor with the closest pick point to the camera position.
//            float t;
//            Vector3[] gizmoLocalAxes = GetGizmoLocalAxes();
//            Vector3 firstCylinderPoint = gizmoPosition;
//            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
//            {
//                // We will check the intersection between the mouse cursor and the axis line by checking
//                // if the ray generated by the mouse cursor intersects the line's imaginary cylinder.
//                bool axisWasPicked = false;
//                Vector3 secondCylinderPoint = gizmoPosition + gizmoLocalAxes[axisIndex] * _axisLength * gizmoScale;
//                if (pickRay.IntersectsCylinder(firstCylinderPoint, secondCylinderPoint, cylinderRadius, out t))
//                {
//                    // Calculate the intersection point and check if it is closer to the camera than what we have so far
//                    Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//                    float distanceFromCamera = (intersectionPoint - cameraPosition).magnitude;
//                    if (distanceFromCamera < minimumDistanceFromCamera)
//                    {
//                        // This intersection point is closer, so update the selection
//                        minimumDistanceFromCamera = distanceFromCamera;
//                        selectedAxis = (GizmoAxis)axisIndex;
//
//                        // This axis was picked, so we don't need to check the intersection between the ray and the axis scale box. See the next 'if' statement.
//                        axisWasPicked = true;
//                    }
//                }
//
//                // We will also check if the ray intersects the axis' scale box.
//                // Note: We only do this if the corresponding scale axis hasn't been selected. If it was, there is no need
//                //       to perform this test anymore.
//                Matrix4x4[] scaleBoxWorldTransforms = GetScaleBoxesWorldTransforms();
//                if (!axisWasPicked && pickRay.IntersectsBox(1.0f, 1.0f, 1.0f, scaleBoxWorldTransforms[axisIndex], out t))
//                {
//                    // Calculate the intersection point and check if it is closer to the camera than what we have so far
//                    Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//                    float distanceFromCamera = (intersectionPoint - cameraPosition).magnitude;
//                    if (distanceFromCamera < minimumDistanceFromCamera)
//                    {
//                        // This intersection point is closer, so update the selection
//                        minimumDistanceFromCamera = distanceFromCamera;
//                        selectedAxis = (GizmoAxis)axisIndex;
//                    }
//                }
//            }
//
//            // Now we will check if any of the multi-axis triangles are selected by the mouse cursor.
//            // Note: We only perform this check if the all-axes square is not active.
//            if (!_scaleAlongAllAxes)
//            {
//                // Loop through each multi-axis triangle
//                for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//                {
//                    // Retrieve the triangle's world space points
//                    Vector3[] triangleWorldSpaceVertices = GetMultiAxisTriangleWorldSpacePoints(multiAxisIndex);
//
//                    // We will need to construct a plane from the triangle points, but for that we need the triangle
//                    // vertices defined in clockwise winding order when looking down on the triangle surface. This
//                    // info can be found here: http://docs.unity3d.com/ScriptReference/Plane-ctor.html
//                    // We will also need this ordering when checking if the intersection point between the mouse cursor  
//                    // and the triangle plane lies inside the triangle.
//                    Matrix4x4[] multiAxisTrianglesWorldTransforms = GetMultiAxisTrianglesWorldTransforms();
//                    ReorderTriangleVertsForClockwiseWindingOrder(triangleWorldSpaceVertices, multiAxisTrianglesWorldTransforms[multiAxisIndex]);
//                    Plane triangleWorldSpacePlane = new Plane(triangleWorldSpaceVertices[0], triangleWorldSpaceVertices[1], triangleWorldSpaceVertices[2]);
//
//                    // Check if the mouse cursor intersects the triangle plane
//                    if (triangleWorldSpacePlane.Raycast(pickRay, out t))
//                    {
//                        // The ray intersects the plane but we have to check if the intersection point
//                        // lies inside the triangle.
//                        Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//                        if (intersectionPoint.IsInsideTriangle(triangleWorldSpaceVertices))
//                        {
//                            // The point lies inside the triangle. Now we have to check if the intersection point
//                            // is closer than what we have so far.
//                            float distanceFromCamera = (intersectionPoint - cameraPosition).magnitude;
//                            if (distanceFromCamera < minimumDistanceFromCamera)
//                            {
//                                // This intersection point is closer, so update the selection
//                                minimumDistanceFromCamera = distanceFromCamera;
//                                _selectedMultiAxisTriangle = (MultiAxisTriangle)multiAxisIndex;
//
//                                // If a multi-axis triangle was selected, it means that the selection axis which
//                                // was previously selected (if any) is not selected anymore.
//                                selectedAxis = GizmoAxis.None;
//                            }
//                        }
//                    }
//                }
//            }
//
//            // Check if the all-axes scale square is selected. We only do this if the square is active.
//            if (_scaleAlongAllAxes && IsMouseCursorInsideAllAxesScaleSquare())
//            {
//                // The square is selected
//                _isAllAxesSquareSelected = true;
//
//                // We will disable any other selection when the square is selected
//                selectedAxis = GizmoAxis.None;
//                _selectedMultiAxisTriangle = MultiAxisTriangle.None;
//            }
//
//            SelectAxis(selectedAxis);
//        }
//
//        /// <summary>
//        /// Called after the camera has finished rendering the scene and it allows
//        /// us to perform any necessary drawing.
//        /// </summary>
//        protected override void OnRenderObject()
//        {
//            //if (Camera.current != EditorCamera.Instance.Camera) return;
//            base.OnRenderObject();
//
//            var activeAxes = InputRange.ActiveAxes;
//
//            // Draw the scale boxes
//            Matrix4x4[] scaleBoxWorldTransforms = GetScaleBoxesWorldTransforms();
//            DrawScaleBoxes(activeAxes, scaleBoxWorldTransforms);
//
//            // Draw the multi-axis triangles.
//            // Note: We don't draw the triangles if the all-axes square is active.
//            //Matrix4x4[] multiAxisTrianglesWorldTransforms = GetMultiAxisTrianglesWorldTransforms();
//            //if (!_scaleAlongAllAxes) DrawMultiAxisTriangles(multiAxisTrianglesWorldTransforms);
//
//            // Draw the scale axes lines
//            DrawAxesLines(activeAxes);
//
//            // Draw the lines which surround the multi-axis triangles
//            MaterialPool.Instance.GizmoLine.SetInt("_StencilRefValue", _doNotUseStencil);
//            //DrawMultiAxisTrianglesLines();
//
//            // Draw the all-axes square lines
//            //DrawAllAxesSquareLines();
//
//            // Draw the objects coordinate system axes
//            //DrawObjectsLocalAxesDuringScaleOperation();
//        }
//
//        /// <summary>
//        /// Called whenever the left mouse button is pressed. The method is responsible for
//        /// checking which components of the gizmo were picked and perform any additional
//        /// actions like storing data which is needed while processing mouse move events.
//        /// </summary>
//        protected override void OnLeftMouseButtonDown()
//        {
//            base.OnLeftMouseButtonDown();
//
//            // If there something selected, it means the user was hovering one of the scale 
//            // gizmo's components when they pressed the left mouse button. In that case
//            // we want to store any necessary information that is needed when processing
//            // the next mouse move event.
//            if (SelectedAxis != GizmoAxis.None || _selectedMultiAxisTriangle != MultiAxisTriangle.None)
//            {
//                // For the next mouse move event we will need to have access to the point which
//                // lies on the plane that contains the currently selected component.
//                // Note: We ignore the all-axes square component because we will use the mouse movmenet
//                //       offset to control the scale in that case.
//                Plane pickPlane;
//                if (SelectedAxis != GizmoAxis.None) pickPlane = GetCoordinateSystemPlaneFromSelectedAxis();
//                else pickPlane = GetMultiAxisTrianglePlane(_selectedMultiAxisTriangle);
//
//                // Construct a ray using the mouse cursor position
//                Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//
//                // Now calculate the intersection point with the plane and store it inside '_lastGizmoPickPoint'.
//                // We will use '_lastGizmoPickPoint' inside the 'OnMouseMoved' method to help us calculate
//                // the amount of accumulated scale axis drag.
//                float t;
//                if (pickPlane.Raycast(pickRay, out t)) _lastGizmoPickPoint = pickRay.origin + pickRay.direction * t;
//            }
//
//            // Whenever the left mouse button is pressed, we have to prepare for a scale operation if
//            // the gizmo is selected. So, we will make sure that the local scale snapshot is updated
//            // so that it contains the local scale of all controlled objects.
//            if (IsReadyForObjectManipulation()) {
//                _initialScale = InputRange.UnitValue;
////                foreach (GameObject gameObject in ControlledObjects) {
////                    var uiRange = gameObject.GetComponent<AbstractUiRange>();
////                    if (uiRange != null) {
////                        _uiRange = uiRange;
////                        _initialScale = _uiRange.InputRange.Value;
////                    }
////                }
//            }
//        }
//
//        /// <summary>
//        /// Called when the left mouse button is released.
//        /// </summary>
//        protected override void OnLeftMouseButtonUp()
//        {
//            base.OnLeftMouseButtonUp();
//
//            // Whenever the left mouse button is released, we will reset the accumulated scale 
//            // axis drag back to 0 so that the gizmo axes can be rendered normally. We do the
//            // same for the accumulated multi-axis triangle and all-axes square drag.
//            _accumulatedScaleAxisDrag = 0.0f;
//            _accumulatedMultiAxisTriangleDrag = 0.0f;
//            _accumulatedAllAxesSquareDragInScreenUnits = 0.0f;
//            _accumulatedAllAxesSquareDragInWorldUnits = 0.0f;
//
//            // The data stored in the local scale snapshot is no longer needed when the left mouse button
//            // is released. It will always be populated when the left mouse buton is pressed.
//            //_uiRange = null;
//        }
//
//        /// <summary>
//        /// Called when the mouse is moved. The main responsibility of this method is to
//        /// make sure that any necessary scale is applied to the controlled objects.
//        /// </summary>
//        protected override void OnMouseMoved()
//        {
//            base.OnMouseMoved();
//
//            //if (!CanAnyControlledObjectBeManipulated()) return;
//
//            // If the left mouse button is down, we will perform a scale operation if something is selected
//            if (_mouse.IsLeftMouseButtonDown)
//            {
//                // Is there a scale axis/box selected?
//                if (SelectedAxis != GizmoAxis.None)
//                {
//                    // Identify the scale axis vector based on the currently selected scale axis
//                    Vector3 scaleAxis;
//                    if (SelectedAxis == GizmoAxis.X) scaleAxis = _gizmoTransform.right;
//                    else if (SelectedAxis == GizmoAxis.Y) scaleAxis = _gizmoTransform.up;
//                    else scaleAxis = _gizmoTransform.forward;
//
//                    // Retrieve the plane that contains the selected axis and construct a ray using the current mouse cursor position
//                    Plane coordinateSystemPlane = GetCoordinateSystemPlaneFromSelectedAxis();
//                    Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//
//                    // If the ray intersects the plane, we have some work to do
//                    float t;
//                    if (coordinateSystemPlane.Raycast(pickRay, out t))
//                    {
//                        // The ray intersects the plane. In order to perform a scale operation, we will calculate a vector
//                        // which goes from the last gizmo pick point to the current intersection point. Projecting the
//                        // resulting vector on the gizmo scale axis, we get the amount that we need to add to the scale axis drag.
//                        Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//                        Vector3 offsetVector = intersectionPoint - _lastGizmoPickPoint;
//                        float projectionOnScaleAxis = Vector3.Dot(offsetVector, scaleAxis);
//
//                        // Adjust the accumulated scale axis drag value. 
//                        // Note: Depending on the direction of movement along the scale axis, 'projectionOnScaleAxis' can be either
//                        //       positive or negative. Positive values will stretch the objects and negative values will shrink them.
//                        _accumulatedScaleAxisDrag += projectionOnScaleAxis;
//
//                        // Scale the controlled objects along the selected axis
//                        bool[] axisScaleBooleanFlags = new bool[] { false, false, false };
//                        axisScaleBooleanFlags[(int)SelectedAxis] = true;
//                        ScaleControlledObjects(axisScaleBooleanFlags);
//
//                        // Store the gizmo pick point for the next mouse move event
//                        _lastGizmoPickPoint = intersectionPoint;
//                    }
//                }
//                else
//                // Is there any multi-axis triangle selected?
//                if (_selectedMultiAxisTriangle != MultiAxisTriangle.None)
//                {
//                    // Retrieve the plane of the selected multi-axis triangle
//                    Plane trianglePlane = GetMultiAxisTrianglePlane(_selectedMultiAxisTriangle);
//
//                    // Construct a ray using the current mouse cursor position
//                    Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//
//                    // Check if the ray intersects the triangle plane
//                    float t;
//                    if (trianglePlane.Raycast(pickRay, out t))
//                    {
//                        // The ray intersects the plane. In order to perform a scale operation, we will calculate a vector
//                        // which goes from the last gizmo pick point to the current intersection point. Projecting the
//                        // resulting vector on the triangle median vector, we get the amount that we need to add to the
//                        // multi-axis drag value.
//                        Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//                        Vector3 offsetVector = intersectionPoint - _lastGizmoPickPoint;
//
//                        // Retrieve the median vector, normalize it, and then project the offset vector on the resulting vector.
//                        // Note: Projecting on the median vector is not an arbitrary decision. When the user moves the mouse cursor
//                        //       along the surface of a multi-axis triangle we want to give them full scale power as long as they
//                        //       move the mouse along the median vector which can be thought as a diagonal for an imaginary square
//                        //       with its side length equal to the triangle side length. This is because with a multi-axis triangle, we
//                        //       are scaling along 2 axes at once. We could just choose on of these axes as the projection destination,
//                        //       but that wouldn't be 'fair' :D. We want to assign equal importance to both axes and the median vector
//                        //       seems like a good choice to do that.
//                        Vector3 medianVector = GetMultiAxisTriangleMedianVector(_selectedMultiAxisTriangle);
//                        medianVector.Normalize();
//                        float projectionOnMedianVector = Vector3.Dot(medianVector, offsetVector);
//
//                        // Adjust the accumulated scale axis drag value. 
//                        // Note: Depending on the direction of movement along the triangle plane, 'projectionOnMedianVector' can be either
//                        //       positive or negative. Positive values will stretch the objects and negative values will shrink them.
//                        _accumulatedMultiAxisTriangleDrag += projectionOnMedianVector;
//
//                        // Scale the controlled objects along the axes that correspond to the selected multi-axis triangle
//                        ScaleControlledObjects(GetScaleAxisBooleanFlagsForMultiAxisTriangle(_selectedMultiAxisTriangle));
//
//                        // Store the gizmo pick point for the next mouse move event
//                        _lastGizmoPickPoint = intersectionPoint;
//                    }
//                }
//                else
//                // Is the all-axes square selected and does the user want to scale along all axes?
//                if (_scaleAlongAllAxes && _isAllAxesSquareSelected)
//                {
//                    // The following code constructs an imaginary circle around the all-axis square rectangle. The radius of this circle 
//                    // is always the distance between the square center and the current mouse position. When the mouse is moved away from 
//                    // the center of the circle, we add a positive value to '_accumulatedAllAxesSquareDrag' in order to increase the scale.
//                    // When the mouse is moved closer to the center of the circle, we subtract from '_accumulatedAllAxesSquareDrag' in 
//                    // order to decrease the scale. This is not all that we have to do however. Imagine that the user moves the mouse close 
//                    // to the center of the square going from the top of the screen towards the bottom and then, at some point the cursor goes
//                    // below the square center. In that case the distance from the center will start increasing again and it would be very hard 
//                    // for the user to shrink the object. For that reason, we will establish a rule that if the mouse cursor position goes below
//                    // the center of the square, the scale sign is reversed and the movement will perform a decrease rather than an increase.
//                    Vector2 mousePosition = Input.mousePosition;
//                    Vector2 allAxesSquareCenter = _camera.WorldToScreenPoint(_gizmoTransform.position);
//                    Vector2 circleRadiusVector = mousePosition - allAxesSquareCenter;   // This is the circle's radius vector which goes from the square center to the current mouse position
//                    circleRadiusVector.Normalize();
//
//                    // Calculate the scale sign. We want to let the user increase the scale as the cursor moves away
//                    // from its center and decrease it as it moves closer. This is done by checking if the current radius
//                    // vector points away from the mouse cursor move offset since the last frame. We do this by performing
//                    // the dot product between the 2 vectors and storing the sign of the result in 'scaleSign'.
//                    float scaleSign = Mathf.Sign(Vector2.Dot(circleRadiusVector, _mouse.CursorOffsetSinceLastFrame));
//
//                    // If the mouse cursor lies below the circle center, we will invert the sign so that the mouse movement
//                    // interpretation is reversed. When below, moving away means shrinking and moving closer means stretching.
//                    if (circleRadiusVector.y < 0.0f) scaleSign *= -1.0f;
//
//                    // Calculate the accumulated drag value using the length of the mouse cursor offset vector. We scale it by the scale
//                    // sign and by '_allAxesSquareDragUnitsPerScreenUnit' to convert to drag units.
//                    _accumulatedAllAxesSquareDragInScreenUnits += _mouse.CursorOffsetSinceLastFrame.magnitude * scaleSign * _allAxesSquareDragUnitsPerScreenUnit;
//                    ScaleControlledObjects(new bool[] { true, true, true });    // Scale the objects along all axes
//
//                    // When we have snapping activated, we will need a drag value in world units. The following code calculates this value
//                    // and stores in inside '_accumulatedAllAxesSquareDragInWorldUnits'. We do this by constructing a plane instance which
//                    // describes the plane on which the all-axes square is sitting. We then intersect this plane with the mouse cursor ray
//                    // and use the intersection point to calculate the drag amount (i.e. the length of the vector which goes from the gizmo
//                    // position to the intersection point). We then multiply by the sign of the calculated drag in screen units in order to
//                    // ensure that we are scaling in the same direction.
//                    float t;
//                    Plane squareWorldPlane = new Plane(_camera.transform.forward, _gizmoTransform.position);
//                    Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//                    if(squareWorldPlane.Raycast(pickRay, out t))
//                    {
//                        // Calculate the intersection point
//                        Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//
//                        // The distance between the intersection point and the gizmo position represents the amount of drag.
//                        // Note: We aslo multiply with the sign of '_accumulatedAllAxesSquareDragInScreenUnits' to enusre that
//                        //       we are scaling in the same direction.
//                        _accumulatedAllAxesSquareDragInWorldUnits = (_gizmoTransform.position - intersectionPoint).magnitude;
//                        _accumulatedAllAxesSquareDragInWorldUnits *= Mathf.Sign(_accumulatedAllAxesSquareDragInScreenUnits);
//                    }
//                }
//            }
//        }
//        #endregion
//
//        #region Private Methods
//        /// <summary>
//        /// Returns the screen space points which form the square that can be used to scale along
//        /// all axes at once.
//        /// </summary>
//        private Vector2[] GetAllAxesScaleSquareScreenPoints()
//        {
//            // We need to make sure that if we are currently performing a scale operation and '_adjustAllAxesScaleSquareWhileScalingObjects'
//            // is true, we scale the square sides using the correct scale factor.
//            float squareSideScaleFactor = GetAllAxesScaleSquareScaleFactorDuringScaleOperation();
//
//            // Store needed data. We will construct the points by moving away from the square's
//            // center in screen space by half the square size along the screen right and up axes.
//            Vector2 screenSpaceSquareCenter = _camera.WorldToScreenPoint(_gizmoTransform.position);
//            float halfSquareSize = _screenSizeOfAllAxesSquare * 0.5f * squareSideScaleFactor;
//
//            // Construct the point array
//            return new Vector2[]
//            {
//                screenSpaceSquareCenter - (Vector2.right - Vector2.up) * halfSquareSize,        // Top left point
//                screenSpaceSquareCenter + (Vector2.right + Vector2.up) * halfSquareSize,        // Top right point
//                screenSpaceSquareCenter + (Vector2.right - Vector2.up) * halfSquareSize,        // Bottom right point
//                screenSpaceSquareCenter - (Vector2.right + Vector2.up) * halfSquareSize         // Bottom left point
//            };
//        }
//
//        /// <summary>
//        /// This method can be used to check if the mouse cursor position lies inside the area
//        /// of the square which can be used to scale along all axes.
//        /// </summary>
//        private bool IsMouseCursorInsideAllAxesScaleSquare()
//        {
//            // We will need this to perform the check
//            Vector2 screenSpaceSquareCenter = _camera.WorldToScreenPoint(_gizmoTransform.position);
//            float halfSquareSize = _screenSizeOfAllAxesSquare * 0.5f;
//
//            // In order to test if the mouse cursor position lies inside the square, we will first construct
//            // a vector which goes from the square's center to the mouse cursor position. If the X and Y
//            // components of the resulitng vector are <= to half the square size, it means the cursor position
//            // lies inside the square.
//            Vector2 mouseCursorPosition = Input.mousePosition;
//            Vector2 fromSquareCenterToCursorPosition = mouseCursorPosition - screenSpaceSquareCenter;
//
//            // Perform the check by testing the vector's components against the square's half size
//            return Mathf.Abs(fromSquareCenterToCursorPosition.x) <= halfSquareSize && Mathf.Abs(fromSquareCenterToCursorPosition.y) <= halfSquareSize;
//        }
//
//        /// <summary>
//        /// When the user is using a multi-axis triangle to perform a scale operation, we will need
//        /// an array of boolean flags which tell us which axes should be used to scale our objects.
//        /// This method is used for that purpose and it returns an array of boolean values where 
//        /// each boolean value is set to either true or false based on whether or not a scale needs
//        /// to be performed along the corresponding axis. The elements in the array are mapped to the
//        /// scale axes in the following way: element 0 -> X axis, element 1-> Y axis, element 2 -> Z axis.
//        /// So, for example, if the user is scaling using the XY multi-axis traingle, the array will
//        /// contain the following values: true, true, false.
//        /// </summary>
//        private bool[] GetScaleAxisBooleanFlagsForMultiAxisTriangle(MultiAxisTriangle multiAxisTriangle)
//        {
//            switch (multiAxisTriangle)
//            {
//                case MultiAxisTriangle.XY:
//
//                    return new bool[] { true, true, false };
//
//                case MultiAxisTriangle.XZ:
//
//                    return new bool[] { true, false, true };
//
//                case MultiAxisTriangle.YZ:
//
//                    return new bool[] { false, true, true };
//
//                default:
//
//                    return null;
//            }
//        }
//
//        /// <summary>
//        /// Given a multi-axis triangle, the method returns the median vector which starts at
//        /// the point that is shared by the 2 adjacent sides and ends in the middle of the
//        /// triangle's hypotenuse.. We will need to call this method when we need to perform
//        /// a scale using a multi-axis triangle.
//        /// </summary>
//        private Vector3 GetMultiAxisTriangleMedianVector(MultiAxisTriangle multiAxisTriangle)
//        {
//            Vector3 hypotenuseVector;
//            float hypotenuseLength;
//
//            // We will need these to perform the necessary calculations.
//            // Note: We will need to call 'GetWorldAxesMultipliedByMultiAxisExtensionSigns' to make sure
//            //       that we get access to the gizmo axes scaled by the multi-axis triangle extension signs.
//            //       This is necessary because if '_adjustMultiAxisForBetterVisibility' is true, the triangle
//            //       sides will always extend in different directions based on how the camera looks at them.
//            Vector3[] axesUsedToDrawTriangleLines = GetWorldAxesMultipliedByMultiAxisExtensionSigns();
//            float multiAxisTriangleSideLength = GetMultiAxisTriangleSideLength();
//            Vector3 gizmoRight = axesUsedToDrawTriangleLines[0];
//            Vector3 gizmoUp = axesUsedToDrawTriangleLines[1];
//            Vector3 gizmoLook = axesUsedToDrawTriangleLines[2];
//
//            // Calculate the median vector based on specified multi-axis triangle
//            switch (multiAxisTriangle)
//            {
//                case MultiAxisTriangle.XY:
//
//                    // The idea is to generate a point which lies in the middle of the hypotenuse edge. In order to do this
//                    // we will move from the tip of the triangle side which extends along the up vector towards the tip of
//                    // the side which extends along the right vector. If you imagine the trianlge in the XY plane, this is 
//                    // a vector which goes from the end of the Y axis adjacent side to the end of the X axis side. The vector
//                    // that we get is the hypotenuse of the triangle which unites the ends of the 2 adjacent sides.
//                    hypotenuseVector = (gizmoRight - gizmoUp) * multiAxisTriangleSideLength;
//                    hypotenuseLength = hypotenuseVector.magnitude;
//                    hypotenuseVector.Normalize();
//
//                    // Now to get the actual point on the hypotenuse, we will move upwards at the end of the Y axis adjacent side
//                    // and from there we will move along the hypotenuse vector by half its length to get to the median point.
//                    return gizmoUp * multiAxisTriangleSideLength + hypotenuseVector * hypotenuseLength * 0.5f;
//
//                case MultiAxisTriangle.XZ:
//
//                    // Same reasoning as in the 'MultiAxisTriangle.XY' case except different axes
//                    hypotenuseVector = (gizmoRight - gizmoLook) * multiAxisTriangleSideLength;
//                    hypotenuseLength = hypotenuseVector.magnitude;
//                    hypotenuseVector.Normalize();
//
//                    return gizmoLook * multiAxisTriangleSideLength + hypotenuseVector * hypotenuseLength * 0.5f;
//
//                case MultiAxisTriangle.YZ:
//
//                    // Same reasoning as in the 'MultiAxisTriangle.XY' case except different axes
//                    hypotenuseVector = (gizmoLook - gizmoUp) * multiAxisTriangleSideLength;
//                    hypotenuseLength = hypotenuseVector.magnitude;
//                    hypotenuseVector.Normalize();
//
//                    return gizmoUp * multiAxisTriangleSideLength + hypotenuseVector * hypotenuseLength * 0.5f;
//
//                default:
//
//                    return Vector3.zero;
//            }
//        }
//
//        /// <summary>
//        /// Returns the length of the adjacent sides which form the multi-axis triangles. Normally,
//        /// the length can be accessed through '_multiAxisTriangleSideLength', but the real length
//        /// must also take the gizmo scale into consideration and this is what the method does. It
//        /// returns the real side length taking the gizmo scale into account.
//        /// </summary>
//        private float GetMultiAxisTriangleSideLength()
//        {
//            return CalculateGizmoScale() * _multiAxisTriangleSideLength;
//        }
//
//        /// <summary>
//        /// Returns the plane on which the specified multi-axis triangle resides.
//        /// </summary>
//        private Plane GetMultiAxisTrianglePlane(MultiAxisTriangle multiAxisTriangle)
//        {
//            switch (multiAxisTriangle)
//            {
//                case MultiAxisTriangle.XY:
//
//                    return new Plane(_gizmoTransform.forward, _gizmoTransform.position);
//
//                case MultiAxisTriangle.XZ:
//
//                    return new Plane(_gizmoTransform.up, _gizmoTransform.position);
//
//                case MultiAxisTriangle.YZ:
//
//                    return new Plane(_gizmoTransform.right, _gizmoTransform.position);
//
//                default:
//
//                    return new Plane();
//            }
//        }
//
//        /// <summary>
//        /// The method will reorder the triangle vertices in 'worldSpaceTriangleVerts' to ensure
//        /// a clockwise winding order.
//        /// </summary>
//        /// <param name="worldSpaceTriangleVerts">
//        /// The world space triangle vertices which must be reordered.
//        /// </param>
//        /// <param name="triangleWorldTransform">
//        /// The triangle's world transform matrix.
//        /// </param>
//        private void ReorderTriangleVertsForClockwiseWindingOrder(Vector3[] worldSpaceTriangleVerts, Matrix4x4 triangleWorldTransform)
//        {
//            // In order to ensure proper winding order, we will transform the triangle vertices 
//            // in model space using the inverse of the triangle transform matrix.
//            Matrix4x4 triangleInverseTransform = triangleWorldTransform.inverse;
//
//            // Transform the world space vertices in model space and store them in the 'modelSpaceVerts' array
//            Vector3[] modelSpaceVerts = worldSpaceTriangleVerts.Clone() as Vector3[];
//            for (int vertIndex = 0; vertIndex < 3; ++vertIndex)
//            {
//                modelSpaceVerts[vertIndex] = triangleInverseTransform.MultiplyPoint(worldSpaceTriangleVerts[vertIndex]);
//            }
//
//            // Check the winding order. In order to do this, we will build a vector which represents 
//            // the triangle plane normal. If the 'Z' coordinate of the resulting normal is less than 
//            // 0.0f, it means the triangle vertices exist in counter clock-wise winding order and
//            // must be swapped.
//            // Note: The following lines of code assume that the triangle was generated in the XY plane.
//            Vector3 toSecondPoint = modelSpaceVerts[1] - modelSpaceVerts[0];
//            Vector3 toThirdPoint = modelSpaceVerts[2] - modelSpaceVerts[0];
//            Vector3 triangleNormal = Vector3.Cross(toThirdPoint, toSecondPoint);
//
//            // Counter clock-wise?
//            if (triangleNormal.z < 0.0f)
//            {
//                // Swap the last 2 vertices
//                Vector3 temp = worldSpaceTriangleVerts[1];
//                worldSpaceTriangleVerts[1] = worldSpaceTriangleVerts[2];
//                worldSpaceTriangleVerts[2] = temp;
//            }
//        }
//
//        /// <summary>
//        /// Returns the world space points of the specified multi-axis triangle. We need this function
//        /// when checking if the mouse cursor hovers one of the multi-axis triangles.
//        /// </summary>
//        /// <param name="multiAxisTriangleIndex">
//        /// The index of the multi-axis triangle whose world space points must be returned.
//        /// </param>
//        /// <returns>
//        /// The world space points of the specified multi-axis triangle.
//        /// </returns>
//        private Vector3[] GetMultiAxisTriangleWorldSpacePoints(int multiAxisTriangleIndex)
//        {
//            // We will make use of the world axes which are used to draw the triangles
//            Vector3[] axesUsedToDrawTriangles = GetWorldAxesUsedToDrawMultiAxisTriangleLines();
//
//            // There are 2 axes for each triangle so we need to multiply the multi-axis triangle index by 2.
//            // We will use this index to get access to the axes that interest us inside the 'axesUsedToDrawTriangles'
//            // array.
//            float gizmoScale = CalculateGizmoScale();
//            int indexOfFirstAxis = (int)multiAxisTriangleIndex * 2;
//            return new Vector3[] 
//            { 
//                // Note: We will scale the axes by the gizmo scale to obtain the actual axis length as it exists in world space.
//                _gizmoTransform.position, 
//                _gizmoTransform.position + axesUsedToDrawTriangles[indexOfFirstAxis + 1] * _multiAxisTriangleSideLength * gizmoScale,
//                _gizmoTransform.position + axesUsedToDrawTriangles[indexOfFirstAxis] * _multiAxisTriangleSideLength * gizmoScale
//            };
//        }
//
//        /// <summary>
//        /// Draws the gizmo axes lines.
//        /// </summary>
//        private void DrawAxesLines(Axis activeAxes)
//        {
//            // Retrieve the sorted axis indices
//            int[] axisIndices = GetSortedGizmoAxesIndices();
//
//            // Loop through all axes and draw them
//            float gizmoScale = CalculateGizmoScale();
//            Vector3[] gizmoLocalAxes = GetGizmoLocalAxes();
//            Vector3 startPoint = _gizmoTransform.position;
//            foreach (int axisIndex in axisIndices)
//            {
//                if (activeAxes.Has(axisIndex)) {
//                    // Calculate the axis end point
//                    Vector3 endPoint = startPoint + gizmoLocalAxes[axisIndex] * GetAxisLength(axisIndex, gizmoScale);
//
//                    // Make sure the stencil reference values are updated correctly
//                    UpdateShaderStencilRefValuesForGizmoAxisLineDraw(axisIndex, startPoint, endPoint, gizmoScale);
//
//                    // Draw the axis line
//                    GLPrimitives.Draw3DLine(startPoint, endPoint, SelectedAxis == (GizmoAxis)axisIndex ? _selectedAxisColor : _axesColors[axisIndex], MaterialPool.Instance.GizmoLine);   
//                }
//            }
//        }
//
//        /// <summary>
//        /// Returns the length of the specified gizmo axis. This method is necessary because
//        /// the axis length may be different depending on whether or not the user is performing
//        /// a scale operation. So it is not always equal to '_axisLength'. The second parameter
//        /// represents the current gizmo scale which must also be taken into account when we
//        /// need to calculate the axis length.
//        /// </summary>
//        private float GetAxisLength(int axisIndex, float gizmoScale)
//        {
//            // If '_adjustAxisLengthWhileScalingObjects' is false it means that the user doesn't want to
//            // have the length of the axes adjusted during a scale operation. In that case we just return
//            // the original axis length. If '_adjustAxisLengthWhileScalingObjects' is true, but the gizmo
//            // axis is not involved in any way in a scale operation, we also return the original axis legnth.
//            // The axis is involved in a scale operation in the following situations:
//            //      a) it is currently selected (maybe the user is using it to perform a scale operation);
//            //      b) the user has selected a multi-axis triangle and the axis may be shared by that triangle;
//            //      c) the all-axes square is selected.
//            if (!_adjustAxisLengthWhileScalingObjects ||
//                (axisIndex != (int)SelectedAxis && !IsGizmoAxisSharedBySelectedMultiAxisTriangle(axisIndex) && !_isAllAxesSquareSelected)) return _axisLength * gizmoScale;
//
//            // The length of the gizmo axis has to be adjusted and the axis is involved in some way or another
//            // in a scale operation. We will check what kind of scale operation is involved in and apply the
//            // correspondin scale factor to make sure the axis is scaled accordingly.
//            if (SelectedAxis != GizmoAxis.None) return _axisLength * gizmoScale * GetAxisScaleFactorForAccumulatedDrag(SelectedAxis);
//            else if (_selectedMultiAxisTriangle != MultiAxisTriangle.None) return (_axisLength * gizmoScale) * GetMultiAxisTriangleScaleFactorForAccumulatedDrag(_selectedMultiAxisTriangle);
//            else return (_axisLength * gizmoScale) * GetAllAxesSquareScaleFactorForAccumulatedDrag();
//        }
//
//        /// <summary>
//        /// This method checks if the specified gizmo axis is shared by the currently selected
//        /// multi-axis triangle. We will need this because if the gizmo axes have to be adjusted
//        /// during a scale operation, the axis length will need to be scaled when the user is
//        /// using a multi-axis triangle to scale objects.
//        /// </summary>
//        private bool IsGizmoAxisSharedBySelectedMultiAxisTriangle(int axisIndex)
//        {
//            // If no multi-axis triangle is selected, we will just return false
//            if (_selectedMultiAxisTriangle == MultiAxisTriangle.None) return false;
//
//            // Check if the axis is shared by the selected multi-axis triangle
//            if (_selectedMultiAxisTriangle == MultiAxisTriangle.XY) return axisIndex == 0 || axisIndex == 1;
//            else if (_selectedMultiAxisTriangle == MultiAxisTriangle.XZ) return axisIndex == 0 || axisIndex == 2;
//            else return axisIndex == 1 || axisIndex == 2;
//        }
//
//        /// <summary>
//        /// Draws the scale boxes which sit at the tip of each scale axis.
//        /// </summary>
//        /// <param name="worldTransforms">
//        /// This is an array of world transform matrices which describe the absolute
//        /// world transform for each scale box that must be drawn.
//        /// </param>
//        private void DrawScaleBoxes(Axis activeAxes, Matrix4x4[] worldTransforms) {
//            var forward = _camera != null ? _camera.transform.forward : Vector3.forward;
//
//            Material material = MaterialPool.Instance.GizmoSolidComponent;
//            material.SetInt("_IsLit", _areScaleBoxesLit ? 1 : 0);
//            material.SetVector("_LightDir", forward);
//
//            // Loop through each axis and submit the mesh for drawing
//            Mesh boxMesh = MeshPool.Instance.BoxMesh;
//            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
//            {
//                if (activeAxes.Has(axisIndex)) {
//                    Color axisColor = axisIndex == (int)SelectedAxis ? _selectedAxisColor : _axesColors[axisIndex];
//                    material.SetColor("_Color", axisColor);
//                    material.SetInt("_StencilRefValue", _axesStencilRefValues[axisIndex]);
//                    material.SetPass(0);
//                    Graphics.DrawMeshNow(boxMesh, worldTransforms[axisIndex]);                    
//                }
//            }
//        }
//
//        /// <summary>
//        /// Retrieves the world transform matrices which store the world transform information
//        /// for the scale boxes.
//        /// </summary>
//        private Matrix4x4[] GetScaleBoxesWorldTransforms()
//        {
//            Matrix4x4[] worldTransforms = new Matrix4x4[3];
//            Vector3[] localPositions = GetScaleBoxesGizmoLocalPositions(CalculateGizmoScale());
//            Quaternion[] localRotations = GetScaleBoxesGizmoLocalRotations();
//
//            // Loop through each axis
//            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
//            {
//                // Transform the local position and orientation in world space.
//                // Note: The local position was generated along the local axes of the gizmo, so in a way, the position
//                //       is tied to the axes of the gizmo object. For this reason, we will need to rotate the local position
//                //       using the gizmo's rotation to bring it in world space with respect to the orientation of the axes.
//                //       We then add the position of the gizmo object to the result to get the final world space position.
//                Vector3 worldPosition = _gizmoTransform.position + _gizmoTransform.rotation * localPositions[axisIndex];
//                Quaternion worldRotation = _gizmoTransform.rotation * localRotations[axisIndex];
//
//                // Construct the world transform matrix
//                worldTransforms[axisIndex] = new Matrix4x4();
//                worldTransforms[axisIndex].SetTRS(worldPosition, worldRotation, Vector3.Scale(_gizmoTransform.lossyScale, new Vector3(_scaleBoxWidth, _scaleBoxHeight, _scaleBoxDepth)));
//            }
//
//            return worldTransforms;
//        }
//
//        /// <summary>
//        /// Returns the an array of 'Quaternion' instances which represent the rotations
//        /// of the scale boxes in gizmo local space. 
//        /// </summary>
//        private Quaternion[] GetScaleBoxesGizmoLocalRotations()
//        {
//            // We will generate the rotations such that when the user changes the width of the
//            // scale boxes, the width will increase/decrease along the axis to which the scale
//            // box is mapped.
//            return new Quaternion[]
//            {
//                Quaternion.identity,                    
//                Quaternion.Euler(0.0f, 0.0f, 90.0f),
//                Quaternion.Euler(0.0f, 90.0f, 0.0f),
//            };
//        }
//
//        /// <summary>
//        /// Returns the an array of 'Vector3' instances which represent the positions
//        /// of the scale boxes in gizmo local space. 
//        /// </summary>
//        private Vector3[] GetScaleBoxesGizmoLocalPositions(float gizmoScale)
//        {
//            // Note: We always use the width of the scale box to establish the position. This has to do with
//            //       the way in which the quaternions inside 'GetScaleBoxesGizmoLocalRotations' are generated.
//            //       This seems to provide a much more intuitive way of handling positioning and resizing of the
//            //       scale boxes.
//            float halfScaleBoxWidth = 0.5f * _scaleBoxWidth * gizmoScale;
//            return new Vector3[]
//            {
//                Vector3.right * (GetAxisLength(0, gizmoScale) + halfScaleBoxWidth),
//                Vector3.up * (GetAxisLength(1, gizmoScale) + halfScaleBoxWidth),
//                Vector3.forward * (GetAxisLength(2, gizmoScale) + halfScaleBoxWidth),
//            };
//        }
//
//        /// <summary>
//        /// Draws the multi-axis triangles which allow the user to scale along 2 axes at once.
//        /// </summary>
//        /// <param name="worldTransforms">
//        /// This is an array of world transform matrices which describe the absolute
//        /// world transform for each multi-axis triangle that must be drawn.
//        /// </param>
//        private void DrawMultiAxisTriangles(Matrix4x4[] worldTransforms)
//        {
//            Material material = MaterialPool.Instance.GizmoSolidComponent;
//            material.SetInt("_IsLit", 0);
//            material.SetVector("_LightDir", _camera.transform.forward);
//            int cullMode = material.GetInt("_CullMode");
//            material.SetInt("_CullMode", 0);
//
//            // Loop through each multi-axis and submit the mesh for drawing
//            Mesh triMesh = MeshPool.Instance.RightAngledTriangleMesh;
//            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//            {
//                Color multiAxisColor = multiAxisIndex == (int)SelectedAxis ? _selectedMultiAxisTriangleColor : _multiAxisTriangleColors[multiAxisIndex];
//                material.SetColor("_Color", multiAxisColor);
//                material.SetPass(0);
//                Graphics.DrawMeshNow(triMesh, worldTransforms[multiAxisIndex]);
//            }
//            material.SetInt("_CullMode", cullMode);
//        }
//
//        /// <summary>
//        /// Retrieves the world transform matrices that store the world transform information
//        /// for the multi-axis triangles.
//        /// </summary>
//        private Matrix4x4[] GetMultiAxisTrianglesWorldTransforms()
//        {
//            Matrix4x4[] worldTransforms = new Matrix4x4[3];
//            Vector3[] localScales = GetMultiAxisTrianglesGizmoLocalScales();
//            Quaternion[] localRotations = GetMultiAxisTrianglesGizmoLocalRotations();
//
//            // Loop through each axis
//            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//            {
//                // When drawing the triangles we will also need to take into account the fact that the user may be performing a scale
//                // operation. In that case, if '_adjustMultiAxisTrianglesWhileScalingObjects' is true, the triangle area has to become 
//                // larger or shorter depending on how the user is moving the mouse.
//                float multiAxisTriangleScaleFactor = GetMultiAxisTriangleScaleFactorDuringScaleOperation((MultiAxisTriangle)multiAxisIndex);
//
//                // Transform the local scale and orientation in world space. The local position is always the zero vector
//                // because of the way in which the triangle meshes are generated.
//                // Note: When calculating the world scale, we also factor in 'multiAxisTriangleScaleFactor' in order to make sure
//                //       that the triangle area is scaled accordingly.
//                Vector3 worldScale = Vector3.Scale(_gizmoTransform.lossyScale, Vector3.Scale(localScales[multiAxisIndex], new Vector3(_multiAxisTriangleSideLength, _multiAxisTriangleSideLength, 1.0f))) * multiAxisTriangleScaleFactor;
//                Quaternion worldRotation = _gizmoTransform.rotation * localRotations[multiAxisIndex];
//
//                // Build the world matrix
//                worldTransforms[multiAxisIndex] = new Matrix4x4();
//                worldTransforms[multiAxisIndex].SetTRS(_gizmoTransform.position, worldRotation, worldScale);
//            }
//
//            return worldTransforms;
//        }
//
//        /// <summary>
//        /// When performing a scale using a multi-axis triangle, the triangle might need to be scaled
//        /// along with the scaled objects. When drawing the multi-axis triangles, we will call this method
//        /// to retrieve a scale factor which needs to be applied to the adjacent sides of the triangle
//        /// in order to render it correctly.
//        /// </summary>
//        private float GetMultiAxisTriangleScaleFactorDuringScaleOperation(MultiAxisTriangle multiAxisTriangle)
//        {
//            // Note: Because the multi-axis triangles are not drawn when the all-axes square is selected, we will
//            //       not treat the case in which the all-axes square is selected.
//            // If the multi-axis is not selected, it means it can't possibly be involved in a scale operation
//            // and in that case we will just return a value of 1 so that the triangle will be rendered at its
//            // original size. We also return 1 when the triangles must not be adjusted during a scale operation.
//            if (!_adjustMultiAxisTrianglesWhileScalingObjects || multiAxisTriangle != _selectedMultiAxisTriangle) return 1.0f;
//
//            // We can only reach this point when the triangle is selected and '_adjustMultiAxisTrianglesWhileScalingObjects'
//            // is true. In this case we will call 'GetMultiAxisTriangleScaleFactorForAccumulatedDrag' to get access to
//            // the scale factor that we need.
//            return GetMultiAxisTriangleScaleFactorForAccumulatedDrag(multiAxisTriangle);
//        }
//
//        /// <summary>
//        /// When performing a scale using the all-axes square, the square might need to be scaled
//        /// along with the scaled objects. When drawing the square, we will call this method to 
//        /// retrieve a scale factor which needs to be applied to the square sides in order to render
//        /// it correctly.
//        /// </summary>
//        private float GetAllAxesScaleSquareScaleFactorDuringScaleOperation()
//        {
//            // If the square doesn't have to have its size adjusted, we will return a factor of 1 to keep its original size.
//            // Otherwise, we just call 'GetAllAxesSquareScaleFactorForAccumulatedDrag' to retrieve the scale factor.
//            if (!_adjustAllAxesScaleSquareWhileScalingObjects) return 1.0f;
//            return GetAllAxesSquareScaleFactorForAccumulatedDrag();
//        }
//
//        /// <summary>
//        /// Returns the an array of 'Quaternion' instances which represent the rotations
//        /// of the multi-axis triangles in gizmo local space. 
//        /// </summary>
//        private Quaternion[] GetMultiAxisTrianglesGizmoLocalRotations()
//        {
//            return new Quaternion[]
//            {
//                Quaternion.identity,                    // XY
//                Quaternion.Euler(90.0f, 0.0f, 0.0f),    // XZ
//                Quaternion.Euler(0.0f, -90.0f, 0.0f),   // YZ
//            };
//        }
//
//        /// <summary>
//        /// Returns an array of 'Vector3' instances which holds the gizmo local scale of each 
//        /// of the multi-axis triangles. 
//        /// </summary>
//        private Vector3[] GetMultiAxisTrianglesGizmoLocalScales()
//        {
//            // We will call 'GetMultiAxisExtensionSigns' to retrieve the multi-axis extension
//            // signs. This call is necessary in order to account for the existence of the 
//            // '_adjustMultiAxisForBetterVisibility' variable.
//            float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
//
//            // Create the array of local scales.
//            // Note: In order to generate each scale vector, we will multiply the vector components
//            //       with the extension signs retrieved earlier. Only the vector components which
//            //       correspond to the dimensions in which the triangles extend will be multiplied
//            //       by the sign values. For example, for the XY triangle, we will only multiply the
//            //       X and Y components with the extension signs. 
//            // Note: It may be confusing the way in which we multiply by the sign values. This has
//            //       to do with the way in which we generate the local rotations for the multi-axis
//            //       triangles inside the 'GetMultiAxisTrianglesGizmoLocalRotations' method. For example,
//            //       the XZ triangle is rotated 90 degrees around the X axis, which means that we have to
//            //       apply the Z value sign to the Y component because after the rotation, the Y component
//            //       is aligned with the Z axis.
//            return new Vector3[]
//            {
//                new Vector3(1.0f * signs[0], 1.0f * signs[1], 1.0f),  // XY
//                new Vector3(1.0f * signs[0], 1.0f * signs[2], 1.0f),  // XZ
//                new Vector3(1.0f * signs[2], 1.0f * signs[1], 1.0f)   // YZ
//            };
//        }
//
//        /// <summary>
//        /// Draws the lines that surround the multi-axis triangles.
//        /// </summary>
//        private void DrawMultiAxisTrianglesLines()
//        {
//            // We only draw the lines if the all-axes square is not active
//            if (!_scaleAlongAllAxes)
//            {
//                // Retrieve the line points and colors which are needed to draw the multi-axis triangle lines
//                Vector3[] triangleLinesPoints;
//                Color[] triangleLinesColors;
//                GetMultiAxisTrianglesLinePointsAndColors(out triangleLinesPoints, out triangleLinesColors);
//
//                // Draw the lines
//                GLPrimitives.Draw3DLines(triangleLinesPoints, triangleLinesColors, false, MaterialPool.Instance.GizmoLine, false, Color.black);
//            }
//        }
//
//        /// <summary>
//        /// In order to draw the multi-axis triangle lines with one call to the 'GLPrimitives'
//        /// API, we will call this function to give us an array of points and colors for
//        /// the triangle lines. We can then use those arrays to draw the triangle lines using
//        /// a single call to 'GLPrimitives.Draw3DLines'.
//        /// </summary>
//        /// <param name="triangleLinesPoints">
//        /// At the end of the function call, this will hold the points which can be used
//        /// to draw the triangle lines.
//        /// </param>
//        /// <param name="triangleLinesColors">
//        /// At the end of the function call, this will hold the colors which can be used
//        /// to draw the triangle lines.
//        /// </param>
//        private void GetMultiAxisTrianglesLinePointsAndColors(out Vector3[] triangleLinesPoints, out Color[] triangleLinesColors)
//        {
//            // Establish the multi-axis line length so that we don't have to calculate it
//            // every time inside the 'for' loop. We add a small offset to the triangle size
//            // in order to make sure the lines are sitting close to but not exactly on the
//            // filled multi-axis triangle borders.
//            float gizmoScale = CalculateGizmoScale();
//            float multiAxisLineLength = (_multiAxisTriangleSideLength + 0.001f) * gizmoScale;
//
//            // Create the points and colors arrays.
//            // Note: We need 18 elements in the line points array because we have 9 lines total
//            //       and each line requires 2 vertices.
//            triangleLinesPoints = new Vector3[18];
//            triangleLinesColors = new Color[9];
//
//            // All triangle will start from this point
//            Vector3 firstPoint = _gizmoTransform.position;
//
//            // Retrieve the axes which will help us draw the lines and then draw
//            Vector3[] axesUsedForMultiAxisLineDraw = GetWorldAxesUsedToDrawMultiAxisTriangleLines();
//            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//            {
//                // Establish the color of the lines based on whether or not the multi-axis triangle is selected
//                Color lineColor = (multiAxisIndex == (int)_selectedMultiAxisTriangle) ? _selectedMultiAxisTriangleLineColor : _multiAxisTriangleLineColors[multiAxisIndex];
//
//                // Store the color in the color array for each line which makes up the triangle
//                int indexOfFirstColor = multiAxisIndex * 3;
//                triangleLinesColors[indexOfFirstColor] = lineColor;
//                triangleLinesColors[indexOfFirstColor + 1] = lineColor;
//                triangleLinesColors[indexOfFirstColor + 2] = lineColor;
//
//                // Calculate the points which make up the corners of the triangle. 
//                // Note: We need to call 'GetMultiAxisTriangleScaleFactorDuringScaleOperation' to factor in any
//                //       scale if we are scaling game objects.
//                int indexOfFirstDrawAxis = multiAxisIndex * 2;
//                float multiAxisTriangleScaleFactor = GetMultiAxisTriangleScaleFactorDuringScaleOperation((MultiAxisTriangle)multiAxisIndex);
//                Vector3 secondPoint = _gizmoTransform.position + axesUsedForMultiAxisLineDraw[indexOfFirstDrawAxis] * multiAxisLineLength * multiAxisTriangleScaleFactor;
//                Vector3 thirdPoint = _gizmoTransform.position + axesUsedForMultiAxisLineDraw[indexOfFirstDrawAxis + 1] * multiAxisLineLength * multiAxisTriangleScaleFactor;
//
//                // Store the points in the line points array.
//                // Note: We multiply by 6 to get the index of the first point in the current triangle because 
//                //       each triangle has 3 lines, each of them having 2 points.
//                int indexOfFirstPoint = multiAxisIndex * 6;
//                triangleLinesPoints[indexOfFirstPoint] = firstPoint;
//                triangleLinesPoints[indexOfFirstPoint + 1] = secondPoint;
//                triangleLinesPoints[indexOfFirstPoint + 2] = secondPoint;
//                triangleLinesPoints[indexOfFirstPoint + 3] = thirdPoint;
//                triangleLinesPoints[indexOfFirstPoint + 4] = thirdPoint;
//                triangleLinesPoints[indexOfFirstPoint + 5] = firstPoint;
//            }
//        }
//
//        /// <summary>
//        /// Draws the lines which form the square that allows the user to scale along all axes at once.
//        /// </summary>
//        private void DrawAllAxesSquareLines()
//        {
//            // Note: We only draw if the user has activated the square.
//            if (_scaleAlongAllAxes)
//            {
//                // Establish the color which must be used to draw the square lines
//                Color squareLineColor = _isAllAxesSquareSelected ? _colorOfAllAxesSquareLinesWhenSelected : _colorOfAllAxesSquareLines;
//
//                // Draw
//                GLPrimitives.Draw2DRectangleBorderLines(GetAllAxesScaleSquareScreenPoints(), squareLineColor, MaterialPool.Instance.GizmoLine, _camera);
//            }
//        }
//
//        /// <summary>
//        /// This method is used to draw the local axes of all controlled game objects
//        /// during a scale operation. The method only has effect if the user is currently
//        /// performing a scale operation and if they specified that the axes must be drawn
//        /// by settings the '_drawObjectsLocalAxesWhileScaling' to true.
//        /// </summary>
//        private void DrawObjectsLocalAxesDuringScaleOperation()
//        {
//            // The user must be performing a scale operation and the '_drawObjectsLocalAxesWhileScaling' must be true
//            // for any drawing to be performed.
//            if (_mouse.IsLeftMouseButtonDown && _drawObjectsLocalAxesWhileScaling && IsReadyForObjectManipulation() && ControlledObjects != null)
//            {
//                // Retrieve the points and colors which can be used to draw the axes lines.
//                // Note: We will call 'GetTopParentsFromControlledObjects' to get the list of objects that have
//                //       no parents inside the controlled objects list. Only those objects will be affected by
//                //       the scale operation so we will only draw the axes for those objects.
//                List<GameObject> gameObjects = GetParentsFromControlledObjects(true);
//                Vector3[] axesLinesPoints;
//                Color[] axesLinesColors;
//                GetObjectLocalAxesLinePointsAndColors(gameObjects, out axesLinesPoints, out axesLinesColors);
//
//                // Draw the lines in one single draw call
//                GLPrimitives.Draw3DLines(axesLinesPoints, axesLinesColors, false, MaterialPool.Instance.GizmoLine, false, Color.black);
//            }
//        }
//
//        /// <summary>
//        /// This method can be used to retrieve a list of points and colors which are necessary
//        /// for drawing the local axes of the game objects which reside in 'gameObjects'. This
//        /// will help us draw all the axes lines in one single draw call to the GL API.
//        /// </summary>
//        /// <param name="gameObjects">
//        /// The game objects whose local axes must be drawn.
//        /// </param>
//        /// <param name="axesLinesPoints">
//        /// At the end of the function call, this will hold the points which can be used
//        /// to draw the axes lines.
//        /// </param>
//        /// <param name="axesLinesColors">
//        /// At the end of the function call, this will hold the colors which can be used
//        /// to draw the axes lines.
//        /// </param>
//        private void GetObjectLocalAxesLinePointsAndColors(List<GameObject> gameObjects, out Vector3[] axesLinesPoints, out Color[] axesLinesColors)
//        {
//            // We need to establish the axis scale factor for all axes. We do this in order to take into account the 
//            // '_preserveObjectLocalAxesScreenSize' property which states whether or not the objects' local axes must
//            // have their size presered in screen space. If it is true, we will use the same scale that is applied to
//            // the gizmo object. Otherwise, we will use the value 1.0f so that no scaling occurs.
//            float gizmoScale = CalculateGizmoScale();
//            float axisScaleFactor = _preserveObjectLocalAxesScreenSize ? gizmoScale : 1.0f;
//
//            // We need to get the actual axis length to account for the axis scale factor. This will help us avoid
//            // having to recalculate it inside the 'for' loop.
//            float objectLocalAxesLength = _objectsLocalAxesLength * axisScaleFactor;
//
//            // Now we need to identify the scale factor which must be used for each axis individually. This is necessary
//            // in order to take into account the '_adjustObjectLocalAxesWhileScalingObjects' variable. The axes which
//            // correspond to the ones which are involved in a scale operation, will have to be adjusted accordingly if
//            // the '_adjustObjectLocalAxesWhileScalingObjects' variable is true. We will assume a scale of 1.0f for all
//            // axes initially and modify the scale only if necessary.
//            float xAxisScale = 1.0f;
//            float yAxisScale = 1.0f;
//            float zAxisScale = 1.0f;
//            if (_adjustObjectLocalAxesWhileScalingObjects)
//            {
//                // Calculate the scale factor for the X axis.
//                // Note: The scale factor will be the same as the on that is used to scale the component
//                //       which is involved in the scale operation. This will make sure that the objects'
//                //       local axes are scaled proportionally to the other components.
//                if (SelectedAxis == GizmoAxis.X) xAxisScale = GetAxisScaleFactorForAccumulatedDrag(GizmoAxis.X);
//                else if (_selectedMultiAxisTriangle == MultiAxisTriangle.XY || _selectedMultiAxisTriangle == MultiAxisTriangle.XZ) xAxisScale = GetMultiAxisTriangleScaleFactorForAccumulatedDrag(_selectedMultiAxisTriangle);
//                else if (_isAllAxesSquareSelected) xAxisScale = GetAllAxesSquareScaleFactorForAccumulatedDrag();
//
//                // Calculate the scale factor for the Y axis.
//                if (SelectedAxis == GizmoAxis.Y) yAxisScale = GetAxisScaleFactorForAccumulatedDrag(GizmoAxis.Y);
//                else if (_selectedMultiAxisTriangle == MultiAxisTriangle.XY || _selectedMultiAxisTriangle == MultiAxisTriangle.YZ) yAxisScale = GetMultiAxisTriangleScaleFactorForAccumulatedDrag(_selectedMultiAxisTriangle);
//                else if (_isAllAxesSquareSelected) yAxisScale = GetAllAxesSquareScaleFactorForAccumulatedDrag();
//
//                // Calculate the scale factor for the Z axis.
//                if (SelectedAxis == GizmoAxis.Z) zAxisScale = GetAxisScaleFactorForAccumulatedDrag(GizmoAxis.Z);
//                else if (_selectedMultiAxisTriangle == MultiAxisTriangle.XZ || _selectedMultiAxisTriangle == MultiAxisTriangle.YZ) zAxisScale = GetMultiAxisTriangleScaleFactorForAccumulatedDrag(_selectedMultiAxisTriangle);
//                else if (_isAllAxesSquareSelected) zAxisScale = GetAllAxesSquareScaleFactorForAccumulatedDrag();
//            }
//
//            // Create the lines points and colors arrays
//            int numberOfLines = gameObjects.Count * 3;
//            axesLinesPoints = new Vector3[numberOfLines * 2];
//            axesLinesColors = new Color[numberOfLines];
//
//            // Loop through all game objects
//            for (int gameObjectIndex = 0; gameObjectIndex < gameObjects.Count; ++gameObjectIndex)
//            {
//                // Cache the object's transform information like position and local axes so that we can access them easily
//                Transform objectTransform = gameObjects[gameObjectIndex].transform;
//                Vector3 gameObjectPosition = objectTransform.position;
//                Vector3 gameObjectRight = objectTransform.right;
//                Vector3 gameObjectUp = objectTransform.up;
//                Vector3 gamebjectLook = objectTransform.forward;
//
//                // Calculate the index of the first color element in the color array. We have 3 axes
//                // per game object which means that we have to multiply the game object index by 3
//                // to get the color of the first axis of the current game object.
//                int indexOfFirstColor = gameObjectIndex * 3;
//                axesLinesColors[indexOfFirstColor] = _axesColors[0];        // X axis
//                axesLinesColors[indexOfFirstColor + 1] = _axesColors[1];    // Y axis
//                axesLinesColors[indexOfFirstColor + 2] = _axesColors[2];    // Z axis
//
//                // Calculate the index of the first point element inside the line points array. We have
//                // 3 axes per game object and each aixs has 2 points. So we have 6 points in total for each
//                // game object. This is why we have to multiply the game object index by 6 to get the 
//                // index of the first point for the first axis of the current object.
//                int indexOfFirstPoint = gameObjectIndex * 6;
//                axesLinesPoints[indexOfFirstPoint] = gameObjectPosition + gameObjectRight * objectLocalAxesLength * xAxisScale;         // X start
//                axesLinesPoints[indexOfFirstPoint + 1] = gameObjectPosition - gameObjectRight * objectLocalAxesLength * xAxisScale;     // X end
//                axesLinesPoints[indexOfFirstPoint + 2] = gameObjectPosition + gameObjectUp * objectLocalAxesLength * yAxisScale;        // Y start
//                axesLinesPoints[indexOfFirstPoint + 3] = gameObjectPosition - gameObjectUp * objectLocalAxesLength * yAxisScale;        // Y end
//                axesLinesPoints[indexOfFirstPoint + 4] = gameObjectPosition + gamebjectLook * objectLocalAxesLength * zAxisScale;       // Z start
//                axesLinesPoints[indexOfFirstPoint + 5] = gameObjectPosition - gamebjectLook * objectLocalAxesLength * zAxisScale;       // Z end
//            }
//        }
//
//        /// <summary>
//        /// Returns an array of world space axes which are used when drawing the lines that 
//        /// surround the multi-axis triangles. The returned axes are normalized.
//        /// </summary>
//        private Vector3[] GetWorldAxesUsedToDrawMultiAxisTriangleLines()
//        {
//            // Note: The axes are scaled by the corresponding sign value in order to make sure the
//            //       lines extend in the correct direction along each axis.
//            float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
//            return new Vector3[]
//            {
//                _gizmoTransform.right * signs[0], _gizmoTransform.up * signs[1],            // XY multi-axis sides extend along the right and up vectors
//                _gizmoTransform.right * signs[0], _gizmoTransform.forward * signs[2],       // XZ multi-axis sides extend along the right and forward vectors
//                _gizmoTransform.up * signs[1], _gizmoTransform.forward * signs[2]           // YZ multi-axis sides extend along the up and forward vectors
//            };
//        }
//
//        /// <summary>
//        /// Returns an array of world space axes which describe the gizmo local axes in world
//        /// space, each of them being scaled by the multi-axis extension signs. We will need
//        /// to call this function when we need to figure out the axes along which the multi-axis
//        /// triangles are extending.
//        /// </summary>
//        private Vector3[] GetWorldAxesMultipliedByMultiAxisExtensionSigns()
//        {
//            // Retrieve the multi-axis extension signs and use them to construct the axes array
//            float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
//            return new Vector3[]
//            {
//                _gizmoTransform.right * signs[0],
//                _gizmoTransform.up * signs[1], 
//                _gizmoTransform.forward * signs[2],
//            };
//        }
//
//        /// <summary>
//        /// Scales the controlled objects using the specified axes flags array. Each element inside
//        /// the array is mapped to a scale axis in the following manner: element 0 -> X, element 1 -> Y,
//        /// element 2 -> Z. When an element is true, a scale operation will be performed along that axis.
//        /// </summary>
//        private void ScaleControlledObjects(bool[] axesFlags)
//        {
//            // The new objects' scale along the scale axes is the scale the objects had at the moment the scale operation  
//            // started multiplied by the scale which is returned from 'GetScaleFactorForGameObjectGlobalScaleSnapshot'.
//            float scaleFactor = GetScaleFactorForGameObjectGlobalScaleSnapshot();
//
//            // In order to avoid having 'if' statements inside the 'for' loop, we will construct a scale vector which we
//            // will use to scale each axis individually for those axes that require scaling. We will start with a vector 
//            // that has all components set to 1 an all axes by default. Components which require scaling will be set to
//            // 'scaleFactor'.
//            Vector3 scaleFactorVector = Vector3.one;
//            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
//            {
//                // If the axis requires scaling, we will store the scale factor
//                if (axesFlags[axisIndex]) scaleFactorVector[axisIndex] = scaleFactor;
//            }
//
//            // Experimenting a little bit with the way in which Unity scales objects which belong to
//            // hierarchies, it seems that when you select a group of objects that belong to the same 
//            // hierarchy, only the top parent of the selection is scaled. This makes sense because if
//            // we were to modify the scale of all objects, without taking into consideration the parent
//            // child relationships, we would get undesirable results because the scale that we apply 
//            // to the parents would propagate to the children and combining that with the scale that 
//            // we would also apply to the children directly, would result in really uncontrollable scale
//            // values. So, the first step is to identify the top parents and store them in a separate list.
//            //List<GameObject> topParents = GetParentsFromControlledObjects(true);
//
//            // Loop through all top parents and scale them accordingly
//            const float minObjectScale = 0.1f;
//            const float maxObjectScale = 4.0f;
//            // Cache object data
//            //Transform objectTransform = _gizmoTransform;
//
//            // Calculate the new scale of the object.
//            // Note: Make sure the game object's scale doesn't reach 0 on any axis. If that happens, we would not be 
//            //       able to further scale the game object because every scale factor applied to 0 would result in a
//            //       value of 0.
//            Vector3 newObjectLocalScale = _initialScale;
//            newObjectLocalScale = Vector3.Scale(newObjectLocalScale, scaleFactorVector);
//            newObjectLocalScale.x = Mathf.Clamp(newObjectLocalScale.x, minObjectScale, maxObjectScale);
//            newObjectLocalScale.y = Mathf.Clamp(newObjectLocalScale.y, minObjectScale, maxObjectScale);
//            newObjectLocalScale.z = Mathf.Clamp(newObjectLocalScale.z, minObjectScale, maxObjectScale);
//
//            // We will need to calculate the relative scale of the game object (i.e. how much it has changed).
//            // Note: We will make sure to clamp the scale to the minimum value to avoid divides by 0.
//            Vector3 oldLocalScale = InputRange.UnitValue;
//            oldLocalScale.x = Mathf.Clamp(oldLocalScale.x, minObjectScale, maxObjectScale);
//            oldLocalScale.y = Mathf.Clamp(oldLocalScale.y, minObjectScale, maxObjectScale);
//            oldLocalScale.z = Mathf.Clamp(oldLocalScale.z, minObjectScale, maxObjectScale);
//
//            // Calculate the relative scale by performing a component-wise division between the new scale and the old one
//            Vector3 relativeScale = Vector3.Scale(newObjectLocalScale, new Vector3(1.0f / oldLocalScale.x, 1.0f / oldLocalScale.y, 1.0f / oldLocalScale.z));
//
//            // If the center transform pivot point is used, we will scale the objects' distance from the gizmo position.
//            // This seems to be the behaviour of the Unity gizmos, so we will do the same thing.
//            if (_transformPivotPoint == TransformPivotPoint.Center)
//            {
//                // Construct a vector from the gizmo position to the object's position
//                //Vector3 fromPivotPointToPosition = objectTransform.position - _gizmoTransform.position;
//                Vector3 fromPivotPointToPosition = Vector3.zero;
//
//                // Store the object's local axes
//                Vector3 objectRight = _gizmoTransform.right;
//                Vector3 objectUp = _gizmoTransform.up;
//                Vector3 objectLook = _gizmoTransform.forward;
//
//                // Calculate the projection of 'fromPivotPointToPosition' onto the object's local axes. This
//                // will give us the distance between the object's position and the gizmo position along each
//                // of the 3 axes.
//                float projectionOnRight = Vector3.Dot(objectRight, fromPivotPointToPosition);
//                float projectionOnUp = Vector3.Dot(objectUp, fromPivotPointToPosition);
//                float projectionOnLook = Vector3.Dot(objectLook, fromPivotPointToPosition);
//
//                // Scale the offsets which correspond to the axes that are being scaled
//                if (axesFlags[0]) projectionOnRight *= relativeScale.x;
//                if (axesFlags[1]) projectionOnUp *= relativeScale.y;
//                if (axesFlags[2]) projectionOnLook *= relativeScale.z;
//
//                // Change the object's scale and also adjust the position by scaling the distance between the
//                // object and the gizmo position.
//                //objectTransform.localScale = newObjectLocalScale;
//                InputRange.SetUnitValue(newObjectLocalScale);
//                //objectTransform.position = _gizmoTransform.position + objectRight * projectionOnRight + objectUp * projectionOnUp + objectLook * projectionOnLook;
//            }
//            else {
//                InputRange.SetUnitValue(newObjectLocalScale);
//                //objectTransform.localScale = newObjectLocalScale;  // When mesh pivot is used as the transform pivot, just scale the object and leave the postion untouched
//            }
//
//            // The game objects were transformed since the left mouse button was pressed
//            _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
//        }
//
//        /// <summary>
//        /// This method is called from 'ScaleControlledObjects' to establish the scale factor which
//        /// must be applied to the 'snapshot' global scale of a game object.
//        /// </summary>
//        private float GetScaleFactorForGameObjectGlobalScaleSnapshot()
//        {
//            // We will return the scale factor based on what is currently selected
//            if (SelectedAxis != GizmoAxis.None) return GetAxisScaleFactorForAccumulatedDrag(SelectedAxis);
//            else if (_selectedMultiAxisTriangle != MultiAxisTriangle.None) return GetMultiAxisTriangleScaleFactorForAccumulatedDrag(_selectedMultiAxisTriangle);
//            else return GetAllAxesSquareScaleFactorForAccumulatedDrag();
//        }
//
//        /// <summary>
//        /// When we need to apply a scale to the controlled objects, we will call this method to retrieve
//        /// the scale factor that must be used to modify the objects' scale. This method is only called
//        /// when peforming a scale operation using one of the scale axis/boxes.
//        /// </summary>
//        private float GetAxisScaleFactorForAccumulatedDrag(GizmoAxis gizmoAxis)
//        {
//            // We need to handle this differently based on whether or not snapping is used
//            if(_snapSettings.IsSnappingEnabled)
//            {
//                // If the accumulated scale axis drag is greater than the step value in world units, we will
//                // need to calculate the scale factor and return it. Otherwise, we will return 1.0f meaning that
//                // no scale should be applied until a full step value was traversed.
//                if (Mathf.Abs(_accumulatedScaleAxisDrag) >= _snapSettings.StepValueInWorldUnits)
//                {
//                    // Calculate the total number of world units which reside inside the acumulated drag value.
//                    // This is the number of full step values which reside in '_accumulatedScaleAxisDrag' multiplied
//                    // by the step value and the sign of '_accumulatedScaleAxisDrag' to take the scale direction
//                    // into account.
//                    float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedScaleAxisDrag / _snapSettings.StepValueInWorldUnits)));
//                    float totalWorldUnits = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedScaleAxisDrag);
//
//                    // The scale factor is (1 + totalWorldUnits) / 1. We use the value 1 because the snap step value
//                    // represents a value in world units, so we must use the value 1 as the reference value to establish
//                    // how the scale operation affects the scaled entities.
//                    return 1 + totalWorldUnits;
//                }
//                else return 1.0f;
//            }
//            else
//            {
//                // Let's forget about how we are scaling our objects for now and imagine that we did it
//                // in a different way. For example, let's suppose that each time that we dragged one of
//                // the scale axes, we added (or subtracted) a small value from the current scale of the
//                // controlled objects. This would probably work and it isn't that bad. However, imagine
//                // that we wanted to shrink the objects or even invert the scale along one of the axes.
//                // When the scale box reaches the other side of the axis (positive or negative depending
//                // on how we are scaling), some objects would have had their scale inverted but others
//                // didn't. This can happen when the objects' scale values differ. For example if we had
//                // 2 controlled objects: a cube (A) with a scale of 1 on all axes and another cube (B)
//                // with a scale of 10 on all axes. It is clear that it will take a lot longer for cube
//                // B to have its scale inverted. Moreover, we want the scale axes to act as a visual guide
//                // for the current scale situation. That means that as soon as a gizmo scale axis is inverted,
//                // we also want the scale of the objects to be inverted. For this reason, we will scale the
//                // 'snapshot' local scale of the controlled objects by the ratio between 2 values: the
//                // first value represents the sum between the scale axis length and the accumulated axis
//                // drag and the second value represents the actual axis length. When no drag exists, the
//                // ratio will evalulate to 1, meaning that no scale should be performed. When the drag
//                // reaches the same value as the axis length, the ratio will evaluate to 2, meaning that
//                // the scale axis has become 2 times bigger. Scaling the local object scale by this ratio
//                // will keep a tight relationship between the gizmo axis length and the local object scale.
//                // When the ratio reaches the value 0, the local scale will also be set to 0. When the
//                // ratio becomes negative, the local scale will also become negative. And because we are
//                // working in percentages (the ratio), all controlled objects will be scaled in the same
//                // manner regardless of their difference in size/scale. When the ratio reaches the value 0,
//                // the local scale of all objects will be set to 0. When the ratio becomes negative, the
//                // local scale of all objects will become negative.
//                float axisLength = _axisLength * CalculateGizmoScale();
//                return (axisLength + _accumulatedScaleAxisDrag) / axisLength;
//            }
//        }
//
//        /// <summary>
//        /// When we need to apply a scale to the controlled objects, we will call this method to retrieve
//        /// the scale factor that must be used to modify the objects' scale. This method is only called
//        /// when peforming a scale operation using one of the multi-axis triangles.
//        /// </summary>
//        private float GetMultiAxisTriangleScaleFactorForAccumulatedDrag(MultiAxisTriangle multiAxisTriangle)
//        {
//            // Note: The code which handles snapping is the same as the one used in 'GetAxisScaleFactorForAccumulatedDrag',
//            //       with the exception that we are now using '_accumulatedMultiAxisTriangleDrag' to perform the necessary
//            //       calculations.
//            if(_snapSettings.IsSnappingEnabled)
//            {
//                if (Mathf.Abs(_accumulatedMultiAxisTriangleDrag) >= _snapSettings.StepValueInWorldUnits)
//                {
//                    float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedMultiAxisTriangleDrag / _snapSettings.StepValueInWorldUnits)));
//                    float totalWorldUnits = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedMultiAxisTriangleDrag);
//
//                    return 1 + totalWorldUnits;
//                }
//                else return 1.0f;
//            }
//            else
//            {
//                // We will follow the same principles as we did inside the 'GetAxisScaleFactorForAccumulatedDrag'
//                // but this time we will use the median vector of the specified multi-axis triangle instead of 
//                // the scale axis length. We use the median vector because inside the 'OnMouseMoved' method, the
//                // median vector is used to decide how much drag must be added to '_accumulatedMultiAxisTriangleDrag'.
//                // So the scale has to be calculated relative to this vector.
//                float multiAxisTriangleMedianLength = GetMultiAxisTriangleMedianVector(_selectedMultiAxisTriangle).magnitude;
//                return (multiAxisTriangleMedianLength + _accumulatedMultiAxisTriangleDrag) / multiAxisTriangleMedianLength;
//            }
//        }
//
//        /// <summary>
//        /// When we need to apply a scale to the controlled objects, we will call this method to retrieve
//        /// the scale factor that must be used to modify the objects' scale. This method is only called
//        /// when peforming a scale operation using the all-axes scale square.
//        /// </summary>
//        private float GetAllAxesSquareScaleFactorForAccumulatedDrag()
//        {
//            // Note: The code which handles snapping is the same as the one used in 'GetAxisScaleFactorForAccumulatedDrag',
//            //       with the exception that we are now using '_accumulatedAllAxesSquareDragInWorldUnits' to perform the 
//            //       necessary calculations.
//            if(_snapSettings.IsSnappingEnabled)
//            {
//                if (Mathf.Abs(_accumulatedAllAxesSquareDragInWorldUnits) >= _snapSettings.StepValueInWorldUnits)
//                {
//                    float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedAllAxesSquareDragInWorldUnits / _snapSettings.StepValueInWorldUnits)));
//                    float totalWorldUnits = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedAllAxesSquareDragInWorldUnits);
//
//                    return 1 + totalWorldUnits;
//                }
//                else return 1.0f;
//            }
//            else
//            {
//                // We will follow the same principles as we did inside the 'GetAxisScaleFactorForAccumulatedDrag' and
//                // 'GetMultiAxisTriangleScaleFactorForAccumulatedDrag', but this time we will use square's screen
//                // size to calculate the scale factor.
//                return (_screenSizeOfAllAxesSquare + _accumulatedAllAxesSquareDragInScreenUnits) / _screenSizeOfAllAxesSquare;
//            }
//        }
//
//        #if UNITY_EDITOR
//        /// <summary>
//        /// We will use this to draw a rough representation of the gizmo inside the scene view.
//        /// </summary>
//        private void OnDrawGizmosSelected()
//        {
//            // Save these for later restore
//            Color oldGizmosColor = Gizmos.color;
//            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
//
//            Transform gizmoTransform = transform;
//            Gizmos.matrix = gizmoTransform.localToWorldMatrix * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_gizmoBaseScale, _gizmoBaseScale, _gizmoBaseScale));
//
//            // Draw the X axis
//            Vector3 startPoint = Vector3.zero;
//            Vector3 endPoint = startPoint + Vector3.right * _axisLength;
//            Gizmos.color = _axesColors[0];
//            Gizmos.DrawLine(startPoint, endPoint);
//
//            // Draw the Y axis
//            endPoint = startPoint + Vector3.up * _axisLength;
//            Gizmos.color = _axesColors[1];
//            Gizmos.DrawLine(startPoint, endPoint);
//
//            // Draw the Z axis
//            endPoint = startPoint + Vector3.forward * _axisLength;
//            Gizmos.color = _axesColors[2];
//            Gizmos.DrawLine(startPoint, endPoint);
//
//            // Draw the scale boxes
//            DrawAxisScaleBoxInSceneView(GizmoAxis.X);
//            DrawAxisScaleBoxInSceneView(GizmoAxis.Y);
//            DrawAxisScaleBoxInSceneView(GizmoAxis.Z);
//
//            // Draw the multi-axis triangles
//            DrawMultiAxisTriangleInSceneView(MultiAxisTriangle.XY);
//            DrawMultiAxisTriangleInSceneView(MultiAxisTriangle.XZ);
//            DrawMultiAxisTriangleInSceneView(MultiAxisTriangle.YZ);
//
//            // Draw the XY multi-axis triangle border lines
//            Gizmos.color = _multiAxisTriangleLineColors[0];
//            Gizmos.DrawLine(Vector3.zero, Vector3.right * _multiAxisTriangleSideLength);
//            Gizmos.DrawLine(Vector3.right * _multiAxisTriangleSideLength, Vector3.up * _multiAxisTriangleSideLength);
//            Gizmos.DrawLine(Vector3.up * _multiAxisTriangleSideLength, Vector3.zero);
//
//            // Draw the XZ multi-axis triangle border lines
//            Gizmos.color = _multiAxisTriangleLineColors[1];
//            Gizmos.DrawLine(Vector3.zero, Vector3.right * _multiAxisTriangleSideLength);
//            Gizmos.DrawLine(Vector3.right * _multiAxisTriangleSideLength, Vector3.forward * _multiAxisTriangleSideLength);
//            Gizmos.DrawLine(Vector3.forward * _multiAxisTriangleSideLength, Vector3.zero);
//
//            // Draw the YZ multi-axis triangle border lines
//            Gizmos.color = _multiAxisTriangleLineColors[2];
//            Gizmos.DrawLine(Vector3.zero, Vector3.up * _multiAxisTriangleSideLength);
//            Gizmos.DrawLine(Vector3.up * _multiAxisTriangleSideLength, Vector3.forward * _multiAxisTriangleSideLength);
//            Gizmos.DrawLine(Vector3.forward * _multiAxisTriangleSideLength, Vector3.zero);
//
//            // Restore the gizmos color and matrix
//            Gizmos.color = oldGizmosColor;
//            Gizmos.matrix = oldGizmosMatrix;
//        }
//
//        /// <summary>
//        /// Draws a scale box for the specified gizmo axis inside the scene view.
//        /// </summary>
//        private void DrawAxisScaleBoxInSceneView(GizmoAxis gizmoAxis)
//        {
//            // Calculate the scale box size and position based on the specified gizmo axis
//            // Note: The size vector is calculated in such a way that the width of the box extends along
//            //       the direction of the axis and the height and depth extend perpendicular to the axis.
//            Vector3 scaleBoxSize = Vector3.zero, scaleBoxPosition = Vector3.zero;
//            if (gizmoAxis == GizmoAxis.X)
//            {
//                scaleBoxSize = new Vector3(_scaleBoxWidth, _scaleBoxHeight, _scaleBoxDepth);
//                scaleBoxPosition = Vector3.right * (_axisLength + scaleBoxSize[0] * 0.5f);
//            }
//            else if (gizmoAxis == GizmoAxis.Y)
//            {
//                scaleBoxSize = new Vector3(_scaleBoxHeight, _scaleBoxWidth, _scaleBoxDepth);
//                scaleBoxPosition = Vector3.up * (_axisLength + scaleBoxSize[1] * 0.5f);
//            }
//            else if (gizmoAxis == GizmoAxis.Z)
//            {
//                scaleBoxSize = new Vector3(_scaleBoxDepth, _scaleBoxHeight, _scaleBoxWidth);
//                scaleBoxPosition = Vector3.forward * (_axisLength + scaleBoxSize[2] * 0.5f);
//            }
//
//            // Draw the scale box
//            Gizmos.color = _axesColors[(int)gizmoAxis];
//            Gizmos.DrawCube(scaleBoxPosition, scaleBoxSize);
//        }
//
//        /// <summary>
//        /// Draws the specified multi axis triangle in the scene view.
//        /// </summary>
//        private void DrawMultiAxisTriangleInSceneView(MultiAxisTriangle multiAxisTriangle)
//        {
//            // We don't have a triangle drawing function when using the 'Gizmos' API, so we will have to
//            // improvise. We could create a mesh each time the triangle needs to be drawn, but that doesn't
//            // sit well with me :D. So, we will aproximate the area of the triangle using lines.
//            const int numberOfFillLinesPerWorldUnit = 100;
//            int numberOfFillLinesForTriangle = (int)(numberOfFillLinesPerWorldUnit * _multiAxisTriangleSideLength) + 1;
//            float spaceBetweenLines = _multiAxisTriangleSideLength / numberOfFillLinesForTriangle;
//
//            // We will draw the lines across the area of the triangle. That means that each line
//            // has to be positioned along one of the adjacent sides of the triangle and it must 
//            // also become shorter along the other adjacent side. The following lines of code calculate
//            // the vectors which represent the triangle adjacent sides. They will help us when
//            // drawing the lines.
//            Vector3 lineRightAxis = Vector3.zero;
//            Vector3 lineUpAxis = Vector3.zero;
//            if(multiAxisTriangle == MultiAxisTriangle.XY)
//            {
//                lineRightAxis = Vector3.right;
//                lineUpAxis = Vector3.up;
//            }
//            else
//            if (multiAxisTriangle == MultiAxisTriangle.XZ)
//            {
//                lineRightAxis = Vector3.right;
//                lineUpAxis = Vector3.forward;
//            }
//            else
//            if (multiAxisTriangle == MultiAxisTriangle.YZ)
//            {
//                lineRightAxis = Vector3.forward;
//                lineUpAxis = Vector3.up;
//            }
//
//            // Set the triangle color
//            Gizmos.color = _multiAxisTriangleColors[(int)multiAxisTriangle];
//
//            // We will always draw each line such that its first point reisdes on the calculated right axis
//            // and its top point reisdes on the hypotenuse. For this, we will need 2 offset values. The first
//            // one, 'offsetAlongRight', allows us to calculate the starting point of the line and the second
//            // one, 'offsetAlongUp', allows us to position the second point of the line on the hypotenuse.
//            float offsetAlongRight = 0.0f;
//            float offsetAlongUp = _multiAxisTriangleSideLength;
//            for(int lineIndex = 0; lineIndex < numberOfFillLinesForTriangle; ++lineIndex)
//            {
//                // Draw the line. The first point is given by moving along the line right axis by 'offsetAlongRight'
//                // and the second point is calculated by moving from the first point to the point on the hypotenuse.
//                // In order to move the point on the hypotenuse we add 'lineUpAxis * offsetAlongUp'.
//                Gizmos.DrawLine(lineRightAxis * offsetAlongRight, lineRightAxis * offsetAlongRight + lineUpAxis * offsetAlongUp);
//
//                // Now we need to adjust the offsets accordingly. Because we are dealing with a triangle that has equal
//                // adjacent sides, the slope of the hypotenuse is -1. This means that as we move along the line right axis
//                // by a certain amount (i.e. 'spaceBetweenLines'), we will have to move in reverse along the line up axis
//                // by the same amount.
//                offsetAlongRight += spaceBetweenLines;
//                offsetAlongUp -= spaceBetweenLines;
//            }
//        }
//        #endif
//        #endregion
//    }
//}
