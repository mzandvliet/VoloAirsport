//using UnityEngine;
//using System;
//using System.Collections.Generic;
//
//namespace RTEditor
//{
//    /// <summary>
//    /// This class implements the behaviour for a translation gizmo. A translation gizmo is composed 
//    /// of the following:
//    ///     a) 3 translation axes. When the user clicks on of these axes and the starts moving the mouse,
//    ///        a translation will happen.
//    ///     b) 3 arrow cones which appear at the tip of each axis. These can also be used to perform a 
//    ///        translation gizmo in the same way as the axes can be used for the same purpose.
//    ///     c) 3 multi-axis translation squares that can be used to perform a translation on 2 axes
//    ///        simultaneously. When the user clicks one of these squares and then moves the mouse, a
//    ///        translation will happen along the axes which share the same plane as the multi-axis square;
//    ///     d) a square at the center of the gizmo which can be used to translate along the camera right
//    ///        and up vectors respectively;
//    ///     e) a square at the center of the gizmo which can be used to perform vertex snapping.
//    /// </summary>
//    /// <remarks>
//    /// The arrays which hold the gizmo multi-axis properties store the data in the following format:
//    ///     -[0] -> XY multi-axis;
//    ///     -[1] -> XZ multi-axis;
//    ///     -[2] -> YZ multi-axis.
//    /// </remarks>
//    public class TranslationGizmo : Gizmo
//    {
//        #region Private Variables
//        /// <summary>
//        /// This represents the length of a translation axis. All axes share the same length.
//        /// </summary>
//        [SerializeField]
//        private float _axisLength = 5.0f;
//
//        /// <summary>
//        /// This represents the radius of the arrow cones which sit at the end of an axis.
//        /// </summary>
//        [SerializeField]
//        private float _arrowConeRadius = 0.4f;
//
//        /// <summary>
//        /// This represents the length (i.e. height) of the arrow cones which sit at the end of an axis.
//        /// </summary>
//        [SerializeField]
//        private float _arrowConeLength = 1.19f;
//
//        /// <summary>
//        /// The size of a multi-axis square side.
//        /// </summary>
//        [SerializeField]
//        private float _multiAxisSquareSize = 1.0f;
//
//        /// <summary>
//        /// If this is set to true, the multi-axis squares will be positioned in such a way that they
//        /// will always be visible to the user for easy manipulation. Otherwise, the multi-axis squares
//        /// will always be positioned in the following manner: XY multi-axis -> sits at the corner which
//        /// is formed by the X and Y axes; XZ multi-axis-> sits at the corner which is formed by the X
//        /// and Z axes; YZ multi-axis -> sits at the corner which is formed by the Y and Z axes.
//        /// </summary>
//        [SerializeField]
//        private bool _adjustMultiAxisForBetterVisibility = true;
//
//        /// <summary>
//        /// Array which holds colors for each of the 3 multi-axis squares. These
//        /// are the colors used to draw the multi-axis plane meshes.
//        /// </summary>
//        [SerializeField]
//        private Color[] _multiAxisSquareColors = new Color[3] 
//        {
//            new Color(DefaultXAxisColor.r, DefaultXAxisColor.g, DefaultXAxisColor.b, 0.2f),
//            new Color(DefaultYAxisColor.r, DefaultYAxisColor.g, DefaultYAxisColor.b, 0.2f),
//            new Color(DefaultZAxisColor.r, DefaultZAxisColor.g, DefaultZAxisColor.b, 0.2f)
//        };
//
//        /// <summary>
//        /// Array which holds colors used to draw the multi-axis lines that surround the 
//        /// multi-axis squares.
//        /// </summary>
//        [SerializeField]
//        private Color[] _multiAxisSquareLineColors = new Color[] { DefaultXAxisColor, DefaultYAxisColor, DefaultZAxisColor };
//
//        /// <summary>
//        /// This is the color that is used to draw the selected multi-axis square.
//        /// </summary>
//        [SerializeField]
//        private Color _selectedMultiAxisSquareColor = new Color(DefaultSelectedAxisColor.r, DefaultSelectedAxisColor.g, DefaultSelectedAxisColor.b, 0.2f);
//
//        /// <summary>
//        /// This is the color of the selected multi-axis square lines.
//        /// </summary>
//        [SerializeField]
//        private Color _selectedMultiAxisSquareLineColor = DefaultSelectedAxisColor;
//
//        /// <summary>
//        /// Specifies whether or not arrow cones should be lit.
//        /// </summary>
//        [SerializeField]
//        private bool _areArrowConesLit = true;
//
//        /// <summary>
//        /// The client code can set this value to true when a translation along the camera
//        /// right and up vectors is needed. When this is the case, a square will be drawn
//        /// at the center of the gizmo. Clicking this square and then moving the mouse around,
//        /// the user can translate along the camera right and up vectors respectively.
//        /// </summary>
//        [SerializeField]
//        private bool _translateAlongCameraRightAndUpAxes;
//
//        /// <summary>
//        /// The client code can set this to true if they wish to use the gizmo to place objects
//        /// on surfaces.
//        /// </summary>
//        private bool _placeObjectsOnSurface;
//        private bool _alignObjectAxisToSurface = true;
//        private Axis _surfaceAlignmentAxis = Axis.Y;
//        private Dictionary<GameObject, Vector3> _objectOffsetsFromGizmo = new Dictionary<GameObject, Vector3>();
//
//        /// <summary>
//        /// This is the color that is used to draw the lines that make up the square which
//        /// appears when a special operation is performed (vertex snapping, camera axes translation etc).
//        /// </summary>
//        [SerializeField]
//        private Color _specialOpSquareColor = Color.white;
//
//        /// <summary>
//        /// Same as '_specialOpSquareColor', but it applies when the square is selected.
//        /// </summary>
//        [SerializeField]
//        private Color _specialOpSquareColorWhenSelected = Color.yellow;
//
//        private bool _isSpecialOpSquareSelected;
//
//        /// <summary>
//        /// When a special operation is enabled, this variable will be used to draw a square at the center
//        /// of the gizmo which allows the user to perform the operation. It represents the length of
//        /// the square sides in screen units (both width and height).
//        /// </summary>
//        [SerializeField]
//        private float _screenSizeOfSpecialOpSquare = 25.0f;
//
//        /// <summary>
//        /// Holds the snap settings for the translation gizmo.
//        /// </summary>
//        private TranslationGizmoSnapSettings _snapSettings = new TranslationGizmoSnapSettings();
//
//        /// <summary>
//        /// This is a vector which holds the accumulated translation along each of the 3 gizmo
//        /// axes. We will need this when snapping is enabled because it will help us detect when
//        /// we can perform a translation. 
//        /// </summary>
//        /// <remarks>
//        /// When the user is performing a translation along the camera right and up vectors, the
//        /// X component holds the amount of translation along the camera right vector while the
//        /// Y component holds the amount of translation along the camera up vector.
//        /// </remarks>
//        private Vector3 _accumulatedTranslation;
//
//        /// <summary>
//        /// The currently selected multi-axis square.
//        /// </summary>
//        private MultiAxisSquare _selectedMultiAxisSquare = MultiAxisSquare.None;
//
//        /// <summary>
//        /// This is set to true when the camera axes translation vector is selected.
//        /// </summary>
//        private bool _isCameraAxesTranslationSquareSelected;
//        #endregion
//
//        #region Public Static Properties
//        /// <summary>
//        /// Returns the minimum length of the gizmo axes.
//        /// </summary>
//        public static float MinAxisLength { get { return 0.1f; } }
//
//        /// <summary>
//        /// Returns the minimum radius for the arrow cones.
//        /// </summary>
//        public static float MinArrowConeRadius { get { return 0.1f; } }
//
//        /// <summary>
//        /// Returns the minimum value for the arrow cone length.
//        /// </summary>
//        public static float MinArrowConeLength { get { return 0.1f; } }
//
//        /// <summary>
//        /// Returns the minimum size for the multi-axis square.
//        /// </summary>
//        public static float MinMultiAxisSquareSize { get { return 0.1f; } }
//
//        /// <summary>
//        /// Returns the minimum screen size (width and height) for the square which can be used to
//        /// translate along the camera right and up axes.
//        /// </summary>
//        public static float MinScreenSizeOfCameraAxesTranslationSquare { get { return 2.0f; } }
//
//        /// <summary>
//        /// Returns the minimum screen size (width and height) for the vertex snapping square.
//        /// </summary>
//        public static float MinScreenSizeOfVertexSnappingSquare { get { return 2.0f; } }
//        #endregion
//
//        #region Public Properties
//        /// <summary>
//        /// Gets/sets the axis length. The minimum value for the axis length is given by the 'MinAxisLength'
//        /// property. Values smaller than that will be clamped accordingly.
//        /// </summary>
//        public float AxisLength { get { return _axisLength; } set { _axisLength = Mathf.Max(MinAxisLength, value); } }
//
//        /// <summary>
//        /// Gets/sets the arrow cone radius. The minimum value for the arrow cone radius is given by the
//        /// 'MinArrowConeRadius' property. Values smaller than that will be clamped accordingly.
//        /// </summary>
//        public float ArrowConeRadius
//        {
//            get { return _arrowConeRadius; }
//            set { _arrowConeRadius = Mathf.Max(MinArrowConeRadius, value); }
//        }
//
//        /// <summary>
//        /// Gets/sets the arrow cone length. The minimum value for the arrow cone length is given by the 
//        /// 'MinArrowConeLength' property. Values smaller than that will be clamped accordingly.
//        /// </summary>
//        public float ArrowConeLength
//        {
//            get { return _arrowConeLength; }
//            set { _arrowConeLength = Mathf.Max(MinArrowConeLength, value); }
//        }
//
//        /// <summary>
//        /// Gets/sets the multi-axis square size. The minimum value for the multi-axis square size is given by the
//        /// 'MinMultiAxisSquareSize'. Values smaller than that will be clamped accordingly.
//        /// </summary>
//        public float MultiAxisSquareSize
//        {
//            get { return _multiAxisSquareSize; }
//            set { _multiAxisSquareSize = Mathf.Max(MinMultiAxisSquareSize, value); }
//        }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the multi-axis squares must have their positions
//        /// adjusted for better visibility.
//        /// </summary>
//        public bool AdjustMultiAxisForBetterVisibility { get { return _adjustMultiAxisForBetterVisibility; } set { _adjustMultiAxisForBetterVisibility = value; } }
//
//        /// <summary>
//        /// Gets/sets the multi-axis square color when a square is selected.
//        /// </summary>
//        public Color SelectedMultiAxisSquareColor { get { return _selectedMultiAxisSquareColor; } set { _selectedMultiAxisSquareColor = value; } }
//
//        /// <summary>
//        /// Gets/sets the multi-axis square line color when a square is selected.
//        /// </summary>
//        public Color SelectedMultiAxisSquareLineColor { get { return _selectedMultiAxisSquareLineColor; } set { _selectedMultiAxisSquareLineColor = value; } }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the arrow cones must be lit.
//        /// </summary>
//        public bool AreArrowConesLit { get { return _areArrowConesLit; } set { _areArrowConesLit = value; } }
//
//        /// <summary>
//        /// Gets/sets the boolean flag which specifies whether or not the gizmo must translate objects
//        /// along the camera right and up axes during a translation operation.
//        /// </summary>
//        public bool TranslateAlongCameraRightAndUpAxes
//        {
//            get { return _translateAlongCameraRightAndUpAxes; }
//            set
//            {
//                // It is possible that when the client code wants to enable the camera axes translate functionality,
//                // the user is already using the gizmo to transform objects. In that case we deny the request.
//                if (value == true && IsTransformingObjects()) return;
//                _translateAlongCameraRightAndUpAxes = value;
//            }
//        }
//
//        public bool PlaceObjectsOnSurface
//        {
//            get { return _placeObjectsOnSurface; }
//            set
//            {
//                if (value == true && IsTransformingObjects()) return;
//
//                SelectAxis(GizmoAxis.None);
//                _selectedMultiAxisSquare = MultiAxisSquare.None;
//                _placeObjectsOnSurface = value;
//            }
//        }
//
//        public bool AlignObjectAxisToSurface
//        {
//            get { return _alignObjectAxisToSurface; }
//            set { _alignObjectAxisToSurface = value; }
//        }
//
//        public Axis SurfaceAlignmentAxis
//        {
//            get { return _surfaceAlignmentAxis; }
//            set { _surfaceAlignmentAxis = value; }
//        }
//
//        public float ScreenSizeOfSpecialOpSquare { get { return _screenSizeOfSpecialOpSquare; } set { _screenSizeOfSpecialOpSquare = Mathf.Max(value, MinScreenSizeOfVertexSnappingSquare); } }
//        public Color SpecialOpSquareColor { get { return _specialOpSquareColor; } set { _specialOpSquareColor = value; } }
//        public Color SpecialOpSquareColorWhenSelected { get { return _specialOpSquareColorWhenSelected; } set { _specialOpSquareColorWhenSelected = value; } }
//
//        /// <summary>
//        /// Returns the translation snap settings associated with the gizmo.
//        /// </summary>
//        public TranslationGizmoSnapSettings SnapSettings { get { return _snapSettings; } }
//        #endregion
//
//        #region Public Methods
//        /// <summary>
//        /// Returns the color of the specified multi-axis square. If 'multiAxisSquare' is
//        /// set to 'MultiAxisSquare.None', the method will return the color black.
//        /// </summary>
//        public Color GetMultiAxisSquareColor(MultiAxisSquare multiAxisSquare)
//        {
//            if (multiAxisSquare == MultiAxisSquare.None) return Color.black;
//            return _multiAxisSquareColors[(int)multiAxisSquare];
//        }
//
//        /// <summary>
//        /// Sets the color of the specified multi-axis square. If 'multiAxisSquare' is
//        /// set to 'MultiAxisSquare.None', the method will have no effect.
//        /// </summary>
//        public void SetMultiAxisSquareColor(MultiAxisSquare multiAxisSquare, Color color)
//        {
//            if (multiAxisSquare == MultiAxisSquare.None) return;
//            _multiAxisSquareColors[(int)multiAxisSquare] = color;
//        }
//
//        /// <summary>
//        /// Returns the color of the lines which surround the specified multi-axis square. 
//        /// If 'multiAxisSquare' is set to 'MultiAxisSquare.None', the method will return 
//        /// the color black.
//        /// </summary>
//        public Color GetMultiAxisSquareLineColor(MultiAxisSquare multiAxisSquare)
//        {
//            if (multiAxisSquare == MultiAxisSquare.None) return Color.black;
//            return _multiAxisSquareLineColors[(int)multiAxisSquare];
//        }
//
//        /// <summary>
//        /// Sets the color of the lines which surround the specified multi-axis square. 
//        /// If 'multiAxisSquare' is set to 'MultiAxisSquare.None', the method will have
//        /// no effect.
//        /// </summary>
//        public void SetMultiAxisSquareLineColor(MultiAxisSquare multiAxisSquare, Color color)
//        {
//            if (multiAxisSquare == MultiAxisSquare.None) return;
//            _multiAxisSquareLineColors[(int)multiAxisSquare] = color;
//        }
//
//        /// <summary>
//        /// Checks if the gizmo is ready for object manipulation.
//        /// </summary>
//        public override bool IsReadyForObjectManipulation()
//        {
//            // Return true if one of the following is true:
//            //  a) one of the translation axes; (i.e. _selectedAxis != GizmoAxis.None)
//            //  b) one of the arrow cones; (i.e. _selectedAxis != GizmoAxis.None)
//            //  c) one of the multi-axis squares; (i.e. _selectedMultiAxis != MultiAxisSquare.None)
//            //  d) the camera axes translation square; (i.e. _isCameraAxesTranslationSquareSelected)
//            //  e) if vertex snapping is enabled. (i.e. _snapSettings.IsVertexSnappingEnabled == true)
//            //  f) the gizmo is used to place objects on surface (i.e. _placeObjectsOnSurface == true)
//            return SelectedAxis != GizmoAxis.None || _selectedMultiAxisSquare != MultiAxisSquare.None ||
//                   _isCameraAxesTranslationSquareSelected || _snapSettings.IsVertexSnappingEnabled || _placeObjectsOnSurface;
//        }
//
//        /// <summary>
//        /// Returns the gizmo type.
//        /// </summary>
//        public override GizmoType GetGizmoType()
//        {
//            return GizmoType.Translation;
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
//            var selectedAxis = GizmoAxis.None;
//
//            _isSpecialOpSquareSelected = IsMouseCursorInsideSpecialOpSquare();
//            if(!_snapSettings.IsVertexSnappingEnabled && !_placeObjectsOnSurface)
//            {
//                // If the left mouse button is down, we don't want to update the selections
//                // because the user may be moving the mouse around in order to perform a
//                // translation and we don't want to deselect any axes while that happens.
//                if (_mouse.IsLeftMouseButtonDown) return;
//
//                // Reset selection information. We will be updating these in the code which follows.
//                _selectedMultiAxisSquare = MultiAxisSquare.None;
//                _isCameraAxesTranslationSquareSelected = false;
//
//                // Cache needed variables
//                float minimumDistanceFromCamera = float.MaxValue;
//                Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//                float gizmoScale = CalculateGizmoScale();
//                float cylinderRadius = 0.2f * gizmoScale;               // We will need this to check the intersection between the ray and the axis lines which we will treat as really thin cylinders.
//                Vector3 cameraPosition = _camera.transform.position;
//                Vector3 gizmoPosition = _gizmoTransform.position;
//
//                // Loop through all gizmo axis lines and identify the one which is picked by the 
//                // mouse cursor with the closest pick point to the camera position.
//                Matrix4x4[] arrowConeWorldTransforms = GetArrowConesWorldTransforms();
//                float t;
//                Vector3[] gizmoLocalAxes = GetGizmoLocalAxes();
//                Vector3 firstCylinderPoint = gizmoPosition;
//                for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
//                {
//                    // We will check the intersection between the mouse cursor and the axis line by checking
//                    // if the ray generated by the mouse cursor intersects the line's imaginary cylinder.
//                    bool axisWasPicked = false;
//                    Vector3 secondCylinderPoint = gizmoPosition + gizmoLocalAxes[axisIndex] * _axisLength * gizmoScale;
//                    if (pickRay.IntersectsCylinder(firstCylinderPoint, secondCylinderPoint, cylinderRadius, out t))
//                    {
//                        // Check if the intersection point is closer to the camera position than what we have so far
//                        Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//                        float distanceFromCamera = (intersectionPoint - cameraPosition).magnitude;
//                        if (distanceFromCamera < minimumDistanceFromCamera)
//                        {
//                            // This intersection point is closer, so update the selection
//                            minimumDistanceFromCamera = distanceFromCamera;
//                            selectedAxis = (GizmoAxis)axisIndex;
//
//                            // This axis was picked, so we don't need to check the intersection between the ray and the arrow cone. See the next 'if' statement.
//                            axisWasPicked = true;
//                        }
//                    }
//
//                    // We will also check if the mouse cursor intersects the axis arrow cone. This is done
//                    // by checking if the cursor ray intersects the cone.
//                    // Note: We only do this if the corresponding axis hasn't been selected. If it was, there is no need
//                    //       to perform this test anymore.
//                    if (!axisWasPicked && pickRay.IntersectsCone(1.0f, 1.0f, arrowConeWorldTransforms[axisIndex], out t))
//                    {
//                        // Calculate the intersection point and check if it is closer to the camera than what we have so far
//                        Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//                        float distanceFromCamera = (intersectionPoint - cameraPosition).magnitude;
//                        if (distanceFromCamera < minimumDistanceFromCamera)
//                        {
//                            // This intersection point is closer, so update the selection
//                            minimumDistanceFromCamera = distanceFromCamera;
//                            selectedAxis = (GizmoAxis)axisIndex;
//                        }
//                    }
//                }
//
//                // Now check the intersection between the ray and multi-axis squares. 
//                // Note: We only perform this step if the camera axes translation square is not active.
//                if (!_translateAlongCameraRightAndUpAxes)
//                {
//                    Vector3[] squarePlaneNormals = new Vector3[] { _gizmoTransform.forward, _gizmoTransform.up, _gizmoTransform.right };
//                    float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
//                    float gizmoScaleSign = Mathf.Sign(gizmoScale);
//
//                    // When checking if the mouse cursor ray intersects the plan of a square, we will also need
//                    // to check if the intersection point lies within the square bounds. We will need this array
//                    // for that purpose. The array stores the gizmo local axes that are used to construct the squares
//                    // in pairs of 2.
//                    Vector3[] axesUsedForProjection = new Vector3[]
//                    {
//                        _gizmoTransform.right * signs[0], _gizmoTransform.up * signs[1],            // Axes for the XY square 
//                        _gizmoTransform.right * signs[0], _gizmoTransform.forward * signs[2],       // Axes for the XZ square 
//                        _gizmoTransform.up * signs[1], _gizmoTransform.forward * signs[2]           // Axes for the YZ square 
//                    };
//                    for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//                    {
//                        // Check if the ray intersect's the square's plane
//                        Plane squarePlane = new Plane(squarePlaneNormals[multiAxisIndex], _gizmoTransform.position);
//                        if (squarePlane.Raycast(pickRay, out t))
//                        {
//                            // Calculate the intersection point
//                            Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//
//                            // We will need to check if the intersection point lies within the area of the square. The first step is
//                            // to construct a vector which goes from the gizmo position to the intersection point. We then project
//                            // this point onto the current axes pair and store the results in 2 variables: 'projectionOnFirstAxis'
//                            // and 'projectionOnSecondAxis'.
//                            Vector3 fromGizmoOriginToIntersectPoint = intersectionPoint - _gizmoTransform.position;
//                            float projectionOnFirstAxis = Vector3.Dot(fromGizmoOriginToIntersectPoint, axesUsedForProjection[multiAxisIndex * 2]) * gizmoScaleSign;
//                            float projectionOnSecondAxis = Vector3.Dot(fromGizmoOriginToIntersectPoint, axesUsedForProjection[multiAxisIndex * 2 + 1]) * gizmoScaleSign;
//
//                            // The ray intersects the square's plane but we must make sure that the intersection point happens
//                            // inside the area of the square. This only happens when the projection value on both axes is 
//                            // greater than 0 (i.e. heads in the same direction of the corresponding axis) and the length
//                            // of the projection is samller than the square size.
//                            if (projectionOnFirstAxis >= 0.0f && projectionOnFirstAxis <= (_multiAxisSquareSize * Mathf.Abs(gizmoScale)) &&
//                                projectionOnSecondAxis >= 0.0f && projectionOnSecondAxis <= (_multiAxisSquareSize * Mathf.Abs(gizmoScale)))
//                            {
//                                // The intersection point is inside the square. Now we need to check if it is closer to the camera
//                                // position than what we have so far.
//                                float distanceFromCamera = (intersectionPoint - cameraPosition).magnitude;
//                                if (distanceFromCamera < minimumDistanceFromCamera)
//                                {
//                                    // We have a new selection
//                                    minimumDistanceFromCamera = distanceFromCamera;
//                                    _selectedMultiAxisSquare = (MultiAxisSquare)multiAxisIndex;
//
//                                    // When a multi-axis square is selected, we will deselect any previosuly selected axis.
//                                    selectedAxis = GizmoAxis.None;
//                                }
//                            }
//                        }
//                    }
//                }
//
//                // Check if the camera axes translation square is selected. We only do this if the square is hovered by the mouse cursor.
//                if (_translateAlongCameraRightAndUpAxes && IsMouseCursorInsideSpecialOpSquare())
//                {
//                    // The square is selected
//                    _isCameraAxesTranslationSquareSelected = true;
//
//                    // We will disable any other selection when the square is selected
//                    selectedAxis = GizmoAxis.None;
//                    _selectedMultiAxisSquare = MultiAxisSquare.None;
//                }
//            }
//            else
//            // If the left mouse button is not down, we will select a vertex which can be used as the source position
//            // for snapping.
//            if(!_mouse.IsLeftMouseButtonDown && !_placeObjectsOnSurface)
//            {
//                // Make sure that any selection information is reset when vertex snapping is enabled.
//                selectedAxis = GizmoAxis.None;
//                _selectedMultiAxisSquare = MultiAxisSquare.None;
//                _isCameraAxesTranslationSquareSelected = false;
//
//                // When vertex snapping is enabled, we will set the position of the gizmo to the vertex which is closest
//                // to the mouse cursor in screen space. First, we will retrieve the objects whose vertices are good candidates
//                // for modifying the position of the gizmo. These are the objects whose screen rectangles intersect the 
//                // mouse cursor or, if no such objects exist, the object whose screen rectangle is closest to the mouse cursor.
//                List<GameObject> objectsForSourceVertexSelection = GetObjectsForClosestVertexSelection(ControlledObjects);
//                if(objectsForSourceVertexSelection.Count != 0)
//                {
//                    // Use the identified game objects to retrieve the world position of the vertex used to establish the new gizmo position
//                    _gizmoTransform.position = GetWorldPositionClosestToMouseCursorForVertexSnapping(objectsForSourceVertexSelection, false);
//                }
//            }
//            else
//            if (_placeObjectsOnSurface)
//            {
//                selectedAxis = GizmoAxis.None;
//                _selectedMultiAxisSquare = MultiAxisSquare.None;
//            }
//
//            SelectAxis(selectedAxis);
//        }
//
//        /// <summary>
//        /// When vertex snapping is enabled, this method will be called to gather a list
//        /// of game objects which the user can use to select a vertex that is closest to
//        /// the mouse cursor. This will be necessary when the user chooses a source vertex
//        /// and a destination vertex for snapping.
//        /// </summary>
//        private List<GameObject> GetObjectsForClosestVertexSelection(IEnumerable<GameObject> gameObjects, bool onlyHoveredByCursor = false)
//        {
//            if (gameObjects == null) return new List<GameObject>();
//
//            // Cache needed data
//            Vector2 mousePosition = Input.mousePosition;
//            float minDistanceToCursor = float.MaxValue;
//            GameObject objectWithClosestRect = null;
//
//            // Loop through all game objects and check which object has its screen rectangle
//            // intersected by the mouse cursor. Those objects will be added to the output list.
//            // Note: When no such objects are found, we will return the object whose screen
//            //       rectangle is closest to the mouse cursor.
//            Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//            var intersectedGameObjects = new List<GameObject>();
//            foreach (GameObject gameObject in gameObjects)
//            {
//                if (gameObject.GetMesh() == null && gameObject.GetComponent<SpriteRenderer>() == null) continue;
//                if (onlyHoveredByCursor)
//                {
//                    Bounds worldAABB = gameObject.GetWorldBox().ToBounds();
//                    if (!worldAABB.IntersectRay(pickRay)) continue;
//                }
//
//                // Retrieve the game object's screen rectangle and add the object to the output list
//                // if its rectangle contains the mouse position.
//                // Note: When we find an object whose screen rectangle contains the mouse cursor
//                //       position, we will go to the next iteration of the loop because in that
//                //       case we are no longer interested in finding the closest rectangle to the
//                //       mouse cursor.
//                Rect screenRectangle = gameObject.GetScreenRectangle(_camera);
//                if (screenRectangle.Contains(mousePosition, true))
//                {
//                    intersectedGameObjects.Add(gameObject);
//                    continue;
//                }
//
//                // If no intersecting game objects have been found yet, we will perform some additional
//                // calculations that allow us to identify the object which has its screen rectangle 
//                // closest to the mouse cursor.
//                if(intersectedGameObjects.Count == 0)
//                {
//                    // Identify the rectange point closest to the mouse cursor
//                    Vector2 pointClosestToCursor = screenRectangle.GetClosestPointToPoint(mousePosition);
//
//                    // If the distance between the closest point and the mouse cursor is smaller than
//                    // the minimum found so far, we will update the minimum distance and store the
//                    // object reference.
//                    float distanceToCursor = (pointClosestToCursor - mousePosition).magnitude;
//                    if(distanceToCursor < minDistanceToCursor)
//                    {
//                        minDistanceToCursor = distanceToCursor;
//                        objectWithClosestRect = gameObject;
//                    }
//                }
//            }
//
//            // If no intersecting game objects were found and if the object with the cosest rectangle
//            // to the mouse cursor is null, it means we are dealing with an empty input object collection.
//            // In this case, we will return an empty list.
//            if (intersectedGameObjects.Count == 0 && objectWithClosestRect == null) return new List<GameObject>();
//
//            // If no object rectangles contain the mouse cursor, we will just return the game objects
//            // whose screen rectangle is closest to the mouse cursor.
//            if (intersectedGameObjects.Count == 0) intersectedGameObjects.Add(objectWithClosestRect);
//
//            return intersectedGameObjects;
//        }
//
//        /// <summary>
//        /// Given a list of game objects, the method returns the world space position which is closest
//        /// to the mouse cursor for the purpose of vertex snapping. The world position represents the
//        /// world position of a vertex in one of the objects' meshes. This is the vertex whose world
//        /// position is closest to the mouse cursor in screen space. If there are no mesh objects present
//        /// in the object list, the method will return the zero vector if 'considerOnlyMeshObjects' is set
//        /// to true. If set to false, and if there are no mesh objects in the list, the method will return
//        /// the closest world space position of a game object to the cursor position in screen space.
//        /// If the object list is empty, the method will return the zero vector.
//        /// </summary>
//        private Vector3 GetWorldPositionClosestToMouseCursorForVertexSnapping(List<GameObject> gameObjects, bool considerOnlyMeshAndSpriteObjects)
//        {
//            // Store the mouse position for easy access
//            Vector2 mousePosition = Input.mousePosition;
//
//            // First, check if there are any objects that have a mesh. These have priority. If at least
//            // one object is present, we will use that to establish the world position which must be returned
//            // to the caller.
//            List<GameObject> objectsWithMesh = gameObjects.FindAll(item => item.GetMesh() != null);
//            List<GameObject> objectsWithSpriteRenderer = gameObjects.FindAll(item => item.GetComponent<SpriteRenderer>() != null);
//            if(objectsWithMesh.Count != 0)
//            {
//                // Store the mesh vertex group mappings for easy access
//                MeshVertexGroupMappings meshVertexGroupMappings = MeshVertexGroupMappings.Instance;
//
//                // We will need to map each game object to the list of vertex groups of the mesh attached to the object
//                var gameObjectToVertexGroups = new Dictionary<GameObject, List<MeshVertexGroup>>();
//                foreach(GameObject gameObject in gameObjects)
//                {
//                    // If the object's mesh contains a mapping, add an entry in the dictionary
//                    Mesh objectMesh = gameObject.GetMesh();
//                    List<MeshVertexGroup> vertexGroups = meshVertexGroupMappings.GetMeshVertexGroups(objectMesh);
//                    if (vertexGroups.Count != 0) gameObjectToVertexGroups.Add(gameObject, vertexGroups);
//                }
//
//                // The next step is to loop through all the mesh vertex groups from all meshes and identify the
//                // world space vertex position that is closest to the mouse cursor position in screen space. We
//                // will cache all necessary variables here
//                Vector3 worldPositionClosestToCursor = Vector3.zero;        // This will be returned to the caller
//                float minVertexDistanceToCursor = float.MaxValue;           // The minimum screen distance between a vertex and the mouse cursor found so far
//                float minGroupDistanceToCursor = float.MaxValue;            // The minimum distance between a mesh vertex group and the mouse cursor
//                MeshVertexGroup groupClosestToMouseCursor = null;           // The mesh vertex group closest to the mouse cursor position
//                GameObject objectOfGroupClosestToMouseCursor = null;        // The game object that corresponds to 'groupClosestToMouseCursor
//                bool foundGroupIntersectingCursor = false;                  // This will be set to true whenever we find a mesh group whose screen rectangle intersects
//                                                                            // the mouse cursor position.
//
//                // Now we have to loop through all vertex groups and identify the vertex whose world position is closest to the
//                // mouse cursor position. The idea is to attempt to find groups whose screen rectangles intersect the mouse cursor
//                // and consider only the vertices inside those groups. If no such group is found, we will choose the group which
//                // is closest to the mouse cursor and choose the closest vertex from there.
//                foreach(var pair in gameObjectToVertexGroups)
//                {
//                    // Store data for easy access
//                    GameObject gameObject = pair.Key;
//                    List<MeshVertexGroup> meshVertexGroups = pair.Value;
//                    Matrix4x4 objectTransformMatrix = gameObject.transform.localToWorldMatrix;
//
//                    // Loop through all mesh vertex groups mapped to the current game object
//                    foreach(MeshVertexGroup meshVertexGroup in meshVertexGroups)
//                    {
//                        // Calculate the screen rectangle of the group 
//                        Bounds groupWorldSpaceAABB = meshVertexGroup.GroupAABB.Transform(objectTransformMatrix);
//                        Rect groupScreenRectangle = groupWorldSpaceAABB.GetScreenRectangle(_camera);
//
//                        // Does the group intersect the mouse cursor?
//                        if(groupScreenRectangle.Contains(mousePosition, true))
//                        {
//                            // We will set this to true to let the algorithm know that from now on, it will not
//                            // need to find the group closest to the mouse cursor because a group which intersects
//                            // the cursor has already been found. 
//                            foundGroupIntersectingCursor = true;
//
//                            // Loop through all model space vertices which reside inside this group
//                            List<Vector3> modelSpaceVertices = meshVertexGroup.ModelSpaceVertices;
//                            foreach(Vector3 modelSpaceVertex in modelSpaceVertices)
//                            {
//                                // Transform the vertex and world and screen space and store the results
//                                Vector3 worldSpaceVertexPosition = objectTransformMatrix.MultiplyPoint(modelSpaceVertex);
//                                Vector2 screenSpaceVertexPosition = _camera.WorldToScreenPoint(worldSpaceVertexPosition);
//
//                                // Check if the vertex's screen space position is closer to the mouse cursor position
//                                // than what we have found so far. If it is, store the new minimum distance and also
//                                // store the world space vertex position which we will return to the caller.
//                                float distanceToCursor = (screenSpaceVertexPosition - mousePosition).magnitude;
//                                if(distanceToCursor < minVertexDistanceToCursor)
//                                {
//                                    minVertexDistanceToCursor = distanceToCursor;
//                                    worldPositionClosestToCursor = worldSpaceVertexPosition;
//                                }
//                            }
//                        }
//
//                        // If no intersecting group was found, we will update the variables which hold information about
//                        // the group which is closest to the mouse cursor. At the end of the algorithm, if no group which
//                        // intersects the mouse cursor has been found, the closest group to the cursor will be used to 
//                        // identify the world space vertex closest to the mouse cursor in screen space.
//                        if(!foundGroupIntersectingCursor)
//                        {
//                            // Identify the group's rectangle point closest to the mouse cursor
//                            Vector2 pointClosestToRect = groupScreenRectangle.GetClosestPointToPoint(mousePosition);
//
//                            // If the point is closer than what we have so far, we will update the variables
//                            float distanceToCursor = (pointClosestToRect - mousePosition).magnitude;
//                            if (distanceToCursor < minGroupDistanceToCursor)
//                            {
//                                minGroupDistanceToCursor = distanceToCursor;
//                                groupClosestToMouseCursor = meshVertexGroup;
//                                objectOfGroupClosestToMouseCursor = gameObject;
//                            }
//                        }
//                    }
//                }
//
//                // If no vertex group was found to intersect the mouse cursor, we will use the vertex group 
//                // which is closest to the mouse cursor to identify the closest world space vertex position.
//                if (!foundGroupIntersectingCursor && groupClosestToMouseCursor != null)
//                {
//                    // Cache needed data
//                    minVertexDistanceToCursor = float.MaxValue;
//                    Matrix4x4 objectTransformMatrix = objectOfGroupClosestToMouseCursor.transform.localToWorldMatrix;
//                    List<Vector3> modelSpaceVertices = groupClosestToMouseCursor.ModelSpaceVertices;
//
//                    // Loop through all model space vertices which reside inside the group
//                    foreach (Vector3 modelSpaceVertex in modelSpaceVertices)
//                    {
//                        // Transform the vertex and world and screen space and store the results
//                        Vector3 worldSpaceVertexPosition = objectTransformMatrix.MultiplyPoint(modelSpaceVertex);
//                        Vector2 screenSpaceVertexPosition = _camera.WorldToScreenPoint(worldSpaceVertexPosition);
//
//                        // Check if the vertex's screen space position is closer to the mouse cursor position
//                        // than what we have found so far. If it is, store the new minimum distance and also
//                        // store the world space vertex position which we will return to the caller.
//                        float distanceToCursor = (screenSpaceVertexPosition - mousePosition).magnitude;
//                        if (distanceToCursor < minVertexDistanceToCursor)
//                        {
//                            minVertexDistanceToCursor = distanceToCursor;
//                            worldPositionClosestToCursor = worldSpaceVertexPosition;
//                        }
//                    }
//                }
//
//                // Return the world space position to the caller
//                return worldPositionClosestToCursor;
//            }
//            else
//            if(objectsWithSpriteRenderer.Count != 0)
//            {
//                Vector3 worldPositionClosestToCursor = Vector3.zero;
//                float minDistanceToCursor = float.MaxValue;
//
//                foreach(GameObject spriteObject in objectsWithSpriteRenderer)
//                {
//                    SpriteRenderer spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
//                    if (spriteRenderer.sprite == null) continue;
//
//                    List<Vector3> worldCenterAndCornerPoints = spriteRenderer.GetWorldCenterAndCornerPoints();                  
//                    foreach (var pt in worldCenterAndCornerPoints)
//                    {
//                        Vector2 screenPosition = _camera.WorldToScreenPoint(pt);
//                        float distanceToCursor = (screenPosition - mousePosition).magnitude;
//
//                        if (distanceToCursor < minDistanceToCursor)
//                        {
//                            minDistanceToCursor = distanceToCursor;
//                            worldPositionClosestToCursor = pt;
//                        }
//                    }
//                }
//
//                return worldPositionClosestToCursor;
//            }
//            else
//            if (!considerOnlyMeshAndSpriteObjects)
//            {
//                // When there are no mesh objects in the specified list and 'considerOnlyMeshObjects' is false,
//                // we will loop through all game objects's positions and return the position which is closest
//                // to the mouse cursor position in screen space.
//                Vector3 worldPositionClosestToCursor = Vector3.zero;
//                float minDistanceToCursor = float.MaxValue;
//
//                // Loop through each game object
//                foreach(GameObject gameObject in gameObjects)
//                {
//                    // Calculate the object's screen position
//                    Vector3 objectPosition = gameObject.transform.position;
//                    Vector2 screenPosition = _camera.WorldToScreenPoint(objectPosition);
//
//                    // If the position is closer to the mouse cursor than what we have so far, we will update the variables
//                    float distanceToCursor = (screenPosition - mousePosition).magnitude;
//                    if(distanceToCursor < minDistanceToCursor)
//                    {
//                        minDistanceToCursor = distanceToCursor;
//                        worldPositionClosestToCursor = objectPosition;
//                    }
//                }
//
//                // Return the closest position
//                return worldPositionClosestToCursor;
//            }
//
//            // If no object exists in the input list or 'considerOnlyMeshObjects' is true, but there
//            // is no mesh object in the input list, return the zero vector.
//            return Vector3.zero;
//        }
//
//        /// <summary>
//        /// Called when the left mouse button is pressed. The method is responsible for
//        /// checking which components of the gizmo were picked and perform any additional
//        /// actions like storing data which is needed while processing mouse move events.
//        /// </summary>
//        protected override void OnLeftMouseButtonDown()
//        {
//            base.OnLeftMouseButtonDown();
//      
//            // If the left mouse button is pressed, we will store the gizmo pick point so that we
//            // can use it in our 'OnMouseMoved' implementation.
//            if (_mouse.IsLeftMouseButtonDown)
//            {
//                // If there is an axis which is selected, it means the user was hovering one of the translation
//                // axes/cones or a multi-axis square when they pressed the left mouse button. In that case we want
//                // to store any necessary information that is needed when processing the next mouse move event.
//                // Note: We do the same thing if the camera axes translation vector is selected because the same
//                //       data will be needed when processing a mouse move event while that square is active.
//                if (SelectedAxis != GizmoAxis.None || _selectedMultiAxisSquare != MultiAxisSquare.None || _isCameraAxesTranslationSquareSelected)
//                {
//                    // For the next mouse move event we will need to have access to the point which
//                    // lies on the plane that contains the currently selected gizmo component.
//                    Plane coordinateSystemPlane;
//                    if (SelectedAxis != GizmoAxis.None) coordinateSystemPlane = GetCoordinateSystemPlaneFromSelectedAxis();
//                    else if (_selectedMultiAxisSquare != MultiAxisSquare.None) coordinateSystemPlane = GetPlaneFromSelectedMultiAxisSquare();
//                    else coordinateSystemPlane = GetCameraAxesTranslationSquarePlane();
//
//                    // Construct a ray using the mouse cursor position
//                    Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//
//                    // Now calculate the intersection point with the plane and store it inside '_lastGizmoPickPoint'.
//                    // We will use '_lastGizmoPickPoint' inside the 'OnMouseMoved' method to help us calculate the
//                    // amount of translation which needs to be applied to the controlled objects.
//                    float t;
//                    if (coordinateSystemPlane.Raycast(pickRay, out t)) _lastGizmoPickPoint = pickRay.origin + pickRay.direction * t;
//                }
//            }
//
//            // We will reset the accumulated translation each time the left mouse button is pressed
//            // in order to prepare for a new mouse move session.
//            _accumulatedTranslation = Vector3.zero;
//        }
//
//        protected override void OnLeftMouseButtonUp()
//        {
//            base.OnLeftMouseButtonUp();
//            _objectOffsetsFromGizmo.Clear();
//        }
//
//        /// <summary>
//        /// Called when the mouse is moved. This method will make sure that any 
//        /// necessary translation is applied to the gizmo and its controlled objects.
//        /// </summary>
//        protected override void OnMouseMoved()
//        {
//            base.OnMouseMoved();
//
//            //if (!CanAnyControlledObjectBeManipulated()) return;
//            
//            // If the left mouse button is down, we will perform a translation if something is selected
//            if (_mouse.IsLeftMouseButtonDown)
//            {
//                // Is vertex snapping enabled
//                if (_snapSettings.IsVertexSnappingEnabled)
//                {
//                    // When vertex snapping is enabled, we have to snap the gizmo and the objects that it controls.
//                    // First, we will retrieve all the game objects which are visible to the camera. After that, we
//                    // will filter this list to include only the visible objects which can not be found inside the 
//                    // controlled objects list. This is necessary because the controlled object list is only used to
//                    // establish the position of the source vertex. We will also make sure that we don't include any
//                    // game objects which are children of the controlled objects. Otherwise, strange effects can occur
//                    // when snapping the parent objects because the children will be moved along with them.
//                    List<GameObject> visibleGameObjects = _camera.GetVisibleGameObjects();
//                    if (ControlledObjects != null)
//                    {
//                        HashSet<GameObject> controlledObjects = new HashSet<GameObject>(ControlledObjects);
//                        List<GameObject> parents = GameObjectExtensions.GetParentsFromObjectCollection(controlledObjects);
//
//                        // Remove all objects which:
//                        //  -are controlled objects OR
//                        //  -are children of any of the controlled objects.
//                        visibleGameObjects.RemoveAll(item => controlledObjects.Contains(item) ||
//                                                     parents.FindAll(parentItem => item.transform.IsChildOf(parentItem.transform)).Count != 0);
//                    }
//
//                    // Retrieve the game objects which can be used for snapping. If no objects are available, we will snap to grid.
//                    List<GameObject> objectsClosestToCursor = GetObjectsForClosestVertexSelection(visibleGameObjects, true);
//                    if (objectsClosestToCursor.Count != 0)
//                    {
//                        // Retrieve the snap destination point and calculate the translation vector.
//                        // Note: We pass true as the last parameter to 'GetWorldPositionClosestToMouseCursor' because we
//                        //       want to consider only objects which have a mesh assigned to them. This seems the way in
//                        //       which the Unity Editor works.
//                        Vector3 snapDestinationPoint = GetWorldPositionClosestToMouseCursorForVertexSnapping(objectsClosestToCursor, true);
//                        Vector3 translationVector = snapDestinationPoint - _gizmoTransform.position;
//
//                        // Modify the gizmo position and translate the game objects
//                        TranslateControlledObjects(translationVector);
//                    }
//                    else
//                    {
//                        // Snap the vertex to the grid
//                        Vector3 snapDestinationPoint = RuntimeEditorApplication.Instance.XZGrid.GetCellCornerPointClosestToMouseCursor();
//                        Vector3 translationVector = snapDestinationPoint - _gizmoTransform.position;
//
//                        // Modify the gizmo position and translate the game objects
//                        TranslateControlledObjects(translationVector);
//                    }
//                }
//                else
//                // Is there an axis/arrow cone selected?
//                if (SelectedAxis != GizmoAxis.None)
//                {
//                    // Identify the axis of movement based on the selected gizmo axis
//                    Vector3 gizmoMoveAxis;
//                    if (SelectedAxis == GizmoAxis.X) gizmoMoveAxis = _gizmoTransform.right;
//                    else if (SelectedAxis == GizmoAxis.Y) gizmoMoveAxis = _gizmoTransform.up;
//                    else gizmoMoveAxis = _gizmoTransform.forward;
//
//                    // Retrieve the plane which identifies the selected axis and also store the mouse cursor ray
//                    Plane planeFromSelectedAxis = GetCoordinateSystemPlaneFromSelectedAxis();
//                    Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//
//                    // Does the ray intersect the plane?
//                    float t;
//                    if (planeFromSelectedAxis.Raycast(pickRay, out t))
//                    {
//                        // The ray intersects the plane. In order to perform a translation, we will calculate a vector
//                        // which goes from the last gizmo pick point to the current intersection point. Projecting the
//                        // resulting vector on the gizmo move axis, we get the amount that we need to move along the axis.
//                        Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//                        Vector3 offsetVector = intersectionPoint - _lastGizmoPickPoint;
//                        float projectionOnAxis = Vector3.Dot(offsetVector, gizmoMoveAxis);
//
//                        // Store the last gizmo pick point for the next mouse move event
//                        _lastGizmoPickPoint = intersectionPoint;
//
//                        // We need to handle this differently based on whether or not step snapping is enabled
//                        if (_snapSettings.IsStepSnappingEnabled)
//                        {
//                            // Increase the accumulated translation
//                            int axisIndex = (int)SelectedAxis;
//                            _accumulatedTranslation[axisIndex] += projectionOnAxis;
//
//                            // If the accumulated translation is >= the snap step value, we can perform a translation
//                            if (Mathf.Abs(_accumulatedTranslation[axisIndex]) >= _snapSettings.StepValueInWorldUnits)
//                            {
//                                // Calculate the translation vector. In order to do that, we will first have to detect how many
//                                // snap step values have been accumulated inside '_accumulatedTranslation' and then multiply
//                                // that value by the step value and the sign of the accumulated translation in order to obtain
//                                // the amount of translation which must be applied.
//                                float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedTranslation[axisIndex] / _snapSettings.StepValueInWorldUnits)));
//                                float translationAmount = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedTranslation[axisIndex]);
//
//                                // Construct the translation vector and apply it to the gizmo and the controlled objects
//                                Vector3 translationVector = gizmoMoveAxis * translationAmount;
//
//                                TranslateControlledObjects(translationVector);
//
//                                // Ensure that the accumulated translation is adjusted accordingly. We need to take away anything that
//                                // was consumed and keep only the rest.
//                                if (_accumulatedTranslation[axisIndex] > 0.0f) _accumulatedTranslation[axisIndex] -= _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
//                                else if (_accumulatedTranslation[axisIndex] < 0.0f) _accumulatedTranslation[axisIndex] += _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
//                            }
//                        }
//                        else
//                        {
//                            // Move along the gizmo move axis by an amount equal to the projection value
//                            // Note: Positive values of 'projectionOnAxis' will translate along the positive end of the gizmo
//                            //       axis and negative values will translate along the negative end. This is what allows the
//                            //       user to translate in both directions using the same axis.
//                            Vector3 translationVector = gizmoMoveAxis * projectionOnAxis;
//
//                            // Update the gizmo position and make sure all controlled objects are translated accordingly
//                            TranslateControlledObjects(translationVector);
//                        }
//                    }
//                }
//                else
//                // Is there a multi-axis square selected?
//                if (_selectedMultiAxisSquare != MultiAxisSquare.None)
//                {
//                    // A multi-axis square is selected, which means that we need to translate along 2 axes at once. First,
//                    // identify the move axes which correspond to the selected multi-axis square.
//                    // Note: We will need the multi-axis squares extension signs because the position of the squares might
//                    //       be different each frame if '_adjustMultiAxisForBetterVisibility' is set to true.
//                    float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
//                    Vector3 gizmoMoveAxis1, gizmoMoveAxis2;
//                    int indexOfFirstMoveAxis, indexOfSecondMoveAxis;
//                    if (_selectedMultiAxisSquare == MultiAxisSquare.XY)
//                    {
//                        gizmoMoveAxis1 = _gizmoTransform.right * signs[0];
//                        gizmoMoveAxis2 = _gizmoTransform.up * signs[1];
//
//                        indexOfFirstMoveAxis = 0;
//                        indexOfSecondMoveAxis = 1;
//                    }
//                    else if (_selectedMultiAxisSquare == MultiAxisSquare.XZ)
//                    {
//                        gizmoMoveAxis1 = _gizmoTransform.right * signs[0];
//                        gizmoMoveAxis2 = _gizmoTransform.forward * signs[2];
//
//                        indexOfFirstMoveAxis = 0;
//                        indexOfSecondMoveAxis = 2;
//                    }
//                    else
//                    {
//                        gizmoMoveAxis1 = _gizmoTransform.up * signs[1];
//                        gizmoMoveAxis2 = _gizmoTransform.forward * signs[2];
//
//                        indexOfFirstMoveAxis = 1;
//                        indexOfSecondMoveAxis = 2;
//                    }
//
//                    // Construct a plane which identifies the multi-axis square and also store the mouse cursor pick ray
//                    Plane planeFromSelectedMultiAxisSquare = GetPlaneFromSelectedMultiAxisSquare();
//                    Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//
//                    // Does the ray intersect the plane?
//                    float t;
//                    if (planeFromSelectedMultiAxisSquare.Raycast(pickRay, out t))
//                    {
//                        // The ray intersects the plane. In order to perform the translation, we will calculate an offset vector which
//                        // goes from the last gizmo pick point to the intersection point. We will then project this vector on the identified
//                        // gizmo move axes and the 2 projection values tell us how much we have to move aong each of these axes.
//                        Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//                        Vector3 offsetVector = intersectionPoint - _lastGizmoPickPoint;
//
//                        // Calculate the projection values for the 2 axes
//                        float projectionOnFirstAxis = Vector3.Dot(offsetVector, gizmoMoveAxis1);
//                        float projectionOnSecondAxis = Vector3.Dot(offsetVector, gizmoMoveAxis2);
//
//                        // Store the last gizmo pick point for the next mouse move event
//                        _lastGizmoPickPoint = intersectionPoint;
//
//                        // We need to handle this differently based on whether or not step snapping is enabled
//                        if (_snapSettings.IsStepSnappingEnabled)
//                        {
//                            // We will follow the same procedure as when handling snapping for a single axis. Only this time, we
//                            // are dealing with 2 axes at once.
//                            _accumulatedTranslation[indexOfFirstMoveAxis] += projectionOnFirstAxis;
//                            _accumulatedTranslation[indexOfSecondMoveAxis] += projectionOnSecondAxis;
//
//                            // If the accumulated translation is >= the snap step value, we can perform a translation
//                            // Handle the first move axis
//                            Vector3 translationVector = Vector3.zero;
//                            if (Mathf.Abs(_accumulatedTranslation[indexOfFirstMoveAxis]) >= _snapSettings.StepValueInWorldUnits)
//                            {
//                                float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedTranslation[indexOfFirstMoveAxis] / _snapSettings.StepValueInWorldUnits)));
//                                float translationAmount = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedTranslation[indexOfFirstMoveAxis]);
//
//                                translationVector += gizmoMoveAxis1 * translationAmount;
//
//                                if (_accumulatedTranslation[indexOfFirstMoveAxis] > 0.0f) _accumulatedTranslation[indexOfFirstMoveAxis] -= _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
//                                else if (_accumulatedTranslation[indexOfFirstMoveAxis] < 0.0f) _accumulatedTranslation[indexOfFirstMoveAxis] += _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
//                            }
//
//                            // Handle the second move axis
//                            if (Mathf.Abs(_accumulatedTranslation[indexOfSecondMoveAxis]) >= _snapSettings.StepValueInWorldUnits)
//                            {
//                                float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedTranslation[indexOfSecondMoveAxis] / _snapSettings.StepValueInWorldUnits)));
//                                float translationAmount = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedTranslation[indexOfSecondMoveAxis]);
//
//                                translationVector += gizmoMoveAxis2 * translationAmount;
//
//                                if (_accumulatedTranslation[indexOfSecondMoveAxis] > 0.0f) _accumulatedTranslation[indexOfSecondMoveAxis] -= _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
//                                else if (_accumulatedTranslation[indexOfSecondMoveAxis] < 0.0f) _accumulatedTranslation[indexOfSecondMoveAxis] += _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
//                            }
//
//                            // Apply the translation value
//                            _gizmoTransform.position += translationVector;
//                            TranslateControlledObjects(translationVector);
//                        }
//                        else
//                        {
//                            // Use the calculated projection values and gizmo move axes to move by the necessary amount
//                            Vector3 translationVector = projectionOnFirstAxis * gizmoMoveAxis1 + projectionOnSecondAxis * gizmoMoveAxis2;
//
//                            // Update the gizmo position and make sure all controlled objects are translated accordingly
//                            _gizmoTransform.position += translationVector;
//                            TranslateControlledObjects(translationVector);
//                        }
//                    }
//                }
//                else
//                // Is the camera axes translation square selected and does the user want to translate along the camera axes?
//                if (_translateAlongCameraRightAndUpAxes && _isCameraAxesTranslationSquareSelected)
//                {
//                    // Construct a plane which identifies the camera axes translation square square and also store the mouse cursor pick ray
//                    Plane squarePlane = GetCameraAxesTranslationSquarePlane();
//                    Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//
//                    // Does the ray intersect the plane?
//                    float t;
//                    if (squarePlane.Raycast(pickRay, out t))
//                    {
//                        // The ray intersects the plane. In order to perform a translation, we will calculate a vector
//                        // which goes from the last gizmo pick point to the current intersection point. Projecting the
//                        // resulting vector on the camera right and up axes, we get the amount by which we must translate
//                        // along those axes.
//                        Vector3 intersectionPoint = pickRay.origin + pickRay.direction * t;
//                        Vector3 offsetVector = intersectionPoint - _lastGizmoPickPoint;
//                        float projectionOnCameraRightVector = Vector3.Dot(offsetVector, _camera.transform.right);
//                        float projectionOnCameraUpvector = Vector3.Dot(offsetVector, _camera.transform.up);
//
//                        // Store the last gizmo pick point so that we can use it for the next mouse move event
//                        _lastGizmoPickPoint = intersectionPoint;
//
//                        // We need to handle this differently based on whether or not step snapping is enabled
//                        if (_snapSettings.IsStepSnappingEnabled)
//                        {
//                            // We will follow the same procedure as when handling snapping for a single axis. Only this time, we
//                            // are dealing with 2 axes at once.
//                            _accumulatedTranslation[0] += projectionOnCameraRightVector;
//                            _accumulatedTranslation[1] += projectionOnCameraUpvector;
//
//                            // If the accumulated translation is >= the snap step value, we can perform a translation
//                            // Handle the camera right axis
//                            Vector3 translationVector = Vector3.zero;
//                            if (Mathf.Abs(_accumulatedTranslation[0]) >= _snapSettings.StepValueInWorldUnits)
//                            {
//                                float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedTranslation[0] / _snapSettings.StepValueInWorldUnits)));
//                                float translationAmount = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedTranslation[0]);
//
//                                translationVector += _camera.transform.right * translationAmount;
//
//                                if (_accumulatedTranslation[0] > 0.0f) _accumulatedTranslation[0] -= _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
//                                else if (_accumulatedTranslation[0] < 0.0f) _accumulatedTranslation[0] += _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
//                            }
//
//                            // Handle the camera up axis
//                            if (Mathf.Abs(_accumulatedTranslation[1]) >= _snapSettings.StepValueInWorldUnits)
//                            {
//                                float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedTranslation[1] / _snapSettings.StepValueInWorldUnits)));
//                                float translationAmount = _snapSettings.StepValueInWorldUnits * numberOfFullSteps * Mathf.Sign(_accumulatedTranslation[1]);
//
//                                translationVector += _camera.transform.up * translationAmount;
//
//                                if (_accumulatedTranslation[1] > 0.0f) _accumulatedTranslation[1] -= _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
//                                else if (_accumulatedTranslation[1] < 0.0f) _accumulatedTranslation[1] += _snapSettings.StepValueInWorldUnits * numberOfFullSteps;
//                            }
//
//                            // Apply the translation value
//                            _gizmoTransform.position += translationVector;
//                            TranslateControlledObjects(translationVector);
//                        }
//                        else
//                        {
//                            // Construct the translation vector along the camera axes using the calculated projection values
//                            Vector3 translationVector = _camera.transform.right * projectionOnCameraRightVector + _camera.transform.up * projectionOnCameraUpvector;
//
//                            // Update the gizmo position and make sure all controlled objects are translated accordingly
//                            _gizmoTransform.position += translationVector;
//                            TranslateControlledObjects(translationVector);
//                        }
//                    }
//                }
//                else
//                if (_placeObjectsOnSurface && ControlledObjects != null && _isSpecialOpSquareSelected)
//                {
//                    if (_objectOffsetsFromGizmo.Count == 0)
//                    {
//                        List<GameObject> parents = GameObjectExtensions.GetParentsFromObjectCollection(ControlledObjects);
//                        foreach (var parent in parents) _objectOffsetsFromGizmo.Add(parent, parent.transform.position - _gizmoTransform.position);
//                    }
//
//                    RaycastHit rayHit;
//                    Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);
//                    HashSet<GameObject> controlledObjectsHash = new HashSet<GameObject>(ControlledObjects);
//                    Collider collider = PhysicsHelper.RaycastAllClosest(pickRay, out rayHit, new List<Type> { typeof(TerrainCollider) }, controlledObjectsHash);
//                    if (collider != null && collider is TerrainCollider)
//                    {
//                        Vector3 newGizmoPos = rayHit.point;
//                        TerrainCollider terrainCollider = collider as TerrainCollider;
//                        if (terrainCollider != null)
//                        {
//                            List<GameObject> topParents = GameObjectExtensions.GetParentsFromObjectCollection(ControlledObjects);
//                            if (topParents.Count != 0)
//                            {
//                                RaycastHit terrainHit;
//                                foreach (var parent in topParents)
//                                {
//                                    Transform parentTransform = parent.transform;
//                                    parentTransform.position = newGizmoPos + _objectOffsetsFromGizmo[parent];
//                                    
//                                    Ray terrainRay = new Ray(parentTransform.position, -Vector3.up);
//                                    if (terrainCollider.RaycastReverseIfFail(terrainRay, out terrainHit))
//                                    {
//                                        parent.PlaceHierarchyOnPlane(terrainHit.point, terrainHit.normal, AlignObjectAxisToSurface ? (int)SurfaceAlignmentAxis : -1);
//                                        _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
//                                    }
//                                }
//                                _gizmoTransform.position = newGizmoPos;
//                            }
//                        }
//                    }
//                    else
//                    {
//                        Plane xzGridPlane = RuntimeEditorApplication.Instance.XZGrid.Plane;
//                        float t;
//                        if(xzGridPlane.Raycast(pickRay, out t))
//                        {
//                            Vector3 hitPoint = pickRay.GetPoint(t);
//                            Vector3 newGizmoPos = hitPoint;
//
//                            List<GameObject> topParents = GameObjectExtensions.GetParentsFromObjectCollection(ControlledObjects);
//                            if (topParents.Count != 0)
//                            {
//                                foreach (var parent in topParents)
//                                {
//                                    Transform parentTransform = parent.transform;
//                                    parentTransform.position = newGizmoPos + _objectOffsetsFromGizmo[parent];
//
//                                    float distFromPlane = xzGridPlane.GetDistanceToPoint(parentTransform.position);
//                                    Vector3 projectedPos = parentTransform.position - distFromPlane * xzGridPlane.normal;
//
//                                    parent.PlaceHierarchyOnPlane(projectedPos, xzGridPlane.normal, AlignObjectAxisToSurface ? (int)SurfaceAlignmentAxis : -1);
//                                    _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
//                                }
//                                _gizmoTransform.position = newGizmoPos;
//                            }
//                        }
//                    }
//                }
//            }
//        }
//
//        /// <summary>
//        /// This method is called after the camera has finished rendering the scene. 
//        /// It allows us to perform any necessary drawing.
//        /// </summary>
//        protected override void OnRenderObject()
//        {
//            //if (Camera.current != EditorCamera.Instance.Camera) return;
//
//            base.OnRenderObject();
//            float gizmoScale = CalculateGizmoScale();
//
//            // Draw the arrow cones
//            Matrix4x4[] arrowConeWorldTransforms = GetArrowConesWorldTransforms();
//            DrawArrowCones(arrowConeWorldTransforms);
//
//            // Draw the multi-axis squares.
//            // Note: We only draw them if the camera axes translation square is not active and if vertex snapping is not enabled.
////            if (!_translateAlongCameraRightAndUpAxes && !_snapSettings.IsVertexSnappingEnabled && !_placeObjectsOnSurface)
////            {
////                Matrix4x4[] multiAxisWorldTransforms = GetMultiAxisSquaresWorldTransforms();
////                DrawMultiAxisSquares(multiAxisWorldTransforms);
////            }
//
//            // Before we can draw the axes, we will make sure that we draw them in the correct
//            // order. We will retrieve the axis index array by calling 'GetSortedGizmoAxesIndices'.
//            int[] axisIndices = GetSortedGizmoAxesIndices();
//
//            // Now we can start drawing the axis lines
//            Vector3[] gizmoLocalAxes = GetGizmoLocalAxes();
//            Vector3 startPoint = _gizmoTransform.position;
//            foreach (int axisIndex in axisIndices)
//            {
//                // Establish the axis color and its end point
//                Color axisColor = SelectedAxis == (GizmoAxis)axisIndex ? _selectedAxisColor : _axesColors[axisIndex];
//                Vector3 endPoint = startPoint + gizmoLocalAxes[axisIndex] * _axisLength * gizmoScale;
//
//                // Make sure the stencil reference values are updated accordingly
//                UpdateShaderStencilRefValuesForGizmoAxisLineDraw(axisIndex, startPoint, endPoint, gizmoScale);
//
//                // Draw the axis line
//                GLPrimitives.Draw3DLine(startPoint, endPoint, axisColor, MaterialPool.Instance.GizmoLine);
//            }
//
//            // Now draw the multi-axis square lines that surround the multi-axis squares.
////            MaterialPool.Instance.GizmoLine.SetInt("_StencilRefValue", _doNotUseStencil);
////            if (!_translateAlongCameraRightAndUpAxes && !_snapSettings.IsVertexSnappingEnabled && !_placeObjectsOnSurface)
////            {
////                // Retrieve the points and colors which must be used to draw the multi-axis squares
////                Vector3[] squareLinesPoints;
////                Color[] squareLinesColors;
////                GetMultiAxisSquaresLinePointsAndColors(gizmoScale, out squareLinesPoints, out squareLinesColors);
////
////                // Draw the square lines
////                GLPrimitives.Draw3DLines(squareLinesPoints, squareLinesColors, false, MaterialPool.Instance.GizmoLine, false, Color.black);
////            }
//
//            // Draw the special op square if necessary
//            if (_translateAlongCameraRightAndUpAxes)
//            {
//                Color squareLineColor = _isCameraAxesTranslationSquareSelected ? SpecialOpSquareColorWhenSelected : SpecialOpSquareColor;
//                GLPrimitives.Draw2DRectangleBorderLines(GetSpecialOpSquareScreenPoints(), squareLineColor, MaterialPool.Instance.GizmoLine, _camera);
//            }
//
//            if(_snapSettings.IsVertexSnappingEnabled)
//            {
//                Color squareLineColor = _mouse.IsLeftMouseButtonDown ? _specialOpSquareColorWhenSelected : _specialOpSquareColor;
//                GLPrimitives.Draw2DRectangleBorderLines(GetSpecialOpSquareScreenPoints(), squareLineColor, MaterialPool.Instance.GizmoLine, _camera);
//            }
//
//            if(_placeObjectsOnSurface)
//            {
//                Color squareLineColor = _isSpecialOpSquareSelected ? _specialOpSquareColorWhenSelected : _specialOpSquareColor;
//                GLPrimitives.Draw2DRectangleBorderLines(GetSpecialOpSquareScreenPoints(), squareLineColor, MaterialPool.Instance.GizmoLine, _camera);
//            }
//        }
//        #endregion
//
//        #region Private Methods
//        /// <summary>
//        /// In order to draw the multi-axis square lines with one call to the 'GLPrimitives'
//        /// API, we will call this function to give us an array of points and colors for
//        /// the square lines. We can then use those arrays to draw the square lines using
//        /// a single call to 'GLPrimitives.Draw3DLines'.
//        /// </summary>
//        /// <param name="gizmoScale">
//        /// The gizmo scale.
//        /// </param>
//        /// <param name="squareLinesPoints">
//        /// At the end of the function call, this will hold the points which can be used
//        /// to draw the square lines.
//        /// </param>
//        /// <param name="squareLinesColors">
//        /// At the end of the function call, this will hold the colors which can be used
//        /// to draw the square lines.
//        /// </param>
//        private void GetMultiAxisSquaresLinePointsAndColors(float gizmoScale, out Vector3[] squareLinesPoints, out Color[] squareLinesColors)
//        {
//            // Establish the multi-axis line length so that we don't have to calculate it
//            // every time inside the 'for' loop. We add a small offset to the square size
//            // in order to make sure the lines are sitting close to but not exactly on the
//            // filled multi-axis square borders.
//            float multiAxisLineLength = (_multiAxisSquareSize + 0.001f) * gizmoScale;
//
//            // Create the points and colors arrays.
//            // Note: We need 24 elements in the line points array because we have 12 lines total
//            //       and each line requires 2 vertices.
//            squareLinesPoints = new Vector3[24];
//            squareLinesColors = new Color[12];
//
//            // Retrieve the axes which will help us draw the lines and then draw
//            Vector3[] axesUsedForMultiAxisDraw = GetWorldAxesUsedToDrawMultiAxisSquareLines();
//            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//            {
//                // Establish the color for the lines in the current muli-axis square
//                Color lineColor = _selectedMultiAxisSquare == (MultiAxisSquare)multiAxisIndex ? _selectedMultiAxisSquareLineColor : _multiAxisSquareLineColors[multiAxisIndex];
//
//                // Store the color in the color array for each line which makes up the square
//                int indexOfFirstColor = multiAxisIndex * 4;
//                squareLinesColors[indexOfFirstColor] = lineColor;
//                squareLinesColors[indexOfFirstColor + 1] = lineColor;
//                squareLinesColors[indexOfFirstColor + 2] = lineColor;
//                squareLinesColors[indexOfFirstColor + 3] = lineColor;
//
//                // Calculate the points which make up the corners of the square
//                int indexOfFirstDrawAxis = multiAxisIndex * 2;
//                Vector3 firstPoint = _gizmoTransform.position;
//                Vector3 secondPoint = firstPoint + axesUsedForMultiAxisDraw[indexOfFirstDrawAxis + 1] * multiAxisLineLength;
//                Vector3 thirdPoint = secondPoint + axesUsedForMultiAxisDraw[indexOfFirstDrawAxis] * multiAxisLineLength;
//                Vector3 fourthPoint = firstPoint + axesUsedForMultiAxisDraw[indexOfFirstDrawAxis] * multiAxisLineLength;
//
//                // Store the points in the line points array.
//                // Note: We multiply by 8 to get the index of the first point in the current square because 
//                //       each square has 4 lines, each of them having 2 points.
//                int indexOfFirstPoint = multiAxisIndex * 8;
//                squareLinesPoints[indexOfFirstPoint] = firstPoint;
//                squareLinesPoints[indexOfFirstPoint + 1] = secondPoint;
//                squareLinesPoints[indexOfFirstPoint + 2] = secondPoint;
//                squareLinesPoints[indexOfFirstPoint + 3] = thirdPoint;
//                squareLinesPoints[indexOfFirstPoint + 4] = thirdPoint;
//                squareLinesPoints[indexOfFirstPoint + 5] = fourthPoint;
//                squareLinesPoints[indexOfFirstPoint + 6] = fourthPoint;
//                squareLinesPoints[indexOfFirstPoint + 7] = firstPoint;
//            }
//        }
//
//        /// <summary>
//        /// Returns the screen space points which form the special op square.
//        /// </summary>
//        private Vector2[] GetSpecialOpSquareScreenPoints()
//        {
//            Vector2 screenSpaceSquareCenter = _camera.WorldToScreenPoint(_gizmoTransform.position);
//            float halfSquareSize = ScreenSizeOfSpecialOpSquare * 0.5f;
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
//        /// of the special op square.
//        /// </summary>
//        private bool IsMouseCursorInsideSpecialOpSquare()
//        {
//            // We will need this to perform the check
//            Vector2 screenSpaceSquareCenter = _camera.WorldToScreenPoint(_gizmoTransform.position);
//            float halfSquareSize = ScreenSizeOfSpecialOpSquare * 0.5f;
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
//        /// Draws the multi-axis squares using the specified world transforms.
//        /// </summary>
//        private void DrawMultiAxisSquares(Matrix4x4[] worldTransformMatrices)
//        {
//            Material material = MaterialPool.Instance.GizmoSolidComponent;
//            material.SetVector("_LightDir", _camera.transform.forward);
//            material.SetInt("_IsLit", 0);
//            int cullMode = material.GetInt("_CullMode");
//            material.SetInt("_CullMode", 0);
//
//            // Loop through each multi-axis square and draw it using the corresponding world transform
//            Mesh squareMesh = MeshPool.Instance.XYSquareMesh;
//            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//            {
//                Color multiAxisColor = multiAxisIndex == (int)_selectedMultiAxisSquare ? _selectedMultiAxisSquareColor : _multiAxisSquareColors[multiAxisIndex];
//                material.SetColor("_Color", multiAxisColor);
//                material.SetPass(0);
//                Graphics.DrawMeshNow(squareMesh, worldTransformMatrices[multiAxisIndex]);
//            }
//            material.SetInt("_CullMode", cullMode);
//        }
//
//        /// <summary>
//        /// Retrieves the world transform matrices that store the world transform information
//        /// for the multi-axis squares.
//        /// </summary>
//        private Matrix4x4[] GetMultiAxisSquaresWorldTransforms()
//        {
//            Matrix4x4[] worldTransforms = new Matrix4x4[3];
//            Vector3[] localPositions = GetMultiAxisSquaresGizmoLocalPositions();
//            Quaternion[] localRotations = GetMultiAxisSquaresGizmoLocalRotations();
//
//            // Loop through each axis
//            float gizmoScale = CalculateGizmoScale();
//            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//            {
//                // Transform the local position and orientation in world space.
//                // Note: The local position was generated along the local axes of the gizmo, so in a way, the position
//                //       is tied to the axes of the gizmo object. For this reason, we will need to rotate the local position
//                //       using the gizmo's rotation to bring it in world space with respect to the orientation of the axes.
//                //       We then add the position of the gizmo object to the result to get the final world space position.
//                Vector3 worldPosition = _gizmoTransform.position + _gizmoTransform.rotation * localPositions[multiAxisIndex] * gizmoScale;
//                Quaternion worldRotation = _gizmoTransform.rotation * localRotations[multiAxisIndex];
//
//                // Build the world matrix
//                worldTransforms[multiAxisIndex] = new Matrix4x4();
//                worldTransforms[multiAxisIndex].SetTRS(worldPosition, worldRotation, Vector3.Scale(_gizmoTransform.lossyScale, new Vector3(_multiAxisSquareSize, _multiAxisSquareSize, 1.0f)));
//            }
//
//            return worldTransforms;
//        }
//
//        /// <summary>
//        /// Retrieves an array of quaternions which represent the gizmo local rotations of the
//        /// multi-axis squares.
//        /// </summary>
//        private Quaternion[] GetMultiAxisSquaresGizmoLocalRotations()
//        {
//            return new Quaternion[]
//            {
//                Quaternion.identity,
//                Quaternion.Euler(90.0f, 0.0f, 0.0f),
//                Quaternion.Euler(0.0f, 90.0f, 0.0f)
//            };
//        }
//
//        /// <summary>
//        /// Returns an array of 'Vector3' instances which holds the gizmo local positions of each 
//        /// of the multi-axis squares. 
//        /// </summary>
//        private Vector3[] GetMultiAxisSquaresGizmoLocalPositions()
//        {
//            // The multi-axis squares are positioned by offsetting from the gzimo origin by half the square size on 
//            // both axes (XY, XZ, or YZ). However, we also need to know the direction in which this offset must be 
//            // applied. If '_adjustMultiAxisForBetterVisibility' is set to true, this offset will differ based on the
//            // camera orientation. So we call 'GetMultiAxisExtensionSigns' to give us an array of sign values that
//            // will allow us to change the offset direction correctly.
//            float halfSize = _multiAxisSquareSize * 0.5f;
//            float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
//
//            // Create the array of local positions.
//            // Note: Multiply each local axis by the corresponding sign value to make sure that the squares are always
//            //       positioned correctly.
//            return new Vector3[]
//            {
//                (Vector3.right * signs[0] + Vector3.up * signs[1]) * halfSize,
//                (Vector3.right * signs[0] + Vector3.forward * signs[2]) * halfSize,
//                (Vector3.up * signs[1] + Vector3.forward * signs[2]) * halfSize
//            };
//        }
//
//        /// <summary>
//        /// Returns an array of world space axes which are used when drawing the lines that 
//        /// surround the multi-axis squares. The returned axes are normalized.
//        /// </summary>
//        private Vector3[] GetWorldAxesUsedToDrawMultiAxisSquareLines()
//        {
//            // Note: The axes are scaled by the corresponding sign value in order to make sure the
//            //       lines extend in the correct direction along each axis.
//            float[] signs = GetMultiAxisExtensionSigns(_adjustMultiAxisForBetterVisibility);
//            return new Vector3[]
//            {
//                _gizmoTransform.right * signs[0], _gizmoTransform.up * signs[1],
//                _gizmoTransform.right * signs[0], _gizmoTransform.forward * signs[2],
//                _gizmoTransform.up * signs[1], _gizmoTransform.forward * signs[2]
//            };
//        }
//
//        /// <summary>
//        /// Returns a plane instance on which the selected multi-axis square resides.
//        /// </summary>
//        private Plane GetPlaneFromSelectedMultiAxisSquare()
//        {
//            switch (_selectedMultiAxisSquare)
//            {
//                case MultiAxisSquare.XY:
//
//                    return new Plane(_gizmoTransform.forward, _gizmoTransform.position);
//
//                case MultiAxisSquare.XZ:
//
//                    return new Plane(_gizmoTransform.up, _gizmoTransform.position);
//
//                case MultiAxisSquare.YZ:
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
//        /// Returns the plane on which the camera axes translation square resides in world space.
//        /// </summary>
//        private Plane GetCameraAxesTranslationSquarePlane()
//        {
//            // The square has its center in the same position as the gizmo's position in screen space. This means
//            // that in order to construct the plane, we will specify the gizmo's position as the point which lies
//            // on the plane. The plane normal is the same as the camera forward vector because the square is drawn
//            // on the screen and the camera is always looking straight at the screen's plane.
//            return new Plane(_camera.transform.forward, _gizmoTransform.position);
//        }
//
//        /// <summary>
//        /// Translates all controlled objects using the specified translation vector.
//        /// </summary>
//        private void TranslateControlledObjects(Vector3 translationVector) {
//            InputRange.SetValue(InputRange.Value + translationVector);
//            _gizmoTransform.localPosition = InputRange.Value;
////            if (ControlledObjects != null)
////            {
////                // Retrieve the list of top parents in the controlled game object collection. The reason that
////                // we need to do this is because otherwise, if some of the controlled game objects are children
////                // of other objects in the same collection, the children would be translated twice: once because
////                // of the direct transformation that we would apply to them and a second time, indirectly, through
////                // the translation that we apply to the parent. In order to avoid this, we will only translate
////                // the parents.
////                List<GameObject> topParents = GetParentsFromControlledObjects(true);
////               
////                if(topParents.Count != 0)
////                {
////                    // Loop through all top parent objects and transform them
////                    foreach (GameObject topParent in topParents)
////                    {
////                        // Translate the object.
////                        // Note: When translating, we don't need to take the pivot point into account. Whether we
////                        //       translate the object center or the mesh pivot, the result is the same (i.e. they
////                        //       are both translated by the same amount).
////                        if (topParent != null)
////                        {
////                            topParent.transform.position += translationVector;
////
////                            // The game objects were transformed since the left mouse button was pressed
////                            _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
////                        }
////                    }
////                }
////            }
//        }
//
//        /// <summary>
//        /// Draws the arrow cones using the specified world transforms.
//        /// </summary>
//        private void DrawArrowCones(Matrix4x4[] worldTransformMatrices)
//        {
//            Material material = MaterialPool.Instance.GizmoSolidComponent;
//            material.SetVector("_LightDir", _camera.transform.forward);
//            material.SetInt("_IsLit", _areArrowConesLit ? 1 : 0);
//
//            // Loop through each arrow cone and draw it using its corresponding transform matrix
//            Mesh coneMesh = MeshPool.Instance.ConeMesh;
//            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
//            {
//                Color axisColor = axisIndex == (int)SelectedAxis ? _selectedAxisColor : _axesColors[axisIndex];
//                material.SetInt("_StencilRefValue", _axesStencilRefValues[axisIndex]);
//                material.SetColor("_Color", axisColor);
//                material.SetPass(0);
//                Graphics.DrawMeshNow(coneMesh, worldTransformMatrices[axisIndex]);
//            }
//        }
//
//        /// <summary>
//        /// Retrieves the world transform matrices which store the world transform information
//        /// for the arrow cones.
//        /// </summary>
//        private Matrix4x4[] GetArrowConesWorldTransforms()
//        {
//            Matrix4x4[] worldTransforms = new Matrix4x4[3];
//            Vector3[] localPositions = GetArrowConesGizmoLocalPositions();
//            Quaternion[] localRotations = GetArrowConesGizmoLocalRotations();
//
//            // Loop through each axis
//            float gizmoScale = CalculateGizmoScale();
//            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
//            {
//                // Transform the local position and orientation in world space.
//                // Note: The local position was generated along the local axes of the gizmo, so in a way, the position
//                //       is tied to the axes of the gizmo object. For this reason, we will need to rotate the local position
//                //       using the gizmo's rotation to bring it in world space with respect to the orientation of the axes.
//                //       We then add the position of the gizmo object to the result to get the final world space position.
//                Vector3 worldPosition = _gizmoTransform.position +_gizmoTransform.rotation * localPositions[axisIndex] * gizmoScale;
//                Quaternion worldRotation = _gizmoTransform.rotation * localRotations[axisIndex];
//
//                // Construct the world transform matrix
//                worldTransforms[axisIndex] = new Matrix4x4();
//                worldTransforms[axisIndex].SetTRS(worldPosition, worldRotation, Vector3.Scale(Vector3.one * gizmoScale, new Vector3(_arrowConeRadius, _arrowConeLength, _arrowConeRadius)));
//            }
//
//            return worldTransforms;
//        }
//
//        /// <summary>
//        /// Returns an array of vectors which represent the gizmo local positions
//        /// of the arrow cones.
//        /// </summary>
//        private Vector3[] GetArrowConesGizmoLocalPositions()
//        {
//            return new Vector3[]
//            {
//                Vector3.right * _axisLength,
//                Vector3.up * _axisLength,
//                Vector3.forward * _axisLength
//            };
//        }
//
//        /// <summary>
//        /// Returns an array of quaternions which represent the gizmo local rotations of
//        /// the arrow cones.
//        /// </summary>
//        private Quaternion[] GetArrowConesGizmoLocalRotations()
//        {
//            return new Quaternion[]
//            {
//                Quaternion.Euler(0.0f, 0.0f, -90.0f),
//                Quaternion.identity,
//                Quaternion.Euler(90.0f, 0.0f, 0.0f)
//            };
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
//            // Draw the arrow cones
//            DrawArrowConeInSceneView(GizmoAxis.X);
//            DrawArrowConeInSceneView(GizmoAxis.Y);
//            DrawArrowConeInSceneView(GizmoAxis.Z);
//
//            // Draw the XY multi-axis square
//            const float multiAxisLineOffsetScale = 1.001f;
//            float multiAxisHalfSize = _multiAxisSquareSize * 0.5f;
//            Gizmos.color = _multiAxisSquareColors[0];
//            Vector3 multiAxisLocalPosition = (Vector3.right + Vector3.up) * multiAxisHalfSize;
//            Gizmos.DrawCube(multiAxisLocalPosition, new Vector3(_multiAxisSquareSize, _multiAxisSquareSize, 0.0f));
//            Gizmos.color = _multiAxisSquareLineColors[0];
//            Gizmos.DrawLine(Vector3.up * _multiAxisSquareSize * multiAxisLineOffsetScale, (Vector3.right + Vector3.up) * _multiAxisSquareSize * multiAxisLineOffsetScale);
//            Gizmos.DrawLine(Vector3.right * _multiAxisSquareSize * multiAxisLineOffsetScale, (Vector3.right + Vector3.up) * _multiAxisSquareSize * multiAxisLineOffsetScale);
//
//            // Draw the XZ multi-axis square
//            Gizmos.color = _multiAxisSquareColors[1];
//            multiAxisLocalPosition = (Vector3.right + Vector3.forward) * multiAxisHalfSize;
//            Gizmos.DrawCube(multiAxisLocalPosition, new Vector3(_multiAxisSquareSize, 0.0f, _multiAxisSquareSize));
//            Gizmos.color = _multiAxisSquareLineColors[1];
//            Gizmos.DrawLine(Vector3.forward * _multiAxisSquareSize * multiAxisLineOffsetScale, (Vector3.forward + Vector3.right) * _multiAxisSquareSize * multiAxisLineOffsetScale);
//            Gizmos.DrawLine(Vector3.right * _multiAxisSquareSize * multiAxisLineOffsetScale, (Vector3.forward + Vector3.right) * _multiAxisSquareSize * multiAxisLineOffsetScale);
//
//            // Draw the YZ multi-axis square
//            Gizmos.color = _multiAxisSquareColors[2];
//            multiAxisLocalPosition = (Vector3.up + Vector3.forward) * multiAxisHalfSize;
//            Gizmos.DrawCube(multiAxisLocalPosition, new Vector3(0.0f, _multiAxisSquareSize, _multiAxisSquareSize));
//            Gizmos.color = _multiAxisSquareLineColors[2];
//            Gizmos.DrawLine(Vector3.up * _multiAxisSquareSize * multiAxisLineOffsetScale, (Vector3.up + Vector3.forward) * _multiAxisSquareSize * multiAxisLineOffsetScale);
//            Gizmos.DrawLine(Vector3.forward * _multiAxisSquareSize * multiAxisLineOffsetScale, (Vector3.up + Vector3.forward) * _multiAxisSquareSize * multiAxisLineOffsetScale);
//
//            // Restore the gizmos color and matrix
//            Gizmos.color = oldGizmosColor;
//            Gizmos.matrix = oldGizmosMatrix;
//        }
//
//        /// <summary>
//        /// Draws an arrow cone for the specified gizmo axis inside the scene view. The method will
//        /// draw the arrow cone using line segments between the cone's bottom cap points and then
//        /// between each cap point and the cone's top point.
//        /// </summary>
//        private void DrawArrowConeInSceneView(GizmoAxis gizmoAxis)
//        {
//            // Calculate the top point of the arrow cone based on the specified axis
//            Vector3 topConePoint;
//            if (gizmoAxis == GizmoAxis.X) topConePoint = Vector3.right * (_arrowConeLength + _axisLength);
//            else if (gizmoAxis == GizmoAxis.Y) topConePoint = Vector3.up * (_arrowConeLength + _axisLength);
//            else topConePoint = Vector3.forward * (_arrowConeLength + _axisLength);
//
//            // Retrieve the points which make up the bottom cap of the cone
//            Vector3[] coneCapPoints = GetArrowConeBottomCapPoints(gizmoAxis);
//
//            // Draw the necessary lines. We will draw lines between successive points that make up the bottom cap
//            // and we will also draw a line which goes from each point to the calculated top point.
//            Gizmos.color = _axesColors[(int)gizmoAxis];
//            for (int pointIndex = 0; pointIndex < coneCapPoints.Length; ++pointIndex)
//            {
//                Gizmos.DrawLine(coneCapPoints[pointIndex], coneCapPoints[(pointIndex + 1) % coneCapPoints.Length]);
//                Gizmos.DrawLine(coneCapPoints[pointIndex], topConePoint);
//            }
//        }
//
//        /// <summary>
//        /// Called from 'DrawArrowConeInSceneView' in order to retrieve an array of points which
//        /// make up the arrow cone's bottom cap.
//        /// </summary>
//        private Vector3[] GetArrowConeBottomCapPoints(GizmoAxis gizmoAxis)
//        {
//            // We will need to identify the rotation and offset that needs to be applied
//            // to the cap points based on the specified gizmo axis.
//            Quaternion rotationQuaternion;
//            Vector3 pointOffset;
//            if (gizmoAxis == GizmoAxis.X)
//            {
//                rotationQuaternion = Quaternion.Euler(0.0f, 0.0f, 90.0f);
//                pointOffset = Vector3.right * _axisLength;
//            }
//            else if (gizmoAxis == GizmoAxis.Y)
//            {
//                rotationQuaternion = Quaternion.identity;
//                pointOffset = Vector3.up * _axisLength;
//            }
//            else
//            {
//                rotationQuaternion = Quaternion.Euler(90.0f, 0.0f, 0.0f);
//                pointOffset = Vector3.forward * _axisLength;
//            }
//
//            // Create the points by rotating around the Y axis. After a point is generated, we will
//            // apply the calculated rotation and offset to get the final point.
//            const int numberOfBottomCapPoints = 80;
//            Vector3[] bottomCapPoints = new Vector3[numberOfBottomCapPoints];
//            float angleStep = 360.0f / numberOfBottomCapPoints;
//            for (int pointIndex = 0; pointIndex < numberOfBottomCapPoints; ++pointIndex)
//            {
//                // Create a point by rotating arounf the Y axis
//                float angleInRadians = angleStep * pointIndex * Mathf.Deg2Rad;
//                Vector3 point = new Vector3(Mathf.Cos(angleInRadians) * _arrowConeRadius, 0.0f, Mathf.Sin(angleInRadians) * _arrowConeRadius);
//
//                // Apply rotation and offset
//                point = rotationQuaternion * point;
//                point += pointOffset;
//
//                // Store the point inside the array
//                bottomCapPoints[pointIndex] = point;
//            }
//
//            return bottomCapPoints;
//        }
//        #endif
//        #endregion
//    }
//}
