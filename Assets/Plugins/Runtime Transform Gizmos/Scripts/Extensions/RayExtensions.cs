using UnityEngine;
using RTEditor;

namespace RTEditor
{
    /// <summary>
    /// This is a static class that implements some useful 'Ray' extension methods.
    /// </summary>
    public static class RayExtensions
    {
        #region Public Static Functions
        /// <summary>
        /// Checks if the specified ray intersects the specified 3D circle. A 3D circle is a flat
        /// circle which sits on a plane in 3D space.
        /// </summary>
        /// <param name="ray">
        /// The ray involved in the intersection test.
        /// </param>
        /// <param name="circleCenter">
        /// The center of the circle.</param>
        /// <param name="circleRadius">The circle's radius.
        /// </param>
        /// <param name="circlePlaneNormal">
        /// The normal of the plane on which the circle lies.
        /// </param>
        /// <param name="acceptOnlyCircumference">
        /// If this is set to true, the function will return true only if the intersection point between
        /// the ray and the circle lies on the circle's circumference. This can be set to false in order
        /// to allow for intersection points which lie inside the circle.
        /// </param>
        /// <param name="circumferenceEpsilon">
        /// This is only used if 'acceptOnlyCircumference' is true. It represents the epsilon value that
        /// will be used when testing whether or not the intersection point lies on the circle's circumference.
        /// </param>
        /// <param name="t">
        /// When the function returns true, this holds the offset along the ray at which the intersection
        /// happens. Whenever the function returns false, this will be set to 0.0f.
        /// </param>
        /// <returns>
        /// True if the ray intersects the circle and false otherwise.
        /// </returns>
        public static bool Intersects3DCircle(this Ray ray, Vector3 circleCenter, float circleRadius, Vector3 circlePlaneNormal, bool acceptOnlyCircumference, float circumferenceEpsilon, out float t)
        {
            t = 0.0f;

            // Construct the circle plane and check if the ray intersects it. If the ray does not intersect
            // the plane, it means it doesn't intersect the circle either. In order to construct the plane,
            // we need the plane normal and a point known to be on the plane. The plane normal is given to
            // us by 'circlePlaneNormal' and the point on the plane is the center of the circle.
            Plane circlePlane = new Plane(circlePlaneNormal, circleCenter);
            if (circlePlane.Raycast(ray, out t))
            {
                // Calculate the intersection point between the ray and the plane
                Vector3 intersectionPoint = ray.origin + ray.direction * t;
                if (acceptOnlyCircumference)
                {
                    // When 'acceptOnlyCircumference' is true, the distance between the intersection point and the ideal point
                    // on the circumference of the circle must be <= than the specified epsilon. So, the first step which must
                    // be performed is to calculate the ideal point on the circumference of the circle. We do this by constructing
                    // a vector from the center of the circle to the point of intersection. We then normalize this vector and 
                    // use it to move from the center of the circle on its circumference. The resulting point is stored inside
                    // the 'pointOnCircumference' variable.
                    Vector3 fromCenterToIntersectionPoint = intersectionPoint - circleCenter;
                    fromCenterToIntersectionPoint.Normalize();
                    Vector3 pointOnCircumference = circleCenter + fromCenterToIntersectionPoint * circleRadius;

                    // Now, we need to check if the distance between the ideal point (pointOnCircumference) and the intersection
                    // point is acceptable. We do this by performing the difference between the 2 points and then checking if the
                    // magnitude of the resulting vector is <= then the specified epsilon value.
                    bool wasIntersected = (intersectionPoint - pointOnCircumference).magnitude <= circumferenceEpsilon;
                    if (!wasIntersected) t = 0.0f;

                    return wasIntersected;
                }
                else
                {
                    // When 'acceptOnlyCircumference' is false, the distance between the center of the circle and intersection point
                    // must be <= the cicle radius.
                    bool wasIntersected = (intersectionPoint - circleCenter).magnitude <= circleRadius;
                    if (!wasIntersected) t = 0.0f;

                    return wasIntersected;
                }
            }

            // If this point is reached it means the ray does not intersect the circle plane, and so, it doesn't intersect the circle either.
            return false;
        }

        /// <summary>
        /// Checks if the specified ray intersects the specified sphere.
        /// </summary>
        /// <param name="ray">
        /// The ray involved in the intersection test.
        /// </param>
        /// <param name="sphereCenter">
        /// The center of the sphere.
        /// </param>
        /// <param name="sphereRadius">
        /// The sphere's radius.
        /// </param>
        /// <param name="t">
        /// The offset along the ray at which the intersection happens. Whenever the function returns 
        /// false, this will be set to 0.0f. When checking the intersection between a ray and a sphere,
        /// we can obtain 2 intersection points and thus 2 t values. The function will always return 
        /// the smallest positive t value. If both t values are negative, it means the ray only intersects
        /// the sphere from behind and in that case the method will return false.
        /// </param>
        /// <returns>
        /// True if the ray intersects the sphere and false otherwise.
        /// </returns>
        public static bool IntersectsSphere(this Ray ray, Vector3 sphereCenter, float sphereRadius, out float t)
        {
            t = 0.0f;

            // Calculate the coefficients of the quadratic equation
            Vector3 sphereCenterToRayOrigin = ray.origin - sphereCenter;
            float a = Vector3.SqrMagnitude(ray.direction);
            float b = 2.0f * Vector3.Dot(ray.direction, sphereCenterToRayOrigin);
            float c = Vector3.SqrMagnitude(sphereCenterToRayOrigin) - sphereRadius * sphereRadius;

            // If we have a solution to the equation, the ray most likely intersects the sphere.
            float t1, t2;
            if (Equation.SolveQuadratic(a, b, c, out t1, out t2))
            {
                // Make sure the ray doesn't intersect the sphere only from behind
                if (t1 < 0.0f && t2 < 0.0f) return false;

                // Make sure we are using the smallest positive t value
                if (t1 < 0.0f)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                t = t1;

                return true;
            }

            // If we reach this point it means the ray does not intersect the sphere in any way
            return false;
        }

        /// <summary>
        /// Checks if the specified ray intersects the specified cylinder.
        /// </summary>
        /// <param name="ray">
        /// The ray involved in the intersection test.
        /// </param>
        /// <param name="cylinderAxisFirstPoint">
        /// The first point which makes up the cylinder axis.
        /// </param>
        /// <param name="cylinderAxisSecondPoint">
        /// The second point which makes up the cylinder axis.
        /// </param>
        /// <param name="cylinderRadius">
        /// The radius of the cylinder.
        /// </param>
        /// <param name="t">
        /// The offset along the ray at which the intersection happens. Whenever the function returns 
        /// false, this will be set to 0.0f. When checking the intersection between a ray and a cylinder,
        /// we can obtain 2 intersection points and thus 2 t values. The function will always return 
        /// the smallest positive t value. If both t values are negative, it means the ray only intersects
        /// the cylinder from behind and in that case the method will return false.
        /// </param>
        /// <returns>
        /// True if the ray intersects the cylinder and false otherwise.
        /// </returns>
        public static bool IntersectsCylinder(this Ray ray, Vector3 cylinderAxisFirstPoint, Vector3 cylinderAxisSecondPoint, float cylinderRadius, out float t)
        {
            t = 0;

            // We will need the length of the cylinder axis later. We also need to normalize the
            // cylinder axis in order to use it for the quadratic coefficients calculation.
            Vector3 cylinderAxis = cylinderAxisSecondPoint - cylinderAxisFirstPoint;
            float cylinderAxisLength = cylinderAxis.magnitude;
            cylinderAxis.Normalize();

            // We need these for the quadratic coefficients calculation
            Vector3 crossRayDirectionCylinderAxis = Vector3.Cross(ray.direction, cylinderAxis);
            Vector3 crossToOriginCylinderAxis = Vector3.Cross((ray.origin - cylinderAxisFirstPoint), cylinderAxis);

            // Calculate the quadratic coefficients
            float a = crossRayDirectionCylinderAxis.sqrMagnitude;
            float b = 2.0f * Vector3.Dot(crossRayDirectionCylinderAxis, crossToOriginCylinderAxis);
            float c = crossToOriginCylinderAxis.sqrMagnitude - cylinderRadius * cylinderRadius;

            // If we have a solution to the equation, the ray most likely intersects the cylinder.
            float t1, t2;
            if (Equation.SolveQuadratic(a, b, c, out t1, out t2))
            {
                // Make sure the ray doesn't intersect the cylinder only from behind
                if (t1 < 0.0f && t2 < 0.0f) return false;

                // Make sure we are using the smallest positive t value
                if (t1 < 0.0f)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                t = t1;

                // Now make sure that the intersection point lies within the boundaries of the cylinder axis. That is,
                // make sure its projection on the cylinder axis lies between the first and second axis points. We will 
                // do this by constructing a vector which goes from the cylinder axis first point to the intersection
                // point. We then project the resulting vector on the cylinder axis and analyze the result. If the result
                // is less than 0, it means the intersection point exists below the first cylinder axis point. If the
                // result is greater than the cylinder axis length, it means the intersection point exists above the 
                // second cylinder axis point. In both of these cases, we will return false because it means the intersection
                // point is outside the cylinder axis range.
                Vector3 intersectionPoint = ray.origin + ray.direction * t;
                float projection = Vector3.Dot(cylinderAxis, (intersectionPoint - cylinderAxisFirstPoint));

                // Below the first cylinder axis point?
                if (projection < 0.0f) return false;

                // Above the second cylinder axis point?
                if (projection > cylinderAxisLength) return false;

                // The intersection point is valid, so we can return true
                return true;
            }

            // If we reach this point, it means the ray does not intersect the cylinder in any way
            return false;
        }

        /// <summary>
        /// Checks if the specified ray intersects the specified cone.
        /// </summary>
        /// <param name="ray">
        /// The ray involved in the intersection test.
        /// </param>
        /// <param name="coneBaseRadius">
        /// The radius of the cone's base.
        /// </param>
        /// <param name="coneHeight">
        /// The cone's height.
        /// </param>
        /// <param name="coneTransformMatrix">
        /// This is a 4x4 matrix which describes the rotation, scale and position of the cone in 3D space.
        /// </param>
        /// <param name="t">
        /// The offset along the ray at which the intersection happens. Whenever the function returns 
        /// false, this will be set to 0.0f. When checking the intersection between a ray and a cone,
        /// we can obtain 2 intersection points and thus 2 t values. The function will always return 
        /// the smallest positive t value. If both t values are negative, it means the ray only intersects
        /// the cone from behind and in that case the method will return false.
        /// </param>
        /// <returns>
        /// True if the ray intersects the cone and false otherwise.
        /// </returns>
        public static bool IntersectsCone(this Ray ray, float coneBaseRadius, float coneHeight, Matrix4x4 coneTransformMatrix, out float t)
        {
            t = 0.0f;

            // Because of the way in which the cone equation works, we will need to work with a ray which exists
            // in the cone's local space. That will make sure that we are dealing with a cone which sits at the
            // origin of the coordinate system with its height axis extending along the global up vector.
            Ray coneSpaceRay = ray.InverseTransform(coneTransformMatrix);

            // We will first perform a preliminary check to see if the ray intersects the bottom cap of the cone.
            // This is necessary because the cone equation views the cone as infinite (i.e. no bottom cap), and
            // if we didn't perform this check, we would never be able to tell when the bottom cap was hit and we
            // would also get false positives when the ray intersects the 'imaginary' part of the cone.
            // Note: In order to calculate the cone's bottom cap plane, we need to know the plane normal and a 
            //       point known to be on the plane. Remembering that we are currently working in cone local space,
            //       the bottom cap plane normal is pointing downwards along the Y axis and the point on the plane
            //       is the center of the bottom cap, which is the zero vector. This is the standard default pose
            //       for a cone object.
            float rayOffset;
            Plane bottomCapPlane = new Plane(-Vector3.up, Vector3.zero);
            if (bottomCapPlane.Raycast(coneSpaceRay, out rayOffset))
            {
                // If the ray intersects the plane of the bottom cap, we will calculate the intersection point
                // and if it lies inside the cone's bottom cap area, it means we have a valid intersection. We
                // store the t value and then return true.
                Vector3 intersectionPoint = coneSpaceRay.origin + coneSpaceRay.direction * rayOffset;
                if (intersectionPoint.magnitude <= coneBaseRadius)
                {
                    t = rayOffset;
                    return true;
                }
            }

            // We need this for the calculation of the quadratic coefficients
            float ratioSquared = coneBaseRadius / coneHeight;
            ratioSquared *= ratioSquared;

            // Calculate the coefficients.
            // Note: The cone equation which was used is: (X^2 + Z^2) / ratioSquared = (Y - coneHeight)^2.
            //       Where X, Y and Z are the coordinates of the point along the ray: (Origin + Direction * t).xyz
            float a = coneSpaceRay.direction.x * coneSpaceRay.direction.x + coneSpaceRay.direction.z * coneSpaceRay.direction.z - ratioSquared * coneSpaceRay.direction.y * coneSpaceRay.direction.y;
            float b = 2.0f * (coneSpaceRay.origin.x * coneSpaceRay.direction.x + coneSpaceRay.origin.z * coneSpaceRay.direction.z - ratioSquared * coneSpaceRay.direction.y * (coneSpaceRay.origin.y - coneHeight));
            float c = coneSpaceRay.origin.x * coneSpaceRay.origin.x + coneSpaceRay.origin.z * coneSpaceRay.origin.z - ratioSquared * (coneSpaceRay.origin.y - coneHeight) * (coneSpaceRay.origin.y - coneHeight);

            // The intersection happnes only if the quadratic equation has solutions
            float t1, t2;
            if (Equation.SolveQuadratic(a, b, c, out t1, out t2))
            {
                // Make sure the ray does not intersect the cone only from behind
                if (t1 < 0.0f && t2 < 0.0f) return false;

                // Make sure we are using the smallest positive t value
                if (t1 < 0.0f)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                t = t1;

                // Make sure the intersection point does not sit below the cone's bottom cap or above the cone's cap
                Vector3 intersectionPoint = coneSpaceRay.origin + coneSpaceRay.direction * t;
                if (intersectionPoint.y < 0.0f || intersectionPoint.y > coneHeight) return false;

                // The intersection point is valid
                return true;
            }

            // If we reached this point, it means the ray does not intersect the cone in any way
            return false;
        }

        /// <summary>
        /// Checks if the specified ray intersects the specified box.
        /// </summary>
        /// <remarks>
        /// The function uses the 'Slabs' method for intersection testing.
        /// More info about this method can be found here: https://www.siggraph.org/education/materials/HyperGraph/raytrace/rtinter3.htm
        /// </remarks>
        /// <param name="ray">
        /// The ray which is involved in the intersection test.
        /// </param>
        /// <param name="boxWidth">
        /// The box width.
        /// </param>
        /// <param name="boxHeight">
        /// The box height.
        /// </param>
        /// <param name="boxDepth">
        /// The box depth.
        /// </param>
        /// <param name="boxTransformMatrix">
        /// The transform matrix which holds the box's transform information.
        /// </param>
        /// <param name="t">
        /// The offset along the ray at which the intersection happens. Whenever the function returns 
        /// false, this will be set to 0.0f. When checking the intersection between a ray and a box,
        /// we can obtain 2 intersection points and thus 2 t values. The function will always return 
        /// the smallest positive t value. If both t values are negative, it means the ray only intersects
        /// the box from behind and in that case the method will return false.
        /// </param>
        /// <returns>
        /// True if the ray intersects the box and false otherwise.
        /// </returns>
        public static bool IntersectsBox(this Ray ray, float boxWidth, float boxHeight, float boxDepth, Matrix4x4 boxTransformMatrix, out float t)
        {
            t = 0.0f;

            // The 'Slabs' method works with a ray and an AABB. The problem is that our box can have an
            // arbitrary orientation in the 3D world which means that we are actually dealing with an 
            // OBB (oriented bounding box). In order to still be able to use the 'Slabs' method, we will
            // transform the ray in the local space of the box. In box local space, the box is an AABB
            // because in its local space any geometric transformation (including rotation) has been cancelled.
            Ray boxSpaceRay = ray.InverseTransform(boxTransformMatrix);

            // Cache needed variables
            Vector3 rayOrigin = boxSpaceRay.origin;
            Vector3 rayDirection = boxSpaceRay.direction;

            // Calculate the reciprocal of the ray direction
            Vector3 rayDirectionReciprocal = new Vector3(1.0f / rayDirection.x, 1.0f / rayDirection.y, 1.0f / rayDirection.z);

            // We will need these to calculate the box extents
            float halfBoxWidth = boxWidth * 0.5f;
            float halfBoxHeight = boxHeight * 0.5f;
            float halfBoxDepth = boxDepth * 0.5f;

            // Calculate the box extent vectors
            Vector3 boxExtentsMin = new Vector3(-halfBoxWidth, -halfBoxHeight, -halfBoxDepth);
            Vector3 boxExtentsMax = new Vector3(halfBoxWidth, halfBoxHeight, halfBoxDepth);

            // The algorithm needs to keep track of the highest minimum and lowest maximum intersection values.
            // These are the intersection values with the slab planes.
            float highestMinimumT = float.MinValue;
            float lowestMaximumT = float.MaxValue;

            // Loop through each slab
            for (int slabIndex = 0; slabIndex < 3; ++slabIndex)
            {
                // Check if the corresponding ray direction component is different than 0. If it is
                // 0, it means the ray is running parallel to the current pair of slab planes and it
                // can't possibly intersect them. So we will only adjust our intersection values if
                // the ray direction vector can hit one of the slab planes.
                if (Mathf.Abs(rayDirection[slabIndex]) > 1e-4f)
                {
                    // Calculate the intersection values with the minimum and maximum slab planes. These are the planes
                    // on which the minimum and maximum extent points reside. The calculation is done using the geometric
                    // ray/plane intersection method and it is simplified a little bit due to the fact that we are working
                    // in box local space where the right, up and forward vectors of the box are always aligned with the
                    // global coordinate system axes.
                    float minimumT = (boxExtentsMin[slabIndex] - rayOrigin[slabIndex]) * rayDirectionReciprocal[slabIndex];
                    float maximumT = (boxExtentsMax[slabIndex] - rayOrigin[slabIndex]) * rayDirectionReciprocal[slabIndex];

                    // Reorder values
                    if (minimumT > maximumT)
                    {
                        float temp = minimumT;
                        minimumT = maximumT;
                        maximumT = temp;
                    }

                    // Adjust the global intersection values
                    if (minimumT > highestMinimumT) highestMinimumT = minimumT;
                    if (maximumT < lowestMaximumT) lowestMaximumT = maximumT;

                    // If at any point we find that the highest minimum intersection value is greater than the
                    // lowest maximum intersection value, it means the ray does not intersect the box.
                    if (highestMinimumT > lowestMaximumT) return false;

                    // The highest minimum intersection value will always be smaller than the lowest maximum intersection value
                    // (we made sure of that in the check above), but if the lowest maximum intersection value is smaller than
                    // 0, it means both intersection values are smaller than 0 in which case the ray intersects the box only if
                    // we travel along the reverse of the ray direction vector. In that case we will return false.
                    if (lowestMaximumT < 0.0f) return false;
                }
                else
                {
                    // If we reached this point it means the ray direction vector runs parallel to the
                    // current pair of slab planes. In this case we will perform a test which tells
                    // us if there is any need to continue. If the corresponding ray origin component
                    // is either smaller than the minimum extents or bigger than the maximum extents,
                    // it means the ray can not possible intersect the box because its origin lies
                    // outside the box and the ray's direction vector does not help either given that
                    // its velocity with respect to the current slab planes is 0.
                    if (rayOrigin[slabIndex] < boxExtentsMin[slabIndex] ||
                        rayOrigin[slabIndex] > boxExtentsMax[slabIndex]) return false;
                }
            }

            // Use the smallest positive t value
            t = highestMinimumT;
            if (t < 0.0f) t = lowestMaximumT;

            // We have an intersection
            return true;
        }

        /// <summary>
        /// Transforms a ray into the local space of an object/entity whose transform
        /// information is given by 'transformMatrix'.
        /// </summary>
        /// <param name="ray">
        /// The ray which must be transformed.
        /// </param>
        /// <param name="transformMatrix">
        /// The transform matrix whose inverse will be used to transform the ray.
        /// </param>
        /// <returns>
        /// The ray as it exists in the local space of the object/entity whose transform
        /// information is given by 'transformMatrix'.
        /// </returns>
        public static Ray InverseTransform(this Ray ray, Matrix4x4 transformMatrix)
        {
            // Calculate the inverse of the specified transform matrix. We will use this matrix
            // to transform the ray into the object/entity's local space.
            Matrix4x4 inverseTransform = transformMatrix.inverse;

            // Transform the origin and direction in the local space of the object/entity.
            // Note: When transforming the ray's direction, we will use the 'MultiplyVector' function,
            //       so that the multiplication takes rotation and scale into account, but not the
            //       position. The position should never affect a direction vector. 
            Vector3 origin = inverseTransform.MultiplyPoint(ray.origin);
            Vector3 direction = inverseTransform.MultiplyVector(ray.direction);

            // Return the transformed ray
            return new Ray(origin, direction);
        }
        #endregion
    }
}
