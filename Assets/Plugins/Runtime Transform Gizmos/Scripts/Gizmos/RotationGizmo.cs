using UnityEngine;
using System.Collections.Generic;
using RamjetAnvil.InputModule;

namespace RTEditor
{
    /// <summary>
    /// The class implements the behaviour of a rotation gizmo. A rotation gizmo is composed of the following:
    ///     a) a central sphere -> when the user clicks the sphere and then starts moving the mouse around, the gizmo 
    ///        and its controlled objects will be rotated around the camera's right and up vectors respectively;
    ///     b) axis rotation circles -> the rotation sphere is surrounded by 3 rotation circles which can be used to 
    ///        rotate arund the corresponding circle's axis.
    ///     c) a circle which surrounds the sphere in screen space and which allows the user to perform rotations around 
    ///        the camera's look vector.
    /// Except for case a), when a rotation is performed, a rotation guide will appear to show a visual representation of 
    /// the accumulated rotation around a particular axis. This rotation guide is a disc which exists in the plane perpendicular
    /// to the axis around which the rotation is performed.
    /// </summary>
    public class RotationGizmo : Gizmo
    {
        #region Private Variables
        /// <summary>
        /// This is the radius of the rotation sphere. This is the rotation sphere that the user
        /// can click on and then move the mouse to rotate the objects around the camera's right
        /// and up vectors respectively.
        /// </summary>
        [SerializeField]
        private float _rotationSphereRadius = 3.0f;

        /// <summary>
        /// The color of the rotation sphere.
        /// </summary>
        [SerializeField]
        private Color _rotationSphereColor = new Color(0.3f, 0.3f, 0.3f, 0.12f);

        /// <summary>
        /// Specifies whether or not the rotation sphere should be affected by lighting.
        /// </summary>
        [SerializeField]
        private bool _isRotationSphereLit = true;

        /// <summary>
        /// Specifies whether or not the rotation guide must be drawn. The rotation guide
        /// is a disc which appears whenever the user is performing a rotation using one of
        /// the available rotation circles. It is a visual representation of the accumulated
        /// rotation.
        /// </summary>
        [SerializeField]
        private bool _showRotationGuide = true;

        /// <summary>
        /// When the rotation guide must be shown, 2 lines will be drawn going from the center of the
        /// rotation sphere to the rotation disc extreme points. This variable holds the color that
        /// must be used when drawing those lines.
        /// </summary>
        [SerializeField]
        private Color _rotationGuieLineColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);

        /// <summary>
        /// When the rotation guide must be shown, a disc will be drawn as an indication of the accumulated
        /// rotation. This variable holds the color that must be used to render the disc.
        /// </summary>
        [SerializeField]
        private Color _rotationGuideDiscColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);

        /// <summary>
        /// If this variable is set to true, a sphere boundary will be shown which surrounds the rotation
        /// sphere in screen space. 
        /// </summary>
        /// <remarks>
        /// Results are not entirely accurate when using a perspective camera. This works best when an
        /// orthographic camera is used to render the gizmo.
        /// </remarks>
        [SerializeField]
        private bool _showRotationSphereBoundary = true;

        /// <summary>
        /// This is the color that is used to render the rotation sphere boundary.
        /// </summary>
        [SerializeField]
        private Color _rotationSphereBoundaryLineColor = Color.white;

        /// <summary>
        /// If this variable is set to true, a circle will be drawn around the rotation sphere in screen space.
        /// When the user clicks this circle and moves the mouse around, they will perform a rotation around the
        /// camera's look vector.
        /// </summary>
        /// <remarks>
        /// Results are not entirely accurate when using a perspective camera. This works best when an
        /// orthographic camera is used to render the gizmo.
        /// </remarks>
        [SerializeField]
        private bool _showCameraLookRotationCircle = true;

        /// <summary>
        /// The scale that should be applied to the camera look rotation circle. When a scale of 1 is specified,
        /// the circle will sit on the rotation sphere's boundary. The scale is relative to the radius of the
        /// rotation sphere in screen space.
        /// </summary>
        [SerializeField]
        private float _cameraLookRotationCircleRadiusScale = 1.11f;

        /// <summary>
        /// The color which must be used to draw the camera look rotation circle.
        /// </summary>
        [SerializeField]
        private Color _cameraLookRotationCircleLineColor = Color.white;

        /// <summary>
        /// The color which must be used to draw the camera look rotation circle when it is selected (i.e. when
        /// the user is using it to perform a rotation).
        /// </summary>
        [SerializeField]
        private Color _cameraLookRotationCircleColorWhenSelected = Color.yellow;

        /// <summary>
        /// This is an instance of the class which holds snap speciffic data.
        /// </summary>
        [SerializeField]
        private RotationGizmoSnapSettings _snapSettings = new RotationGizmoSnapSettings();

        /// <summary>
        /// This is the amount of accumulated rotation and it will be used in conjunction with snapping. It
        /// will allow us to decide when the accumulated rotation has exceeded the snap step value. When it
        /// does, we will need to perform a rotation.
        /// </summary>
        private float _accumulatedRotation = 0.0f;

        /// <summary>
        /// This represents the scale that will be applied to the radius of the rotation cricles that appear
        /// around the rotation sphere. These are the circles that the user can use to perform rotations around
        /// a certain axis. You can change this value as you wish.
        /// </summary>
        /// <remarks>
        /// The scale is relative to the radius of the rotation sphere. So, a value of 1 means that the circles
        /// have the same radius as the rotation sphere. A value of 2, would make the circles have a radius 2 times
        /// bigger than the rotation sphere radius and so on. You can modify this vaue as you wish, but I recommend
        /// leaving this to 1 because it seems to give the most intuitive results.
        /// </remarks>
        private const float _rotationCircleRadiusScale = 1.0f;

        /// <summary>
        /// The rotation circles will be drawn by drawing lines between the points which lie on the circles.
        /// The following arrays hold the circle points in gizmo local space. This will eliminate a little bit
        /// of overhead when we need to draw the circles. Keeping these arrays around will help us eliminate 
        /// calls to 'Mathf.Sin' and 'Mathf.Cos' functions every time we need to draw a circle.
        /// </summary>
        private Vector3[] _rotationCirclePointsForXAxisInLocalSpace;
        private Vector3[] _rotationCirclePointsForYAxisInLocalSpace;
        private Vector3[] _rotationCirclePointsForZAxisInLocalSpace;

        /// <summary>
        /// This is an array which holds the 2 points that make up the rotation guide disc. These
        /// points will be updated whenever the user moves the mouse with the left mouse button
        /// held down. The first element in the array represents the starting point (the point which
        /// is established when the user starts the rotation operation) while the second point holds
        /// the end of the disc and it will be updated whenever the mouse is moved during a rotation
        /// operation.
        /// </summary>
        private Vector3[] _rotationGuideLinePoints = new Vector3[2];

        /// <summary>
        /// This variable is set to true whenever the user selects (i.e. hovers) the circle which
        /// allows them to perform a rotation around the camera look vector. 
        /// </summary>
        private bool _isCameraLookRotationCircleSelected;

        /// <summary>
        /// This is the screen space point which intersects the camera look rotation circle. It
        /// will be adjusted every time the user moves the mouse over the selection circle.
        /// </summary>
        private Vector2 _cameraLookRotationCirclePickPoint;

        /// <summary>
        /// This variable is set to true whenever the user selects the rotation sphere. When
        /// this variable is set to true and the use is moving the mouse while holding the left
        /// mouse button down, they will be able to perform a rotation around the camera's right
        /// and up vectors respectively.
        /// </summary>
        private bool _isRotationSphereSelected;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the minimum value for the camera look rotation circle radius.
        /// </summary>
        public static float MinCameraLookRotationCircleRadiusScale { get { return 0.1f; } }
        
        /// <summary>
        /// Returns the minimum rotation sphere radius.
        /// </summary>
        public static float MinRotationSphereRadius { get { return 0.1f; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the rotation sphere radius. The minimum value for the rotation sphere radius is
        /// given by the 'MinRotationSphereRadius'. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float RotationSphereRadius
        {
            get { return _rotationSphereRadius; }
            set
            {
                _rotationSphereRadius = Mathf.Max(MinRotationSphereRadius, value);
                if (Application.isPlaying) CalculateRotationCirclePointsInGizmoLocalSpace();
            }
        }

        /// <summary>
        /// Gets/sets the rotation sphere color.
        /// </summary>
        public Color RotationSphereColor
        {
            get { return _rotationSphereColor; }
            set { _rotationSphereColor = value; }
        }

        /// <summary>
        /// Gets/sets the boolean flag which specifies whether or not the rotation sphere must be lit.
        /// </summary>
        public bool IsRotationSphereLit { get { return _isRotationSphereLit; } set { _isRotationSphereLit = value; } }

        /// <summary>
        /// Gets/sets the boolean flag which specifies whether or not the rotation guide must be shown.
        /// </summary>
        public bool ShowRotationGuide { get { return _showRotationGuide; } set { _showRotationGuide = value; } }

        /// <summary>
        /// Gets/sets the rotation guide line color.
        /// </summary>
        public Color RotationGuieLineColor { get { return _rotationGuieLineColor; } set { _rotationGuieLineColor = value; } }

        /// <summary>
        /// Gets/sets the rotation guide disc color.
        /// </summary>
        public Color RotationGuideDiscColor
        {
            get { return _rotationGuideDiscColor; }
            set { _rotationGuideDiscColor = value; }
        }

        /// <summary>
        /// Gets/sets the boolean which specifies whether or not the rotation sphere boundary must be shown.
        /// </summary>
        public bool ShowSphereBoundary { get { return _showRotationSphereBoundary; } set { _showRotationSphereBoundary = value; } }

        /// <summary>
        /// Gets/sets the rotation sphere boundary line color.
        /// </summary>
        public Color SphereBoundaryLineColor { get { return _rotationSphereBoundaryLineColor; } set { _rotationSphereBoundaryLineColor = value; } }

        /// <summary>
        /// Gets/sets the boolean which specifies whether or not the camera look rotation circle must be shown.
        /// </summary>
        public bool ShowCameraLookRotationCircle { get { return _showCameraLookRotationCircle; } set { _showCameraLookRotationCircle = value; } }

        /// <summary>
        /// Gets/sets the radius of the camera look rotation circle. The minimum value that the radius can have is given
        /// by the 'MinCameraLookRotationCircleRadiusScale'. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float CameraLookRotationCircleRadiusScale { get { return _cameraLookRotationCircleRadiusScale; } set { _cameraLookRotationCircleRadiusScale = Mathf.Max(MinCameraLookRotationCircleRadiusScale, value); } }
        
        /// <summary>
        /// Gets/sets th camera look rotation circle line color.
        /// </summary>
        public Color CameraLookRotationCircleLineColor { get { return _cameraLookRotationCircleLineColor; } set { _cameraLookRotationCircleLineColor = value; } }

        /// <summary>
        /// Gets/sets the camera look rotation circle line color when the circle is selected.
        /// </summary>
        public Color CameraLookRotationCircleColorWhenSelected { get { return _cameraLookRotationCircleColorWhenSelected; } set { _cameraLookRotationCircleColorWhenSelected = value; } }

        /// <summary>
        /// Returns the rotation snap settings associated with the gizmo.
        /// </summary>
        public RotationGizmoSnapSettings SnapSettings { get { return _snapSettings; } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks if the gizmo is ready for object manipulation.
        /// </summary>
        public override bool IsReadyForObjectManipulation()
        {
            // Return true if one of the following is true:
            //  a) one of the rotation circles; (i.e. _selectedAxis != GizmoAxis.None)
            //  b) rotation sphere; (i.e. _isRotationSphereSelected)
            //  c) camera look rotation circle. (i.e. _isCameraLookRotationCircleSelected)
            return SelectedAxis != GizmoAxis.None || _isRotationSphereSelected || _isCameraLookRotationCircleSelected;
        }

        /// <summary>
        /// Returns the gizmo type.
        /// </summary>
        public override GizmoType GetGizmoType()
        {
            return GizmoType.Rotation;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Performs any necessary initializations.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            CalculateRotationCirclePointsInGizmoLocalSpace();   // Calculate the gizmo local space vertices of the rotation circles
        }

        /// <summary>
        /// Called every frame to perform any necessary updates. The main purpose of this
        /// method is to identify the currently selected gizmo components.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            // We don't want to update the selection if the left mouse button is pressed. In that case the user
            // may be moving the mouse around in the scene to perform a rotation and we don't want to interfere
            // with that.
            if (_mouse.IsLeftMouseButtonDown) return;

            // Reset selection information. We will be updating these in the code which follows.
            var selectedAxis = GizmoAxis.None;
            _isCameraLookRotationCircleSelected = false;
            _isRotationSphereSelected = false;

            // Construct a ray from the current mouse cursor position. We will use this ray to check
            // if it intersects any of the gizmo components.
            Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);

            // Check if the mouse cursor intersects one of the selection circles. We will do this by 
            // looping through each rotation circle and checking its intersection with the ray by
            // calling the 'RayIntersectsRotationCircle' method.
            Vector3 intersectionPoint;
            float minimumDistanceFromCamera = float.MaxValue;
            float distanceFromCameraPositionToSphereCenter = GetDistanceFromCameraPositionToRotationSphereCenter();
            float circleRadius = GetWorldSpaceRotationCircleRadius();
            Vector3[] circlePlaneNormals = GetGizmoLocalAxes();
            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                // Check if the pick ray intersects the rotation circle
                if (RayIntersectsRotationCircle(pickRay, circlePlaneNormals[axisIndex], circleRadius, out intersectionPoint))
                {
                    // Check if the distance between the intersection point and the camera position is smaller than what we have so far
                    float distanceFromCamera = (pickRay.origin - intersectionPoint).magnitude;
                    if (distanceFromCamera < minimumDistanceFromCamera)
                    {
                        // We only consider the circle selection if the intersection point (i.e. which can be treated as a point
                        // on the circumference of the rotation circle) is visible to the camera. Otherwise, it means we intersected
                        // a point which is not being drawn on the screen and in that case we want to ignore it.
                        // Note: This check is necessary because when drawing the rotation circles, we will only draw the part of the
                        //       circle which is not obscured by the rotation sphere.
                        if (IsPointOnRotationCircleVisible(intersectionPoint, distanceFromCameraPositionToSphereCenter))
                        {
                            minimumDistanceFromCamera = distanceFromCamera;
                            selectedAxis = (GizmoAxis)axisIndex;
                        }
                    }
                }
            }

            // Now check if the mouse cursor is intersecting the rotation circle which is used to allow the user to rotate
            // around the camera look vector. We do this by calculating a vector which goes from the circle's center to
            // the current mouse cursor position and checking if the magnitude of the resulting vector is the same as (with
            // tollerance) the circle radius. 
            // Note: We only do this if the camera look rotation circle is allowed to be drawn. Otherwise, there is nothing
            //       to be selected.
            if (_showCameraLookRotationCircle)
            {
                // We will need the radius of the rotation circle in screen space. This is the same as the radius of the
                // sphere boundary scaled by '_cameraLookRotationCircleRadiusScale'.
                Vector2 rotationSphereScreenSpaceCenter = GetRotationSphereScreenSpaceCenter();
                float cameraLookRotationCircleRadius = EstimateRotationSphereScreenSpaceBoundaryCircleRadius(rotationSphereScreenSpaceCenter) * _cameraLookRotationCircleRadiusScale;

                // Check if the mouse cursor position is close enough to the circle's circumference
                const float epsilon = 5.0f;
                Vector2 cursorPosition = Input.mousePosition;
                if (Mathf.Abs((cursorPosition - rotationSphereScreenSpaceCenter).magnitude - cameraLookRotationCircleRadius) <= epsilon)
                {
                    // The circle is intersected by the mouse cursor. Deselect any previously selected axes and mark the
                    // circle as selected. Note that we don't perform any checks to see if the intersection point is closer
                    // to the camera than what we have so far. This is because by preference, we want to give priority to
                    // the camera look rotation circle. We still make sure that we eliminate any other selections just to be safe.
                    selectedAxis = GizmoAxis.None;
                    _isCameraLookRotationCircleSelected = true;

                    // We will need to store the intersection point because we will need it when calculating the rotation guide line points.
                    // We could just set the current mouse cursor position, but we have to remember that we used an epsilon value for the
                    // intersection test. We want to store the exact point on the circle. For that reason, we will first calculate a vector
                    // which goes from the sphere center to the cursor position, normalize it and then use it to generate a new point which
                    // sits exactly on the circle circumference. We store the result inside the '_cameraLookRotationCirclePickPoint' variable.
                    Vector2 toPickPoint = cursorPosition - rotationSphereScreenSpaceCenter;
                    toPickPoint.Normalize();
                    _cameraLookRotationCirclePickPoint = rotationSphereScreenSpaceCenter + toPickPoint * EstimateRotationSphereScreenSpaceBoundaryCircleRadius(rotationSphereScreenSpaceCenter) * _cameraLookRotationCircleRadiusScale;
                }
            }

            SelectAxis(selectedAxis);

            // Finally, check if the ray intersects the rotation sphere.
            // Note: We will only perform this check if nothing else was selected.
            float t;
            if (SelectedAxis == GizmoAxis.None && !_isCameraLookRotationCircleSelected &&
                pickRay.IntersectsSphere(_gizmoTransform.position, GetWorldSpaceRotationSphereRadius(), out t))
            {
                // Mark the rotation sphere as selected
                _isRotationSphereSelected = true;
            }
        }

        /// <summary>
        /// Called whenever the left mouse button is pressed. The method is responsible for
        /// checking which components of the gizmo were picked and perform any additional
        /// actions like storing data which is needed while processing mouse move events.
        /// </summary>
        protected override void OnLeftMouseButtonDown()
        {
            base.OnLeftMouseButtonDown();

            // If there is an axis which is selected, it means the user was hovering one
            // of the rotation circles when they pressed the left mouse button. In that case
            // we want to update the rotation guide lines points accordingly.
            if (SelectedAxis != GizmoAxis.None)
            {
                // Construct a ray using the current mouse cursor position
                Ray pickRay = _camera.ScreenPointToRay(Input.mousePosition);

                // Establish the normal of the rotation circle based on the selected gizmo axis
                Vector3 circlePlaneNormal;
                if (SelectedAxis == GizmoAxis.X) circlePlaneNormal = _gizmoTransform.right;
                else if (SelectedAxis == GizmoAxis.Y) circlePlaneNormal = _gizmoTransform.up;
                else circlePlaneNormal = _gizmoTransform.forward;

                // Check if the ray intersects the rotation circle. This will always be the case because this code is
                // executed if there is currently a selected axis (i.e. rotation circle). We will use the intersection
                // point to calculate the starting point of the rotation disc. We do this because when the user picks
                // a rotation circle, it means they want to start a rotation.
                Vector3 intersectionPoint;
                if (RayIntersectsRotationCircle(pickRay, circlePlaneNormal, GetWorldSpaceRotationCircleRadius(), out intersectionPoint))
                {
                    // Calculate a normalized vector which goes from the rotation sphere's center and aims towards the intersection point.
                    // Note: We will project the intersection point on the circle plane to make sure the rotation guide lines exist
                    //       in that plane. Otherise the rotation guide line points might be placed either above or below the circle plane
                    //       because of the way in which we test for intersection inside the 'RayIntersectsRotationCircle' method (i.e we
                    //       sometimes treat the circle as a cylinder).
                    Plane circlePlane = new Plane(circlePlaneNormal, _gizmoTransform.position);
                    float distanceToIntersectionPoint = circlePlane.GetDistanceToPoint(intersectionPoint);
                    intersectionPoint -= circlePlaneNormal * distanceToIntersectionPoint;

                    // In order to calculate the first point on the rotation guide disc, we will need a vector which goes from the
                    // center of the rotation sphere to the intersection point.
                    Vector3 fromSphereCenterToIntersectionPoint = intersectionPoint - _gizmoTransform.position;
                    fromSphereCenterToIntersectionPoint.Normalize();

                    // Use the normalized vector to calculate the first point which sits on the rotation guide disc. We do this
                    // by moving from the sphere center (which is the same as the gizmo position) along the normalized vector by
                    // a distance equal to the rotation sphere circle radius. We use the rotation circle radius so that the disc
                    // rotation guide disc will extend along the circle's circumference.
                    _rotationGuideLinePoints[0] = _gizmoTransform.position + fromSphereCenterToIntersectionPoint * GetWorldSpaceRotationCircleRadius();
                }
            }
            else
            // When the camera look rotation circle is selected, the rotation guide line point is will be set to the circle pick point in screen space
            if (_isCameraLookRotationCircleSelected) _rotationGuideLinePoints[0] = _cameraLookRotationCirclePickPoint;

            // When the mouse is pressed down, there is no rotation, so we will set the second rotation guide line point to the first and
            // we will make sure to update it when the mouse is moved while holding down the left mouse button.
            _rotationGuideLinePoints[1] = _rotationGuideLinePoints[0];

            // Whenever the left mouse button is pressed, we reset the accumulated rotation to prepare for a new rotation session
            _accumulatedRotation = 0.0f;
        }

        /// <summary>
        /// Called when the mouse is moved. This method will make sure that any necessary rotation 
        /// is applied to the gizmo and its controlled objects.
        /// </summary>
        protected override void OnMouseMoved()
        {
            base.OnMouseMoved();

            //if (!CanAnyControlledObjectBeManipulated()) return;

            // We only proceed if the left mouse button is pressed
            if (_mouse.IsLeftMouseButtonDown)
            {
                // All rotations will be performed based on the amount of mouse movement. We will declare
                // a variable here which represents the number of degrees which correspond to one unit in
                // screen space. You can increase/decrease this value if you wish to change the rotation
                // speed.
                float degreesPerScreenUnit = 0.45f;

                // Is there a rotation circle selected?
                if (SelectedAxis != GizmoAxis.None)
                {
                    // The main idea is to use the mouse movement to generate rotation values around the axis which
                    // corresponds to the selected rotation circle. The first step is to identify the circle plane
                    // normal based on the currently selected axis.
                    Vector3 circlePlaneNormal;
                    if (SelectedAxis == GizmoAxis.X) circlePlaneNormal = _gizmoTransform.right;
                    else if (SelectedAxis == GizmoAxis.Y) circlePlaneNormal = _gizmoTransform.up;
                    else circlePlaneNormal = _gizmoTransform.forward;

                    // We will need to calculate a circle tangent. This is a vector which starts from the first rotation
                    // guide line point and aims in the direction of rotation.
                    Vector3 fromSphereCenterToGuidePoint = _rotationGuideLinePoints[0] - _gizmoTransform.position;
                    Vector3 circleTangent = Vector3.Cross(circlePlaneNormal, fromSphereCenterToGuidePoint);
                    circleTangent.Normalize();

                    // Calculate the start and end (tip) points of the tangent and transform them in screen space. We will
                    // then use these 2 points in screen space to calculate a screen space tangent vector.
                    Vector3 firstTangentPoint = _rotationGuideLinePoints[0];
                    Vector3 secondTangentPoint = firstTangentPoint + circleTangent;
                    firstTangentPoint = _camera.WorldToScreenPoint(firstTangentPoint);
                    secondTangentPoint = _camera.WorldToScreenPoint(secondTangentPoint);

                    // Calculate the screen space tangent vector. The reason that we need this vector is that it will allow 
                    // us to scale the mouse move offset such that it contributes to the rotation more or less based on how
                    // 'well' it projects on the screen space tangent. This is necessary to achieve a behaviour which is 
                    // similar to the one which the Unity editor rotation gizmo uses.
                    Vector2 screenSpaceTangent = secondTangentPoint - firstTangentPoint;
                    screenSpaceTangent.Normalize();

                    // Calculate the relative rotation angle. We do this by projecting the moue cursor move offset on the
                    // circle tangent. MUltiplying the result by 'degreesPerScreenUnit' gives us the relative angle.
                    float mouseCursorMoveOffsetProjection = Vector2.Dot(screenSpaceTangent, _mouse.CursorOffsetSinceLastFrame);
                    float relativeAngleInDegrees = degreesPerScreenUnit * mouseCursorMoveOffsetProjection; 

                    // We need to handle this differently based on whether or not snapping is used
                    if(_snapSettings.IsSnappingEnabled)
                    {
                        // Add the relative angle to accumulated rotation value
                        _accumulatedRotation += relativeAngleInDegrees;

                        // If the absolute accumulated rotation value is >= to the snap step value, it means a rotation must be performed
                        if(Mathf.Abs(_accumulatedRotation) >= _snapSettings.StepValueInDegrees)
                        {
                            // Calculate the rotation angle which must be used to apply the rotation. This angle is equal to the number of
                            // full step values that can be found in the accumulated rotation multiplied by the sign of the relative angle
                            // calculated earlier.
                            float numberOfFullSteps = (float)((int)(Mathf.Abs(_accumulatedRotation / _snapSettings.StepValueInDegrees)));
                            float rotationAngle = _snapSettings.StepValueInDegrees * numberOfFullSteps * Mathf.Sign(relativeAngleInDegrees);

                            // Calculate the new rotation guide line extreme point. We do this by rotating the guide line vector using 
                            // the calculated rotation angle.
                            Vector3 rotatedVector = _rotationGuideLinePoints[1] - _gizmoTransform.position;
                            rotatedVector = Quaternion.AngleAxis(rotationAngle, circlePlaneNormal) * rotatedVector;
                            rotatedVector.Normalize();
                            _rotationGuideLinePoints[1] = _gizmoTransform.position + rotatedVector * GetWorldSpaceRotationCircleRadius();

                            // Apply the rotation
                            _gizmoTransform.Rotate(circlePlaneNormal, rotationAngle, Space.World);
                            RotateControlledObjects(circlePlaneNormal, rotationAngle);

                            // Make sure to adjust the accumulated rotation accordingly. If it is > 0, we will subtract the entire amount of
                            // step values which fit inside it and keep what remains for the next mouse move event. Otherwise, we perform an
                            // addition instead of a subtraction to go in the opposite direction.
                            if (_accumulatedRotation > 0.0f) _accumulatedRotation -= _snapSettings.StepValueInDegrees * numberOfFullSteps;
                            else if (_accumulatedRotation < 0.0f) _accumulatedRotation += _snapSettings.StepValueInDegrees * numberOfFullSteps;
                        }
                    }
                    else
                    {
                        // Calculate the new rotation guide line extreme point. We do this by rotating the guide line vector
                        // using the relative angle that we have just calculated.
                        Vector3 rotatedVector = _rotationGuideLinePoints[1] - _gizmoTransform.position;
                        rotatedVector = Quaternion.AngleAxis(relativeAngleInDegrees, circlePlaneNormal) * rotatedVector;
                        rotatedVector.Normalize();
                        _rotationGuideLinePoints[1] = _gizmoTransform.position + rotatedVector * GetWorldSpaceRotationCircleRadius();

                        // Apply the rotation
                        _gizmoTransform.Rotate(circlePlaneNormal, relativeAngleInDegrees, Space.World);
                        InputRange.SetValue(InputRange.Value - circlePlaneNormal * relativeAngleInDegrees);
                        //RotateControlledObjects(circlePlaneNormal, relativeAngleInDegrees);
                    }
                }
                else
                // Is the camera look rotation circle selected?
                if (_isCameraLookRotationCircleSelected)
                {
                    // Note: The algorithm is based on the same principles as the one used to rotate using a selected rotation circle.
                    Vector2 rotationSphereScreenSpaceCenter = GetRotationSphereScreenSpaceCenter();
                    Vector2 toLookRotationCirclePickPoint = _cameraLookRotationCirclePickPoint - rotationSphereScreenSpaceCenter;

                    // We will start of by calculating the tangent to the camera look rotation circle. This is the vector which
                    // is perpendicular to the vector which goes from the sphere rotation center in screen space to the camera
                    // look rotation circle pick point.
                    Vector2 circleTangent = new Vector2(-toLookRotationCirclePickPoint.y, toLookRotationCirclePickPoint.x);
                    circleTangent.Normalize();

                    // Calculate the normalized accumulated mouse cursor move offset since the left mosue button was pressed
                    Vector2 cursorMoveOffsetSinceLeftMouseButtonDown = _mouse.CursorOffsetSinceLeftMouseButtonDown;
                    cursorMoveOffsetSinceLeftMouseButtonDown.Normalize();

                    // Project the accumulated mouse cursor move offset on the circle tangent and the use it to scale the angle value
                    float accumulatedMoveOffsetProjection = Vector2.Dot(circleTangent, cursorMoveOffsetSinceLeftMouseButtonDown);
                    float accumulatedAngleInDegrees = _mouse.CursorOffsetSinceLeftMouseButtonDown.magnitude * degreesPerScreenUnit * accumulatedMoveOffsetProjection;

                    // Use the calculated angle value to rotate the second guide line point relative to the first one
                    Quaternion rotationQuaternion = Quaternion.AngleAxis(accumulatedAngleInDegrees, Vector3.forward);
                    Vector2 rotatedVector = toLookRotationCirclePickPoint;
                    rotatedVector = rotationQuaternion * rotatedVector;
                    rotatedVector.Normalize();

                    // We will need the normalized mouse cursor move offset during the current frame
                    Vector2 mouseCursorMoveOffsetSinceLastFrame = _mouse.CursorOffsetSinceLastFrame;
                    mouseCursorMoveOffsetSinceLastFrame.Normalize();

                    // Calculate the projection of the mouse cursor move offset in the current frame on the screen space tangent.
                    // Then use this projection to calculate the relative angle value (i.e. how much we need to rotate the gizmo
                    // during this frame).
                    float moveOffsetProjection = Vector2.Dot(circleTangent, mouseCursorMoveOffsetSinceLastFrame);
                    float relativeAngleInDegrees = _mouse.CursorOffsetSinceLastFrame.magnitude * moveOffsetProjection * degreesPerScreenUnit;

                    // Rotate the gizmo
                    _gizmoTransform.Rotate(_camera.transform.forward, relativeAngleInDegrees, Space.World);

                    // Recalculate the second guide line point using the rotated vector
                    _rotationGuideLinePoints[1] = rotationSphereScreenSpaceCenter + rotatedVector * EstimateRotationSphereScreenSpaceBoundaryCircleRadius(rotationSphereScreenSpaceCenter) * _cameraLookRotationCircleRadiusScale;

                    // Make sure all controlled objects are rotated accordingly
                    RotateControlledObjects(_camera.transform.forward, relativeAngleInDegrees);
                }
                //else
                // Is the rotation sphere selected?
//                if (_isRotationSphereSelected)
//                {
//                    // In this case we want to use the mouse cursor move offset to rotate around the camera right and up vectors respectively.
//                    // We will start by calculating 2 screen space tangent vectors: 
//                    //  a) rightTangent -> aims to the right in screen space;
//                    //  b) upTangent -> aims upwards in screen space.
//                    Vector2 rightTangent = new Vector2(1.0f, 0.0f);
//                    Vector2 upTangent = new Vector2(0.0f, 1.0f);
//
//                    // We need the normalized mouse cursor move offset since the last frame
//                    Vector2 mouseCursorMoveOffsetSinceLastFrame = _mouse.CursorOffsetSinceLastFrame;
//                    mouseCursorMoveOffsetSinceLastFrame.Normalize();
//
//                    // Calculate the projections of the mouse cursor move offset on the 2 tangent vectors
//                    float offsetProjectionOnRightTangent = -Vector2.Dot(rightTangent, mouseCursorMoveOffsetSinceLastFrame);
//                    float offsetProjectionOnUpTangent = Vector2.Dot(upTangent, mouseCursorMoveOffsetSinceLastFrame);
//
//                    // We will now calculate the rotation angles in degrees that need to be applied to the gizmo and its controlled
//                    // objects by using the mouse cursor move offset during the current frame and scaling it by the 2 projections.
//                    float angleInDegreesAroundCameraRight = _mouse.CursorOffsetSinceLastFrame.magnitude * offsetProjectionOnUpTangent * degreesPerScreenUnit;
//                    float angleInDegreesAroundCameraUp = _mouse.CursorOffsetSinceLastFrame.magnitude * offsetProjectionOnRightTangent * degreesPerScreenUnit;
//
//                    var cameraTransform = _camera.transform;
//                    // Apply the necessary rotations
//                    _gizmoTransform.Rotate(cameraTransform.right, angleInDegreesAroundCameraRight, Space.World);
//                    _gizmoTransform.Rotate(cameraTransform.up, angleInDegreesAroundCameraUp, Space.World);
//
//                    // Make sure all controlled objects are rotated accordingly
//                    RotateControlledObjects(cameraTransform.right, angleInDegreesAroundCameraRight);
//                    RotateControlledObjects(cameraTransform.up, angleInDegreesAroundCameraUp);
//                }
            }
        }

        /// <summary>
        /// This method is called after the camera has finished rendering the scene. 
        /// It allows us to perform any necessary drawing, like circles, discs etc.
        /// </summary>
        protected override void OnRenderObject()
        {
            //if (Camera.current != EditorCamera.Instance.Camera) return;
            base.OnRenderObject();

            // Draw the rotation sphere
            DrawRotationSphere(GetRotationSphereWorldTransform());

            // If the left mouse button is pressed, we may need to draw the rotation guides
            if (_mouse.IsLeftMouseButtonDown)
            {
                // If the user has selected one of the rotation circles and the rotation guide must be drawn, we will draw it
                if (SelectedAxis != GizmoAxis.None && _showRotationGuide)
                {
                    // Draw the 2 lines which connect the center of the rotation sphere with the 2 points which sit at the beginning and end of the disc respectively
                    GLPrimitives.Draw3DLine(_gizmoTransform.position, _rotationGuideLinePoints[0], _rotationGuieLineColor, MaterialPool.Instance.GizmoLine);
                    GLPrimitives.Draw3DLine(_gizmoTransform.position, _rotationGuideLinePoints[1], _rotationGuieLineColor, MaterialPool.Instance.GizmoLine);

                    // Draw the disc which is formed by the 2 guide points.
                    // Note: In order to draw the disc, we have to supply a normal. The normal of the disc is the normal to the
                    //       plane of the circle which is currently selected.
                    Vector3 discPlaneNormal = _gizmoTransform.right;
                    if (SelectedAxis == GizmoAxis.Y) discPlaneNormal = _gizmoTransform.up;
                    else if (SelectedAxis == GizmoAxis.Z) discPlaneNormal = _gizmoTransform.forward;

                    Material discMaterial = MaterialPool.Instance.GizmoSolidComponent;
                    discMaterial.SetInt("_IsLit", 0);
                    int cullMode = discMaterial.GetInt("_CullMode");
                    discMaterial.SetInt("_CullMode", 0);
                    GLPrimitives.Draw3DFilledDisc(_gizmoTransform.position, _rotationGuideLinePoints[0], _rotationGuideLinePoints[1], discPlaneNormal, _rotationGuideDiscColor, discMaterial);
                    discMaterial.SetInt("_CullMode", cullMode);
                }
                else
                if ((_isCameraLookRotationCircleSelected && _showRotationGuide))
                {
                    // If the camera look rotation circle is selected and the rotation guide must be drawn, we will draw it,
                    // but this time we will draw it in screen space. 
                    // Note: Drawing the rotation guide in screen space in this situation is much more natural because the
                    //       camera look rotation circle has its points defined in screen space.
                    Vector2 rotationSphereCenterInScreenSpace = GetRotationSphereScreenSpaceCenter();
                    GLPrimitives.Draw2DLine(rotationSphereCenterInScreenSpace, _rotationGuideLinePoints[0], _rotationGuieLineColor, MaterialPool.Instance.GizmoLine, _camera);
                    GLPrimitives.Draw2DLine(rotationSphereCenterInScreenSpace, _rotationGuideLinePoints[1], _rotationGuieLineColor, MaterialPool.Instance.GizmoLine, _camera);

                    // Draw the guide disc. The disc will also be rendered in screen space.
                    Material discMaterial = MaterialPool.Instance.GizmoSolidComponent;
                    discMaterial.SetInt("_IsLit", 0);
                    int cullMode = discMaterial.GetInt("_CullMode");
                    discMaterial.SetInt("_CullMode", 0);
                    GLPrimitives.Draw2DFilledDisc(rotationSphereCenterInScreenSpace, _rotationGuideLinePoints[0], _rotationGuideLinePoints[1], _rotationGuideDiscColor, discMaterial, _camera);
                    discMaterial.SetInt("_CullMode", cullMode);
                }
            }

            // We will always draw the rotation circles for each axis
            if (InputRange.ActiveAxes.HasX()) {
                DrawRotationCircle(GizmoAxis.X);
            }
            if (InputRange.ActiveAxes.HasY()) {
                DrawRotationCircle(GizmoAxis.Y);
            }
            if (InputRange.ActiveAxes.HasZ()) {
                DrawRotationCircle(GizmoAxis.Z);
            }

            // Check if the rotation sphere boundary or camera look rotation circle must be rendered.
            // Note: We only draw if the gizmo is visible to the camera. This is a necessary test for screen 
            //       space geometry because this type of geometry is not clipped by the camera near and far
            //       planes and we can get into a situation where the gizmo is in front of any of these planes.
            //       When that happens, the 3D gizmo components will be clipped by the engine, but the 2D geometry
            //       will still be drawn. We don't want that to happen, so we have to perform this visibility
            //       check via 'IsGizmoVisible'.
            if ((_showRotationSphereBoundary || _showCameraLookRotationCircle) && IsGizmoVisible())
            {
                // If at least one of these must be rendered, it means we need to get access to some needed information.
                // We will need this info for both entities in case both variables are true.
                Vector3 sphereCenterInScreenSpace = GetRotationSphereScreenSpaceCenter();
                Vector3[] rotationSphereScreenSpaceBoundaryPoints = GetRotationSphereScreenSpaceBoundaryPoints();

                // Draw the rotation sphere boundary and the camera look rotation circle
                if (_showRotationSphereBoundary) GLPrimitives.Draw2DCircleBorderLines(rotationSphereScreenSpaceBoundaryPoints, sphereCenterInScreenSpace, _rotationSphereBoundaryLineColor, 1.0f, MaterialPool.Instance.GizmoLine, _camera);
                if (_showCameraLookRotationCircle)
                {
                    // Establish the final color of the circle based on whether or not it is currently selected and then draw it
                    Color circleColor = _isCameraLookRotationCircleSelected ? _cameraLookRotationCircleColorWhenSelected : _cameraLookRotationCircleLineColor;
                    GLPrimitives.Draw2DCircleBorderLines(rotationSphereScreenSpaceBoundaryPoints, sphereCenterInScreenSpace, circleColor, _cameraLookRotationCircleRadiusScale, MaterialPool.Instance.GizmoLine, _camera);
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This method can be used to check if the gizmo is visible to the camera. The reason 
        /// that this is necessary for a rotation gizmo is that with a rotation gizmo we are 
        /// also drawing 2D circles. If at any point in time the rotation gizmo sits in front 
        /// of either the camera near or far clip plane, the gizmo will not be rendered on the
        /// screen anymore because the 3D geometry is clipped by the engine, but the 2D geometry
        /// will still be rendered because when drawing in screen space the camera frustum is not
        /// used to clip 2D screen geometry. In that case we will onyl see the sphere boundary and
        /// the camera look rotation circles being drawn which is not desirable. Using this method
        /// we can detect when the gizmo is not visible on the screen anymore and only draw the
        /// 2D geometry when the gizmo is indeed visible.
        /// </summary>
        /// <returns></returns>
        private bool IsGizmoVisible()
        {
            // The gizmo is visible only if the sphere lies at least partially between the
            // camera near and far clip planes.
            return !IsGizmoInFrontOfCameraNearClipPlane() && !IsGizmoInFrontOfCameraFarClipPlane();
        }

        /// <summary>
        /// Checks if the gizmo is in front of the camera near clip plane.
        /// </summary>
        /// <remarks>
        /// The implementation of this method will construct the near camera clip plane
        /// such that its normal points outside of the view volume.
        /// </remarks>
        private bool IsGizmoInFrontOfCameraNearClipPlane() {
            var cameraTransform = _camera.transform;
            // In order to check if the gizmo lies totally in front of the near plane, we will choose a point
            // on the rotation sphere which, if it lies in front of the near plane, we know for sure that the
            // entire gizmo rotation sphere lies in front of that plane. This point is constructed by moving
            // from the gizmo position (sphere center) along the camera forward vector by an amount equal to 
            // the sphere radius. If this point is in front of the plane, the entire sphere is in front of the plane.
            Vector3 queryPoint = _gizmoTransform.position + cameraTransform.forward * GetWorldSpaceRotationSphereRadius();
            Plane cameraNearPlane = new Plane(-cameraTransform.forward, cameraTransform.position + cameraTransform.forward * _camera.nearClipPlane);

            // In front?
            return cameraNearPlane.GetDistanceToPoint(queryPoint) > 0.0f;
        }

        /// <summary>
        /// Checks if the gizmo is in front of the camera far clip plane.
        /// </summary>
        /// <remarks>
        /// The implementation of this method will construct the far camera clip plane
        /// such that its normal points outside of the view volume.
        /// </remarks>
        private bool IsGizmoInFrontOfCameraFarClipPlane() {
            var cameraTransform = _camera.transform;
            // This follows the same logic as the implementation of 'IsGizmoInFrontOfCameraNearClipPlane'. Please 
            // see the comments inside that method to understand why we perform these calculations. The only difference
            // in this implementation is the way we construct the plane because now we are using the camera forward
            // vector as the plane normal instead of the negated version as we did in the other function. 'queryPoint'
            // is also calculated by subtracting from '_gizmoTransform.position' instead of adding. This is because
            // the plane normal has been reversed.
            Vector3 queryPoint = _gizmoTransform.position - cameraTransform.forward * GetWorldSpaceRotationSphereRadius();
            Plane cameraFarPlane = new Plane(cameraTransform.forward, cameraTransform.position + cameraTransform.forward * _camera.farClipPlane);

            // In front?
            return cameraFarPlane.GetDistanceToPoint(queryPoint) > 0.0f;
        }

        /// <summary>
        /// Returns an array of points which make up the rotation circle for 
        /// the specified axis. We need this function when drawing the rotation
        /// circles that the user can use to perform rotations.
        /// </summary>
        private Vector3[] GetRotationCirclePointInWorldSpace(GizmoAxis gizmoAxis)
        {
            // Retrieve the circle points in gizmo local space
            Vector3[] circlePointsInLocalSpace;
            if (gizmoAxis == GizmoAxis.X) circlePointsInLocalSpace = _rotationCirclePointsForXAxisInLocalSpace;
            else if (gizmoAxis == GizmoAxis.Y) circlePointsInLocalSpace = _rotationCirclePointsForYAxisInLocalSpace;
            else circlePointsInLocalSpace = _rotationCirclePointsForZAxisInLocalSpace;

            // Now use the gizmo local space points to generate the world points
            Vector3[] rotationCirclePointsInWorldSpace = new Vector3[circlePointsInLocalSpace.Length];
            for (int linePointIndex = 0; linePointIndex < circlePointsInLocalSpace.Length; ++linePointIndex)
            {
                // Transform the local space point in world space by multiplying it with the gizmo's transform matrix
                rotationCirclePointsInWorldSpace[linePointIndex] = _gizmoTransform.TransformPoint(circlePointsInLocalSpace[linePointIndex]);
            }

            // Rerturn the point array
            return rotationCirclePointsInWorldSpace;
        }

        /// <summary>
        /// Draws the rotation circle which identifies the specified axis.
        /// </summary>
        private void DrawRotationCircle(GizmoAxis gizmoAxis)
        {
            // We will need this to establish circle points which are not visible to the camera
            float distanceFromCameraPositionToSphereCenter = GetDistanceFromCameraPositionToRotationSphereCenter();

            // Retrieve the world space circle points
            Vector3[] worldSpaceCirclePoints = GetRotationCirclePointInWorldSpace(gizmoAxis);

            // We will draw the circle using a single draw call to 'GLPrimitives.Draw3DLines', so we will need
            // to establish the color of each point which lies on the circimference of the circle.
            Color[] circlePointColors = new Color[worldSpaceCirclePoints.Length];

            // Establish the circle color based on whether or not the specified input axis is currently selected
            Color circleColor = (SelectedAxis == gizmoAxis ? _selectedAxisColor : _axesColors[(int)gizmoAxis]);
            for (int pointIndex = 0; pointIndex < worldSpaceCirclePoints.Length; ++pointIndex)
            {
                // Store the point for easy access
                Vector3 point = worldSpaceCirclePoints[pointIndex];

                // Establish the visibility of the point
                bool isPointVisible = IsPointOnRotationCircleVisible(point, distanceFromCameraPositionToSphereCenter);

                // Establish the point color. Points which are invisible will be drawn with an alpha value of 0.
                circlePointColors[pointIndex] = isPointVisible ? circleColor : new Color(circleColor.r, circleColor.g, circleColor.b, 0.0f);
            }

            // Draw the circle
            MaterialPool.Instance.GizmoLine.SetInt("_StencilRefValue", _doNotUseStencil);
            GLPrimitives.Draw3DLines(worldSpaceCirclePoints, circlePointColors, true, MaterialPool.Instance.GizmoLine, true, circlePointColors[0]);
        }

        /// <summary>
        /// Returns a series of screen space points which represent the boundary circle of the rotation sphere.
        /// </summary>
        private Vector3[] GetRotationSphereScreenSpaceBoundaryPoints()
        {
            // We will need the rotation sphere's center and the boundary circle radius in screen space
            Vector3 sphereCenterInScreenSpace = GetRotationSphereScreenSpaceCenter();
            float circleRadius = EstimateRotationSphereScreenSpaceBoundaryCircleRadius(sphereCenterInScreenSpace);

            // Create the point array. We will store all the generated points here.
            const int numberOfBoundaryPoints = 100;
            Vector3[] boundaryPoints = new Vector3[numberOfBoundaryPoints];

            // Generate the screen space points
            float angleStep = 360.0f / (numberOfBoundaryPoints - 1);
            for (int pointIndex = 0; pointIndex < numberOfBoundaryPoints; ++pointIndex)
            {
                // Generate a new point by rotating around the imaginary screen space Z axis.
                // Note: For each point that we generate we have to add the angle step value to the current angle in order to
                //       obtain the current rotation angle. This is done by multiplying the angle step with the current point
                //       index. We then multiply by 'Mathf.Deg2Rad' to transform the angle in radians. We need it in radians,
                //       because we are going to use the 'Sin' and 'Cos' functions which expect paramteres in units of radians.
                float angleInRadians = angleStep * pointIndex * Mathf.Deg2Rad;
                Vector3 point = new Vector3(Mathf.Sin(angleInRadians) * circleRadius, Mathf.Cos(angleInRadians) * circleRadius, 0.0f);

                // Translate the point using the sphere's center in screen space and store the point inside the array
                point += sphereCenterInScreenSpace;
                boundaryPoints[pointIndex] = point;
            }

            // Return the boundary point array
            return boundaryPoints;
        }

        /// <summary>
        /// Returns the screen space center of the rotation sphere.
        /// </summary>
        private Vector2 GetRotationSphereScreenSpaceCenter()
        {
            // The sphere center is the same as the gizmo's position, so all we have to do
            // is to transform the gizmo position in screen space to get the desired result.
            return _camera.WorldToScreenPoint(_gizmoTransform.position);
        }

        /// <summary>
        /// Estimates and returns the screen space radius of the rotation sphere. The word 'Estimates' is used
        /// because when using a perspective camera, the radius is only an approximation. This means that
        /// when you rotate the camera such that the rotation sphere sits somewhere at the edge of the screen,
        /// the result will not be 100% accurate.
        /// </summary>
        /// <param name="screenSpaceBoundaryCircleCenter">
        /// This is the screen space center of the rotation sphere and it is needed to perform the necessary
        /// calculations.
        /// </param>
        private float EstimateRotationSphereScreenSpaceBoundaryCircleRadius(Vector3 screenSpaceBoundaryCircleCenter)
        {
            // We will estimate the radius by moving from the rotation sphere center along the camera's up vector by
            // a distance equal to the rotation sphere radius. We then transform the result in screen space and return
            // the distance between the screen space sphere center and the resulting point. This is our estimated radius.
            Vector3 pointOnTopOfCircleInScreenSpace = _gizmoTransform.position + _camera.transform.up * GetWorldSpaceRotationSphereRadius();
            pointOnTopOfCircleInScreenSpace = _camera.WorldToScreenPoint(pointOnTopOfCircleInScreenSpace);
            pointOnTopOfCircleInScreenSpace.z = 0.0f;

            return (pointOnTopOfCircleInScreenSpace - screenSpaceBoundaryCircleCenter).magnitude;
        }

        /// <summary>
        /// Returns the distance between the camera position and the sphere center. This function was designed
        /// to be used specifically when deciding if a point on one of the rotation circles is visible to the
        /// camera.
        /// </summary>
        /// <remarks>
        /// This function is necessary because the distance will be calculated differently based on the type
        /// of camera which is used to render the gizmo object.
        /// </remarks>
        private float GetDistanceFromCameraPositionToRotationSphereCenter() {
            var cameraTransform = _camera.transform;
            // Because this function is supposed to be used in the context of visibility checking, we will
            // have to calculate the distance differently based on the type of camera that is used to render
            // the gizmo. This is because each camera type models a different kind of view volume which can
            // impact the way in which we see geometry in the 3D world.
            if (_camera.orthographic)
            {
                // When an orthographic camera is used, we will treat the distance between the camea position and the
                // rotation sphere center as the distance of the rotation sphere center from the camera near plane. We
                // do this because we want to get a straight line vector much like the rays that go from the camera
                // near plane in the scene (when an orthographic camera is used).
                Plane cameraNearPlane = new Plane(cameraTransform.forward, cameraTransform.position + cameraTransform.forward * _camera.nearClipPlane);
                float distanceToSphereCenter = cameraNearPlane.GetDistanceToPoint(_gizmoTransform.position);

                // Return the distance. 
                // Note: We use the 'Abs' function because we aren't interested in the sign. We just want a pure distance value.
                return Mathf.Abs(distanceToSphereCenter);
            }
            // A perspective camera models a frustum inside the scene. This means that every ray that we cast from the screen
            // into the scene will be cast at a ceratin angle from the camera's forward vector. A more precise way to put it, 
            // the camera view frustum allows us to see the 3D world in almost the same way as we see in real life, so we will
            // use the real distance from the camera position which is the magnitude of the vector that goes from the gizmo
            // position to the camera position.
            else return (cameraTransform.position - _gizmoTransform.position).magnitude;
        }

        /// <summary>
        /// Checks if the specified point is visible to the camera which is used to render the gizmo.
        /// </summary>
        /// <param name="point">
        /// The point whose visibility is being checked.
        /// </param>
        /// <param name="distanceFromCameraPositionToSphereCenter">
        /// The distance between the camera position and the rotation sphere center.
        /// </param>
        private bool IsPointOnRotationCircleVisible(Vector3 point, float distanceFromCameraPositionToSphereCenter) {
            var cameraTransform = _camera.transform;
            // Plese see the comments written inside the 'GetDistanceFromCameraPositionToRotationSphereCenter' method.
            // The following lines of code follow the same principles.
            if (_camera.orthographic)
            {
                Plane nearPlane = new Plane(cameraTransform.forward, cameraTransform.position + cameraTransform.forward * _camera.nearClipPlane);
                float planeDistanceToPoint = nearPlane.GetDistanceToPoint(point);

                // The point is visible only if its distance between the near plane and the point is <= to 'distanceFromCameraPositionToSphereCenter'.
                return Mathf.Abs(planeDistanceToPoint) <= distanceFromCameraPositionToSphereCenter;
            }
            else
            {
                // The point is visible only if its distance between the camera position and the point is <= to 'distanceFromCameraPositionToSphereCenter'.
                Vector3 fromCameraPositionToPoint = point - cameraTransform.position;
                return fromCameraPositionToPoint.magnitude <= distanceFromCameraPositionToSphereCenter;
            }
        }

        /// <summary>
        /// Calculates the points which are needed to draw the rotation circles. 
        /// The points are calculated in gizmo local space. We will do this whenever
        /// the gizmo object is created so that we can keep them around then we need
        /// to render the rotation circles. The reason that these are cached is that
        /// we want to avoid generating them every time the circles need to be drawn.
        /// Generating those points involves a lot of calls to the 'Sin' and 'Cos'
        /// functions which can be expensive.
        /// </summary>
        private void CalculateRotationCirclePointsInGizmoLocalSpace()
        {
            _rotationCirclePointsForXAxisInLocalSpace = CalculateRotationCirclePointsInInGizmoLocalSpace(GizmoAxis.X);
            _rotationCirclePointsForYAxisInLocalSpace = CalculateRotationCirclePointsInInGizmoLocalSpace(GizmoAxis.Y);
            _rotationCirclePointsForZAxisInLocalSpace = CalculateRotationCirclePointsInInGizmoLocalSpace(GizmoAxis.Z);
        }

        /// <summary>
        /// Calculates the rotation circle points for the specified gizmo axis in gizmo local space.
        /// </summary>
        private Vector3[] CalculateRotationCirclePointsInInGizmoLocalSpace(GizmoAxis gizmoAxis)
        {
            // Calculate the circle radius.
            // Note: Calling 'GetRotationCircleRadius' would be a mistake. Becasue that function returns the
            //       radius in world space and we are currently working in gizmo local space.
            float circleRadius = _rotationSphereRadius * _rotationCircleRadiusScale;

            // Each circle is generated differently depending on the specified gizmo axis. The following lines of code calculate
            // a rotation matrix which is used to establish the final position of the vertices in gizmo local space.
            // Note: The rotation matrix is needed because as we will see in a few lines of code, we will generate the points
            //       inside a 'for' loop and the points will be generated inside the XZ plane. But each circle exists in a
            //       different plane so we will have to rotate those points accordingly based on the specified gizmo axis.
            Matrix4x4 rotationMatrix = new Matrix4x4();
            if (gizmoAxis == GizmoAxis.X) rotationMatrix.SetTRS(Vector3.zero, Quaternion.Euler(0.0f, 0.0f, 90.0f), Vector3.one);
            else if (gizmoAxis == GizmoAxis.Y) rotationMatrix = Matrix4x4.identity;
            else rotationMatrix.SetTRS(Vector3.zero, Quaternion.Euler(90.0f, 0.0f, 0.0f), Vector3.one);

            // Create the array which will hold the circle points
            const int numberOfPointsInCircle = 100;
            Vector3[] circlePoints = new Vector3[numberOfPointsInCircle];

            // Generate the points by rotation around the Y axis in gizmo local space. For each generated point,
            // we then transform it using the calculated rotation matrix in order to obtain the final version of
            // the point which corresponds to the specified gizmo axis.
            float angleStep = 360.0f / (numberOfPointsInCircle - 1);
            for (int pointIndex = 0; pointIndex < numberOfPointsInCircle; ++pointIndex)
            {
                // Generate the point by rotating around the Y axis in gizmo local space
                Vector3 linePoint = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleStep * pointIndex) * circleRadius, 0.0f,
                                                Mathf.Sin(Mathf.Deg2Rad * angleStep * pointIndex) * circleRadius);

                // Transform the generated point by the calculated matrix and store it inside the array which will be returned to the cient code
                linePoint = rotationMatrix.MultiplyPoint(linePoint);
                circlePoints[pointIndex] = linePoint;
            }

            // Return the circle points array
            return circlePoints;
        }

        /// <summary>
        /// Returns the radius of the rotation sphere in world space.
        /// </summary>
        private float GetWorldSpaceRotationSphereRadius()
        {
            return _rotationSphereRadius * CalculateGizmoScale();
        }

        /// <summary>
        /// Returns the rotation circle radius in world space.
        /// </summary>
        private float GetWorldSpaceRotationCircleRadius()
        {
            return GetWorldSpaceRotationSphereRadius() * _rotationCircleRadiusScale;
        }

        /// <summary>
        /// Returns the length of the cylinder (i.e. the length of the cylinder axis) which
        /// is needed when checking if a ray intersects one of the rotation circles. This
        /// method is used inside the 'RayIntersectsRotationCircle' method.
        /// </summary>
        private float GetRotationCircleCylinderAxisLength()
        {
            // The values are purely experimental
            if (_camera.orthographic) return 0.4f;
            else return 0.2f * CalculateGizmoScale();
        }

        /// <summary>
        /// Returns the epsilon value which si used to test the intersection between the
        /// mouse cursor and one of the 3 rotation circles. This method is used inside the
        /// 'RayIntersectsRotationCircle' method.
        /// </summary>
        private float GetRotationCircleIntersectionEpsilon()
        {
            // The values are purely experimental
            if (_camera.orthographic) return 0.35f;
            else return 0.2f * CalculateGizmoScale();
        }

        /// <summary>
        /// Checks if the specified ray intersects the rotation circle described by the parameters.
        /// </summary>
        /// <param name="ray">
        /// The ray involved in the intersection test.
        /// </param>
        /// <param name="circlePlaneNormal">
        /// The normal of the plane on which the circle lies.
        /// </param>
        /// <param name="circleRadius">
        /// The circle radius.
        /// </param>
        /// <param name="intersectionPoint">
        /// If an intersection happens, this will hold the intersectino point. Otherwise, it will be
        /// set to the zero vector.
        /// </param>
        /// <returns>
        /// True if an intersection happens and false otherwise.
        /// </returns>
        private bool RayIntersectsRotationCircle(Ray ray, Vector3 circlePlaneNormal, float circleRadius, out Vector3 intersectionPoint)
        {
            intersectionPoint = Vector3.zero;

            // For different camera angles, some intersection test methods work better than the other. We will always
            // start with a ray per 3D circle test. If that fails, we will check if the ray intersects the cylinder
            // which extends along the circle plane normal.
            float t;
            if (ray.Intersects3DCircle(_gizmoTransform.position, circleRadius, circlePlaneNormal, true, GetRotationCircleIntersectionEpsilon(), out t))
            {
                // The ray intersects the 3D circle. Calculate the intersection point and return true.
                intersectionPoint = ray.origin + ray.direction * t;
                return true;
            }
            else
            {
                // The first test failed, so we need to perform the cylinder intersection test. The following variables hold the cylinder axis points.
                // Note: In order to calculate the cylinder axis points, we will first need to retrieve the cylinder axis length via a call to the 
                //       'GetRotationCircleCylinderAxisLength' method. After we have done that, we will move from the center of the circle in both
                //       directions along the circle plane normal by an amount equal to half the cylinder length to get the 2 axes points.
                float rotationCircleCylinderAxisLength = GetRotationCircleCylinderAxisLength();
                Vector3 cylinderAxisFirstPoint = _gizmoTransform.position - circlePlaneNormal * rotationCircleCylinderAxisLength * 0.5f;
                Vector3 cylinderAxisSecondPoint = _gizmoTransform.position + circlePlaneNormal * rotationCircleCylinderAxisLength * 0.5f;

                // Check if the ray intersects the cylinder
                if (ray.IntersectsCylinder(cylinderAxisFirstPoint, cylinderAxisSecondPoint, circleRadius, out t))
                {
                    // The ray intersects the cylinder. Calculate the intersection point and return true.
                    intersectionPoint = ray.origin + ray.direction * t;
                    return true;
                }
            }

            // If we reached this point it means that the ray does not intersect the circle at all.
            return false;
        }

        /// <summary>
        /// Rotates all controlled objects by the specified angle in degrees around the specified axis.
        /// </summary>
        private void RotateControlledObjects(Vector3 rotationAxis, float angleInDegrees)
        {
            if (ControlledObjects != null)
            {
                // Just make sure the rotation axis is normalized
                rotationAxis.Normalize();

                // Retrieve the top parents from the controlled game object collection.
                // Note: Please see the comments inside 'TranslationGizmo.TranslateControlledObjects' to understand
                //       why we need to perform this step.
                List<GameObject> topParents = GetParentsFromControlledObjects(true);

                if(topParents.Count != 0)
                {
                    // We need to handle the rotation differently based on the specified pivot point
                    if (_transformPivotPoint == TransformPivotPoint.Center)
                    {
                        // Loop through all game objects
                        foreach (GameObject topParent in topParents)
                        {
                            // Rotate the object around the position of the gizmo.
                            // Note: This code assumes that the gizmo is placed at the center of selection. It will still
                            //       work otherwise, but this is the intended behaviour.
                            if (topParent != null)
                            {
                                topParent.Rotate(rotationAxis, angleInDegrees, _gizmoTransform.position);

                                // The game objects were transformed since the left mouse button was pressed
                                _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
                            }
                        }
                    }
                    else
                    {
                        // Loop through all game objects
                        foreach (GameObject topParent in topParents)
                        {
                            // Rotate the object
                            if (topParent != null)
                            {
                                topParent.transform.Rotate(rotationAxis, angleInDegrees, Space.World);

                                // The game objects were transformed since the left mouse button was pressed
                                _objectsWereTransformedSinceLeftMouseButtonWasPressed = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the rotation sphere.
        /// </summary>
        private void DrawRotationSphere(Matrix4x4 worldTransform)
        {
            Material material = MaterialPool.Instance.GizmoSolidComponent;
            material.SetVector("_LightDir", _camera.transform.forward);
            material.SetInt("_IsLit", _isRotationSphereLit ? 1 : 0);
            material.SetColor("_Color", _rotationSphereColor);

            material.SetPass(0);
            Graphics.DrawMeshNow(MeshPool.Instance.SphereMesh, worldTransform);
        }

        /// <summary>
        /// Returns the transform matrix which holds the world transform for the rotation sphere.
        /// </summary>
        private Matrix4x4 GetRotationSphereWorldTransform()
        {
            return Matrix4x4.TRS(_gizmoTransform.position, _gizmoTransform.rotation, _gizmoTransform.lossyScale * _rotationSphereRadius);
        }

        #if UNITY_EDITOR
        /// <summary>
        /// We will use this to draw a rough representation of the gizmo inside the scene view.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Save gimzos color and transform matrix
            Color oldGizmosColor = Gizmos.color;
            Matrix4x4 oldTransformMatrix = Gizmos.matrix;

            // Activate the gizmo transform matrix
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_gizmoBaseScale, _gizmoBaseScale, _gizmoBaseScale));

            // Draw the rotation sphere
            float rotationSphereRadius = _rotationSphereRadius;
            Gizmos.color = _rotationSphereColor;
            Gizmos.DrawSphere(Vector3.zero, rotationSphereRadius);

            // Draw the gizmo rotation circles
            DrawRotationCircleInSceneView(GizmoAxis.X);
            DrawRotationCircleInSceneView(GizmoAxis.Y);
            DrawRotationCircleInSceneView(GizmoAxis.Z);

            // Draw the rotation guide. We will just use a random rotation value for this.
            const float rotationAngle = 30.0f;
            DrawRotationGuideInSceneView(rotationAngle);

            // Draw the camera look rotation circle
            DrawCameraLookRotationCircleInSceneView();

            // Restore old gizmos color and matrix
            Gizmos.color = oldGizmosColor;
            Gizmos.matrix = oldTransformMatrix;
        }

        /// <summary>
        /// Draws a rotation circle inside the scene view for the specified gizmo axis.
        /// </summary>
        private void DrawRotationCircleInSceneView(GizmoAxis gizmoAxis)
        {
            Gizmos.color = _axesColors[(int)gizmoAxis];

            // Generate the circle points and draw the circle
            Vector3[] circlePoints = GenerateRotationCirclePoints(gizmoAxis, _rotationCircleRadiusScale);
            DrawCircleInSceneView(circlePoints);
        }

        /// <summary>
        /// Draws a circle inside the scene view using the specified circle points.
        /// </summary>
        private void DrawCircleInSceneView(Vector3[] circlePoints)
        {
            // Draw lines between the points
            for (int pointIndex = 0; pointIndex < circlePoints.Length; ++pointIndex)
            {
                Gizmos.DrawLine(circlePoints[pointIndex], circlePoints[(pointIndex + 1) % circlePoints.Length]);
            }
        }

        /// <summary>
        /// Generates and returns a series of points on the circle which surrounds the specified
        /// gizmo axis.
        /// </summary>
        private Vector3[] GenerateRotationCirclePoints(GizmoAxis gizmoAxis, float circleRadiusScale = 1.0f)
        {
            const int numberOfPointInCircle = 100;
            float circleRadius = _rotationSphereRadius * circleRadiusScale;

            // We will need to apply a rotation to the circle points based on the specified gizmo axis because
            // inside the 'for' loop the circles will be generated in the XZ plane, but the points will need
            // to be rotated depending on the specified input gizmo axis.
            Quaternion rotation;
            if (gizmoAxis == GizmoAxis.X) rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
            else if (gizmoAxis == GizmoAxis.Y) rotation = Quaternion.identity;
            else rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            // Generate the circle points
            Vector3[] circlePoints = new Vector3[numberOfPointInCircle];
            float angleStep = 360.0f / (numberOfPointInCircle - 1);
            for (int pointIndex = 0; pointIndex < numberOfPointInCircle; ++pointIndex)
            {
                // Generate the point
                float currentAngleInDegrees = angleStep * pointIndex * Mathf.Deg2Rad;
                Vector3 currentPoint = new Vector3(Mathf.Cos(currentAngleInDegrees) * circleRadius, 0.0f, Mathf.Sin(currentAngleInDegrees) * circleRadius);

                // Rotate the point and store it inside the array
                currentPoint = rotation * currentPoint;
                circlePoints[pointIndex] = currentPoint;
            }

            return circlePoints;
        }

        /// <summary>
        /// Draws the rotation guide inside the scene view. This includes the rotation disc and
        /// the 2 lines which connect the disc extreme points to the sphere center.
        /// </summary>
        /// <param name="angleInDegrees">
        /// This represents the angle in degrees that the guide should show.
        /// </param>
        private void DrawRotationGuideInSceneView(float angleInDegrees)
        {
            if (_showRotationGuide)
            {
                // Draw the rotation guide lines. We will simulate a rotation around the Y axis of 'angleInDegrees' degrees.
                Gizmos.color = _rotationGuieLineColor;
                Vector3 firstGuidePoint = new Vector3(_rotationSphereRadius * _rotationCircleRadiusScale, 0.0f, 0.0f);
                Quaternion destinationRotation = Quaternion.Euler(0.0f, angleInDegrees, 0.0f);
                Vector3 secondGuidePoint = destinationRotation * firstGuidePoint;
                Gizmos.DrawLine(Vector3.zero, firstGuidePoint);
                Gizmos.DrawLine(Vector3.zero, secondGuidePoint);

                // Draw the rotation guide disc. We will approximate it with a bunch of lines. The lines are
                // drawn by connecting the rotation sphere center with points which lie on the disc. We will
                // use 800 points for a 180 degree rotation.
                const int numberOfPointsFor180Degrees = 800;
                int actualNumberOfPoints = (int)((float)numberOfPointsFor180Degrees * angleInDegrees / 180.0f);

                // Draw lines between the disc center and the points on the disc
                Gizmos.color = _rotationGuideDiscColor;
                float discRadius = _rotationSphereRadius;
                float tStep = 1.0f / (actualNumberOfPoints - 1);
                for (int pointIndex = 0; pointIndex < actualNumberOfPoints; ++pointIndex)
                {
                    // Generate the point by moving away from the disc start point towards the second point by a factor of 'tStep * pointIndex'.
                    Quaternion rotation = Quaternion.Slerp(Quaternion.identity, destinationRotation, tStep * pointIndex);
                    Vector3 pointOnDisc = new Vector3(discRadius, 0.0f, 0.0f);
                    pointOnDisc = rotation * pointOnDisc;

                    // Draw a line between the generated point and the center of the disc
                    Gizmos.DrawLine(Vector3.zero, pointOnDisc);
                }
            }
        }

        /// <summary>
        /// Draws the camera look rotation circle in scene view.
        /// </summary>
        /// <remarks>
        /// Unlike drawing the circle at runtime, in the scene view, the circle will not
        /// be rendered in screen space. This kees things a little bit easier.
        /// </remarks>
        private void DrawCameraLookRotationCircleInSceneView()
        {
            if (_showCameraLookRotationCircle)
            {
                Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

                // Set the color and matrix.
                Gizmos.color = _cameraLookRotationCircleLineColor;
                Matrix4x4 transformMatrix = new Matrix4x4();
                transformMatrix.SetTRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.matrix = transformMatrix;

                // Generate the points and draw the circle
                Vector3[] circlePoints = GenerateRotationCirclePoints(GizmoAxis.Z, _cameraLookRotationCircleRadiusScale);
                DrawCircleInSceneView(circlePoints);

                // Restore the gizmos matrix
                Gizmos.matrix = oldGizmosMatrix;
            }
        }
        #endif
        #endregion
    }
}
