using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This class contains a series of functions which use the GL API to draw
    /// different types of primitives.
    /// </summary>
    public static class GLPrimitives
    {
        #region Public Static Functions
        /// <summary>
        /// Draws a 3D line.
        /// </summary>
        /// <param name="firstPoint">
        /// The line's first point.
        /// </param>
        /// <param name="secondPoint">
        /// The line's second point.
        /// </param>
        /// <param name="lineColor">
        /// The line color.
        /// </param>
        /// <param name="lineMaterial">
        /// The material which is used to draw the line.
        /// </param>
        public static void Draw3DLine(Vector3 firstPoint, Vector3 secondPoint, Color lineColor, Material lineMaterial)
        {
            // Apply the material
            lineMaterial.SetColor("_Color", lineColor);
            lineMaterial.SetPass(0);

            // Draw the line
            GL.Begin(GL.LINES);
            GL.Color(lineColor);
            GL.Vertex3(firstPoint.x, firstPoint.y, firstPoint.z);
            GL.Vertex3(secondPoint.x, secondPoint.y, secondPoint.z);
            GL.End();
        }

        /// <summary>
        /// Draws a collection of 3D lines.
        /// </summary>
        /// <param name="linePoints">
        /// The line points. When 'drawConnectedLines' is set to false, the number of elements
        /// in this array must be a multiple of 2.
        /// </param>
        /// <param name="lineColors">
        /// The line colors. There must be as many elements in this array as the number of
        /// lines which must be drawn. When 'drawConnectedLines' is set to true, each element
        /// in this array is mapped to each element in 'linePoints'. Otherwise, each element
        /// maps to each pair of points in 'linePoints'.
        /// </param>
        /// <param name="drawConnectedLines">
        /// If this parameter is set to true, the function will draw connected lines. This 
        /// means that each successive vertex is the beginning of a new line. If set to false,
        /// the function will draw a line between each pair of points.
        /// </param>
        /// <param name="lineMaterial">
        /// The material which must be used to draw the line.
        /// </param>
        /// <param name="loop">
        /// If this is set to true, the function will draw a line between the first and last points
        /// in the 'linePoints' array.
        /// </param>
        /// <param name="loopLineColor">
        /// If 'loop' is true, this holds the color of the line which connects the last and first points
        /// inside the 'linePoints' array.
        /// </param>
        public static void Draw3DLines(Vector3[] linePoints, Color[] lineColors, bool drawConnectedLines, Material lineMaterial, bool loop, Color loopLineColor)
        {
            // Apply the material
            lineMaterial.SetPass(0);

            // Cache the number of lines
            int numberOfLines = drawConnectedLines ? linePoints.Length - 1 : linePoints.Length / 2;

            // Draw the lines
            GL.Begin(GL.LINES);
            if (!drawConnectedLines)
            {
                for (int lineIndex = 0; lineIndex < numberOfLines; ++lineIndex)
                {
                    // Identify the first point in this line. This is the line index multiplied
                    // by 2 because each line has 2 vertices.
                    int firstPointIndex = lineIndex * 2;

                    // Store points for easy access
                    Vector3 firstPoint = linePoints[firstPointIndex];
                    Vector3 secondPoint = linePoints[firstPointIndex + 1];

                    // Set the line color
                    lineMaterial.SetColor("_Color", lineColors[lineIndex]);
                    GL.Color(lineColors[lineIndex]);

                    // Send the line vertices to the GL API 
                    GL.Vertex3(firstPoint.x, firstPoint.y, firstPoint.z);
                    GL.Vertex3(secondPoint.x, secondPoint.y, secondPoint.z);
                }
            }
            else
            {
                for (int lineIndex = 0; lineIndex < numberOfLines; ++lineIndex)
                {
                    // Store points for easy access.
                    // Note: The value of 'lineIndex' gives us the index of the first point and
                    //       the value of 'lineIndex + 1' gives us the index of the second line
                    //       point. This is becauase when drawing connected lines, the lines share
                    //       vertices.
                    Vector3 firstPoint = linePoints[lineIndex];
                    Vector3 secondPoint = linePoints[lineIndex + 1];

                    // Set the line color
                    lineMaterial.SetColor("_Color", lineColors[lineIndex]);
                    GL.Color(lineColors[lineIndex]);

                    // Send the line vertices to the GL API 
                    GL.Vertex3(firstPoint.x, firstPoint.y, firstPoint.z);
                    GL.Vertex3(secondPoint.x, secondPoint.y, secondPoint.z);
                }
            }

            // If we need to loop, we must draw an additional line between the last and first points
            // inside the 'linePoints' array.
            if (loop)
            {
                // Store points for easy access
                Vector3 firstPoint = linePoints[0];
                Vector3 secondPoint = linePoints[linePoints.Length - 1];

                // Set the line color
                lineMaterial.SetColor("_Color", loopLineColor);
                GL.Color(loopLineColor);

                // Send the line vertices to the GL API 
                GL.Vertex3(firstPoint.x, firstPoint.y, firstPoint.z);
                GL.Vertex3(secondPoint.x, secondPoint.y, secondPoint.z);
            }
            GL.End();
        }

        /// <summary>
        /// Draws a 3D line.
        /// </summary>
        /// <param name="firstPoint">
        /// The line's first point.
        /// </param>
        /// <param name="secondPoint">
        /// The line's second point.
        /// </param>
        /// <param name="firstPointColor">
        /// The color of the first line point.
        /// </param>
        /// <param name="secondPointColor">
        /// The color of the second line point.
        /// </param>
        /// <param name="lineMaterial">
        /// The material which is used to draw the line.
        /// </param>
        public static void Draw3DLine(Vector3 firstPoint, Vector3 secondPoint, Color firstPointColor, Color secondPointColor, Material lineMaterial)
        {
            // Apply the material
            lineMaterial.SetColor("_Color", firstPointColor);
            lineMaterial.SetPass(0);

            // Draw the line
            GL.Begin(GL.LINES);
            GL.Color(firstPointColor);
            GL.Vertex3(firstPoint.x, firstPoint.y, firstPoint.z);
            GL.Color(secondPointColor);
            GL.Vertex3(secondPoint.x, secondPoint.y, secondPoint.z);
            GL.End();
        }

        /// <summary>
        /// Draws a 2D line in screen space.
        /// </summary>
        /// <param name="firstPoint">
        /// The line's first point.
        /// </param>
        /// <param name="secondPoint">
        /// The line's second point.
        /// </param>
        /// <param name="lineColor">
        /// The line color.
        /// </param>
        /// <param name="lineMaterial">
        /// The material which is used to draw the line.
        /// </param>
        /// <param name="camera">
        /// The camera responsible for rendering the line.
        /// </param>
        public static void Draw2DLine(Vector2 firstPoint, Vector2 secondPoint, Color lineColor, Material lineMaterial, Camera camera)
        {
            // Apply the material
            lineMaterial.SetColor("_Color", lineColor);
            lineMaterial.SetPass(0);

            // Change the model view matrix so that it is suitable for 2D rendering
            GL.PushMatrix();
            GL.LoadOrtho();

            firstPoint = camera.ScreenToViewportPoint(firstPoint);
            secondPoint = camera.ScreenToViewportPoint(secondPoint);

            // Draw the line
            GL.Begin(GL.LINES);
            GL.Color(lineColor);
            GL.Vertex(new Vector3(firstPoint.x, firstPoint.y, 0.0f));
            GL.Vertex(new Vector3(secondPoint.x, secondPoint.y, 0.0f));
            GL.End();

            // Restore the old modelview matrix
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a 2D circle. The function does not draw a filled circle. It will only draw the circle 
        /// border lines.
        /// </summary>
        /// <param name="borderLinePoints">
        /// An array which contains the circle border line points in screen space.
        /// </param>
        /// <param name="circleCenter">
        /// The cricle's center in screen space. The function assumes the 'borderLinePoints' array
        /// holds the border points which were generated by rotating around 'circleCenter'. 
        /// </param>
        /// <param name="borderLineColor">
        /// The color used to draw the circle border lines.
        /// </param>
        /// <param name="radiusScale">
        /// The scale that must be applied to the circle's radius. This value does not represent the actual radius
        /// of the circle. The actual radius is determined by the function using the specified circle points and
        /// center. This value is only used to scale the radius that the function calculates automatically.
        /// </param>
        /// <param name="borderLineMaterial">
        /// The material which is used to draw the border line.
        /// </param>
        /// <param name="camera">
        /// The camera which is responsible for rendering the circle border line.
        /// </param>
        public static void Draw2DCircleBorderLines(Vector3[] borderLinePoints, Vector3 circleCenter, Color borderLineColor, float radiusScale, Material borderLineMaterial, Camera camera)
        {
            // Apply the material
            borderLineMaterial.SetColor("_Color", borderLineColor);
            borderLineMaterial.SetPass(0);

            // Change the model view matrix so that it is suitable for 2D rendering
            GL.PushMatrix();
            GL.LoadOrtho();

            // Determine the final radius of the circle. The actual radius is given by magnitude of the
            // vector which goes from the center of the circle to one of the circle points. We then scale
            // the magnitude by the specified radius scale to obtain the final radius.
            float circleRadius = (borderLinePoints[0] - circleCenter).magnitude * radiusScale;

            // Loop through each pair of points and draw a line between them
            GL.Begin(GL.LINES);
            GL.Color(borderLineColor);
            for (int pointIndex = 0; pointIndex < borderLinePoints.Length; ++pointIndex)
            {
                // We will need to apply a scale on the circle's radius. This means that we need to move
                // from the center in the direction of the 2 current points which are used to render the 
                // connecting line by an amount equal to the calculated radius. The first step is to calculate
                // 2 vectors which aim in the correct direction.
                Vector3 toFirstPoint = borderLinePoints[pointIndex] - circleCenter;
                Vector3 toSecondPoint = borderLinePoints[(pointIndex + 1) % borderLinePoints.Length] - circleCenter;

                // Normalize them so that we can use to travel by the correct amount
                toFirstPoint.Normalize();
                toSecondPoint.Normalize();

                // Move from the circle's center along the 2 vectors by an amount equal to the circle's radius
                Vector3 firstPoint = circleCenter + toFirstPoint * circleRadius;
                Vector3 secondPoint = circleCenter + toSecondPoint * circleRadius;

                firstPoint = camera.ScreenToViewportPoint(firstPoint);
                secondPoint = camera.ScreenToViewportPoint(secondPoint);

                // Draw the line which connects the 2 border line points
                GL.Vertex(new Vector3(firstPoint.x, firstPoint.y, 0.0f));
                GL.Vertex(new Vector3(secondPoint.x, secondPoint.y, 0.0f));
            }

            // End drawing and restore the old modelview matrix
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a 2D rectangle. The function does not draw a filled rectangle. It will only draw the 
        /// rectangle border lines.
        /// </summary>
        /// <param name="borderLinePoints">
        /// The rectangle norder line points. The function assumes that the points are specified in
        /// either clockwise or counter-clockwise consecutive order. 
        /// </param>
        /// <param name="borderLineColor">
        /// The border line color.
        /// </param>
        /// <param name="borderLineMaterial">
        /// The material used to draw the border lines.
        /// </param>
        /// <param name="camera">
        /// The camera responsible for rendering the rectangle border lines.
        /// </param>
        public static void Draw2DRectangleBorderLines(Vector2[] borderLinePoints, Color borderLineColor, Material borderLineMaterial, Camera camera)
        {
            // Apply the material
            borderLineMaterial.SetColor("_Color", borderLineColor);
            borderLineMaterial.SetPass(0);

            // Change the modelview matrix so that it is suitable for 2D rendering
            GL.PushMatrix();
            GL.LoadOrtho();

            // Loop through each pair of points and draw a line between them
            GL.Begin(GL.LINES);
            GL.Color(borderLineColor);
            for (int pointIndex = 0; pointIndex < borderLinePoints.Length; ++pointIndex)
            {
                // Store the points of the current edge for easy access
                Vector3 firstPoint = borderLinePoints[pointIndex];
                Vector3 secondPoint = borderLinePoints[(pointIndex + 1) % borderLinePoints.Length];

                firstPoint = camera.ScreenToViewportPoint(firstPoint);
                secondPoint = camera.ScreenToViewportPoint(secondPoint);

                // Draw the line which connects the 2 points
                GL.Vertex(new Vector3(firstPoint.x, firstPoint.y, 0.0f));
                GL.Vertex(new Vector3(secondPoint.x, secondPoint.y, 0.0f));
            }

            // End drawing and restore the old modelview matrix
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a 2D rectangle. The function does not draw a filled rectangle. It will only draw the 
        /// rectangle border lines.
        /// </summary>
        /// <param name="rectangle">
        /// An instance of the 'Rect' struct which descirbes the rectangle which must be drawn.
        /// </param>
        /// <param name="borderLineColor">
        /// The border line color.
        /// </param>
        /// <param name="borderLineMaterial">
        /// The material used to draw the border lines.
        /// </param>
        /// <param name="camera">
        /// The camera responsible for rendering the rectangle border lines.
        /// </param>
        public static void Draw2DRectangleBorderLines(Rect rectangle, Color borderLineColor, Material borderLineMaterial, Camera camera)
        {
            // Generate the rectangle points
            Vector2[] rectanglePoints = new Vector2[]
            {
                new Vector2(rectangle.xMin, rectangle.yMin),
                new Vector2(rectangle.xMax, rectangle.yMin),
                new Vector2(rectangle.xMax, rectangle.yMax),
                new Vector2(rectangle.xMin, rectangle.yMax)
            };

            // Draw the rectangle
            Draw2DRectangleBorderLines(rectanglePoints, borderLineColor, borderLineMaterial, camera);
        }

        /// <summary>
        /// Draws a 2D filled rectangle.
        /// </summary>
        /// <param name="rectangle">
        /// An instance of the 'Rect' struct which represents the rectangle that must be drawn.
        /// </param>
        /// <param name="rectangleColor">
        /// The rectangle color.
        /// </param>
        /// <param name="rectangleMaterial">
        /// The material which is used to draw the rectangle.
        /// </param>
        /// <param name="camera">
        /// The camera responsible for rendering the rectangle.
        /// </param>
        public static void Draw2DFilledRectangle(Rect rectangle, Color rectangleColor, Material rectangleMaterial, Camera camera)
        {
            // Apply the material
            rectangleMaterial.SetColor("_Color", rectangleColor);
            rectangleMaterial.SetPass(0);

            // Prepare to draw in 2D
            GL.PushMatrix();
            GL.LoadOrtho();

            // Begin drawing
            GL.Begin(GL.QUADS);
            GL.Color(rectangleColor);

            Vector3 point0 = camera.ScreenToViewportPoint(new Vector3(rectangle.xMin, rectangle.yMin, 0.0f));
            Vector3 point1 = camera.ScreenToViewportPoint(new Vector3(rectangle.xMax, rectangle.yMin, 0.0f));
            Vector3 point2 = camera.ScreenToViewportPoint(new Vector3(rectangle.xMax, rectangle.yMax, 0.0f));
            Vector3 point3 = camera.ScreenToViewportPoint(new Vector3(rectangle.xMin, rectangle.yMax, 0.0f));

            // Draw the rectangle
            GL.Vertex3(point0.x, point0.y, 0.0f);
            GL.Vertex3(point1.x, point1.y, 0.0f);
            GL.Vertex3(point2.x, point2.y, 0.0f);
            GL.Vertex3(point3.x, point3.y, 0.0f);

            // End drawing and restore the old modelview matrix
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a 3D filled disc.
        /// </summary>
        /// <param name="discCenter">
        /// The disc's center point. If we imagine that we complete the disc such that it becomes a full
        /// circle, this represents the center of the circle.
        /// </param>
        /// <param name="firstPoint">
        /// The first point which lies on the disc.
        /// </param>
        /// <param name="secondPoint">
        /// The second point which lies on the disc.
        /// </param>
        /// <param name="discPlaneNormal">
        /// This is is the normal of the plane on which the disc resides.
        /// </param>
        /// <param name="discColor">
        /// The disc color.
        /// </param>
        /// <param name="discMaterial">
        /// The material used to draw the disc.
        /// </param>
        public static void Draw3DFilledDisc(Vector3 discCenter, Vector3 firstPoint, Vector3 secondPoint, Vector3 discPlaneNormal, Color discColor, Material discMaterial)
        {
            // Construct 2 vectors which go from the center of the disc to both disc points.
            // We will need these vectors figure out how to draw the disc triangles.
            Vector3 toFirstPoint = firstPoint - discCenter;
            Vector3 toSecondPoint = secondPoint - discCenter;

            // Calculate the disc radius. This is just the distance between the disc center and one of
            // the disc points which we already have access to.
            float discRadius = toFirstPoint.magnitude;

            // Normalize the vectors calculated earlier
            toFirstPoint.Normalize();
            toSecondPoint.Normalize();

            // We will want to calculate the angle between the 2 disc vectors calculated eralier and for
            // that we will use the 'SafeAcos' function. The problem is that we need to figure out the direction
            // of rotation and that information is not returned by the 'SafeAcos' function. So, we will store the
            // sign (i.e. direction) of rotation inside the 'angleSign' variable and this is calculated by
            // performing a cross product between the 2 disc vectors. If the dot product between the resulting
            // vector and the disc normal vector is negative, we will set the sign to -1.0f. Otherwise, we will
            // set it to 1.0f.
            float angleSign = 1.0f;
            if (Vector3.Dot(Vector3.Cross(toFirstPoint, toSecondPoint), discPlaneNormal) < 0.0f) angleSign = -1.0f;
            else angleSign = 1.0f;

            // In order to draw the disc mesh, we will need to keep rotating the first vector towards the second one and
            // for that we need the angle between the 2 vectors. Applying the 'SafeAcos' function to the dot product between
            // the 2 vectors gives us the angle (the 2 vectors were normalized earlier). We then transform the angle in
            // degree units and multiply by the sign value to establish the direction of rotation. After all this is done,
            // we construct a quaternion, 'destinationQuaternion', which can be used to rotate the first vector towards the
            // second one. We will use the 'Slerp' function to rotate the first vector towards the second vector in equal
            // angle increments.
            discPlaneNormal.Normalize();
            float angleInDegrees = MathHelper.SafeAcos(Vector3.Dot(toFirstPoint, toSecondPoint)) * Mathf.Rad2Deg * angleSign;
            Quaternion destinationQuaternion = Quaternion.AngleAxis(angleInDegrees, discPlaneNormal);

            // Calculate the number of points on the circumference of the disc. We will use a number of 'numberOfPointsFor180Degrees'
            // for an 180 degree disc. The actual number of points for the disc that we want to draw is that value scaled by the
            // ration between the calculated angle and 180.0f.
            const int numberOfPointsFor180Degrees = 180;
            int actualNumberOfPoints = (int)((float)numberOfPointsFor180Degrees * Mathf.Abs(angleInDegrees) / 180.0f);
            if (actualNumberOfPoints < 2) return;

            // Apply the material
            discMaterial.SetColor("_Color", discColor);
            discMaterial.SetPass(0);

            // Begin drawing
            GL.Begin(GL.TRIANGLES);
            GL.Color(discColor);

            // Now we need to draw the disc triangles. We will need to generate the disc vertices as we go
            // and for that we will use the 'Slerp' function which will rotate from the first disc vector
            // towards the second one. In order to do that, we calculate the increment step for the slerp
            // interpolation factor and that will make sure that we always rotated in equal angle steps.
            float tStep = 1.0f / (actualNumberOfPoints - 1);
            Vector3 previousVertex = discCenter + toFirstPoint * discRadius;
            for (int pointIndex = 0; pointIndex < actualNumberOfPoints; ++pointIndex)
            {
                // Rotate towards the second vector
                Quaternion rotation = Quaternion.Slerp(Quaternion.identity, destinationQuaternion, tStep * (float)pointIndex);
                Vector3 rotatedVector = rotation * toFirstPoint;
                rotatedVector.Normalize();

                // The current triangle is formed by the disc's center point, the vertex stored inside 'vertex' (generated
                // with the help of the rotated vector), and the vertex which comes before it.
                Vector3 vertex = discCenter + rotatedVector * discRadius;
                GL.Vertex(discCenter);
                GL.Vertex(vertex);
                GL.Vertex(previousVertex);

                // Update the previous vertex for the next triangle that we need to draw
                previousVertex = vertex;
            }

            // End drawing
            GL.End();
        }

        /// <summary>
        /// Draws a 2D filled disc.
        /// </summary>
        /// <param name="discCenter">
        /// The disc's center point. If we imagine that we complete the disc such that it becomes a full
        /// circle, this represents the center of the circle.
        /// </param>
        /// <param name="firstPoint">
        /// The first point which lies on the disc.
        /// </param>
        /// <param name="secondPoint">
        /// The second point which lies on the disc.
        /// </param>
        /// <param name="discColor">
        /// The disc color.
        /// </param>
        /// <param name="discMaterial">
        /// The material used to draw the disc.
        /// </param>
        /// <param name="camera">
        /// The camera responsible for rendering the disc.
        /// </param>
        public static void Draw2DFilledDisc(Vector2 discCenter, Vector2 firstPoint, Vector2 secondPoint, Color discColor, Material discMaterial, Camera camera)
        {
            // Construct 2 vectors which go from the center of the disc to both disc points.
            // We will need these vectors figure out how to draw the disc triangles.
            Vector2 toFirstPoint = firstPoint - discCenter;
            Vector2 toSecondPoint = secondPoint - discCenter;

            // Calculate the disc radius. This is just the distance between the disc center and one of
            // the disc points which we already have access to.
            float discRadius = toFirstPoint.magnitude;

            // Normalize the vectors calculated earlier
            toFirstPoint.Normalize();
            toSecondPoint.Normalize();

            // We will want to calculate the angle between the 2 disc vectors calculated eralier and for
            // that we will use the 'SafeAcos' function. The problem is that we need to figure out the direction
            // of rotation and that information is not returned by the 'SafeAcos' function. So, we will store the
            // sign (i.e. direction) of rotation inside the 'angleSign' variable and this is calculated by
            // performing a cross product between the 2 disc vectors. If the dot product between the resulting
            // vector and the forward vector is negative, we will set the sign to -1.0f. Otherwise, we will
            // set it to 1.0f.
            float angleSign = 1.0f;
            if (Vector3.Dot(Vector3.Cross(toFirstPoint, toSecondPoint), Vector3.forward) < 0.0f) angleSign = -1.0f;
            else angleSign = 1.0f;

            // In order to draw the disc mesh, we will need to keep rotating the first vector towards the second one and
            // for that we need the angle between the 2 vectors. Applying the 'SafeAcos' function to the dot product between
            // the 2 vectors gives us the angle (the 2 vectors were normalized earlier). We then transform the angle in
            // degree units and multiply by the sign value to establish the direction of rotation. After all this is done,
            // we construct a quaternion, 'destinationQuaternion', which can be used to rotate the first vector towards the
            // second one. We will use the 'Slerp' function to rotate the first vector towards the second vector in equal
            // angle increments.
            float angleInDegrees = MathHelper.SafeAcos(Vector3.Dot(toFirstPoint, toSecondPoint)) * Mathf.Rad2Deg * angleSign;
            Quaternion destinationQuaternion = Quaternion.AngleAxis(angleInDegrees, Vector3.forward);

            // Calculate the number of points on the circumference of the disc. We will use a number of 'numberOfPointsFor180Degrees'
            // for an 180 degree disc. The actual number of points for the disc that we want to draw is that value scaled by the
            // ration between the calculated angle and 180.0f.
            const int numberOfPointsFor180Degrees = 180;
            int actualNumberOfPoints = (int)((float)numberOfPointsFor180Degrees * Mathf.Abs(angleInDegrees) / 180.0f);
            if (actualNumberOfPoints < 2) return;

            // Apply the material
            discMaterial.SetColor("_Color", discColor);
            discMaterial.SetPass(0);

            // Prepare to draw in 2D
            GL.PushMatrix();
            GL.LoadOrtho();

            // Begin drawing
            GL.Begin(GL.TRIANGLES);
            GL.Color(discColor);

            // Now we need to draw the disc triangles. We will need togenerate the disc vertices as we go
            // and for that we will use the 'Slerp' function which will rotate from the first disc vector
            // towards the second one. In order to do that, we calculate the increment step for the slerp
            // interpolation factor and that will make sure that we always rotated in equal angle steps.
            float tStep = 1.0f / (actualNumberOfPoints - 1);
            Vector3 previousVertex = discCenter + toFirstPoint * discRadius;
            for (int pointIndex = 0; pointIndex < actualNumberOfPoints; ++pointIndex)
            {
                // Rotate towards the second vector
                Quaternion rotation = Quaternion.Slerp(Quaternion.identity, destinationQuaternion, tStep * (float)pointIndex);
                Vector2 rotatedVector = rotation * toFirstPoint;
                rotatedVector.Normalize();

                // The current triangle is formed by the disc's center point, the vertex stored inside 'vertex' (generated
                // with the help of the rotated vector), and the vertex which comes before it.
                Vector3 vertex = discCenter + rotatedVector * discRadius;

                GL.Vertex(camera.ScreenToViewportPoint(new Vector3(discCenter.x, discCenter.y, 0.0f)));
                GL.Vertex((camera.ScreenToViewportPoint(new Vector3(vertex.x, vertex.y, 0.0f))));
                GL.Vertex((camera.ScreenToViewportPoint(new Vector3(previousVertex.x, previousVertex.y, 0.0f))));

                previousVertex = vertex;
            }

            // End drawing and restore the old modelview matrix
            GL.End();
            GL.PopMatrix();
        }

        public static void DrawWireSelectionBoxes(List<ObjectSelectionBox> selectionBoxes, float boxSizeAdd, Camera camera, Color lineColor, Material boxLineMaterial)
        {
            // Save the current modelview matrix. We will always modify the model view matrix such that it 
            // represents the result of the multiplication between the box's transform matrix and the camera
            // view matrix.
            GL.PushMatrix();

            // Apply the material
            boxLineMaterial.SetColor("_Color", lineColor);
            boxLineMaterial.SetPass(0);

            // Unity uses a left handed coordinate system, but the GL API expects us to work
            // with matrices whose transforms are relative to a right handed coordinate system.
            // So, we will need to multiply our modelview matrix with another matrix which makes
            // sure that the final resulting matrix represents the same transform but in a right
            // handed coordinate system. We do this simply by negating the sign of the Z value
            // inside an identity matrix.
            Matrix4x4 toRightHanded = Matrix4x4.identity;
            toRightHanded[2, 2] *= -1.0f;

            // Store the inverse camera transform for easy access
            Matrix4x4 cameraInverseTransform = camera.transform.worldToLocalMatrix;

            // Loop through each box and render it         
            for (int selectionBoxIndex = 0; selectionBoxIndex < selectionBoxes.Count; ++selectionBoxIndex)
            {
                // Store box for easy access
                ObjectSelectionBox objectSelectionBox = selectionBoxes[selectionBoxIndex];

                // Store the model space box and box transform matrix for easy access
                Box modelSpaceBox = objectSelectionBox.ModelSpaceBox;
                Matrix4x4 boxTransformMatrix = objectSelectionBox.TransformMatrix;
                modelSpaceBox.Size += Vector3.one * boxSizeAdd;

                // Set the corrent modelview matrix. 
                // Note: In order to construct the modelview matrix, we multiply the box world transform
                //       with the camera view matrix (given to us by 'camera.transform.worldToLocalMatrix'),
                //       and then, we perform a final multiplication with the 'toRightHanded' matrix in order
                //       to ensure that the resulting matrix contains a transform which exists inside a 
                //       right handed coordinate system. This is what the GL API expects.
                GL.LoadIdentity();
                GL.MultMatrix(toRightHanded * cameraInverseTransform * boxTransformMatrix);
   
                // Begin drawing for the current box
                GL.Begin(GL.LINES);

                // Set the color used to draw the box lines
                GL.Color(lineColor);

                // Front face
                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Min.y, modelSpaceBox.Min.z);
                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Min.y, modelSpaceBox.Min.z);

                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Min.y, modelSpaceBox.Min.z);
                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Max.y, modelSpaceBox.Min.z);

                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Max.y, modelSpaceBox.Min.z);
                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Max.y, modelSpaceBox.Min.z);

                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Max.y, modelSpaceBox.Min.z);
                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Min.y, modelSpaceBox.Min.z);

                // Back face
                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Min.y, modelSpaceBox.Max.z);
                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Min.y, modelSpaceBox.Max.z);

                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Min.y, modelSpaceBox.Max.z);
                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Max.y, modelSpaceBox.Max.z);

                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Max.y, modelSpaceBox.Max.z);
                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Max.y, modelSpaceBox.Max.z);

                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Max.y, modelSpaceBox.Max.z);
                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Min.y, modelSpaceBox.Max.z);

                // Bottom face (partially drawn when drawing the front and back faces, so we
                // will only draw what's left)
                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Min.y, modelSpaceBox.Min.z);
                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Min.y, modelSpaceBox.Max.z);

                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Min.y, modelSpaceBox.Min.z);
                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Min.y, modelSpaceBox.Max.z);

                // Top face (partially drawn when drawing the front and back faces, so we
                // will only draw what's left)
                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Max.y, modelSpaceBox.Min.z);
                GL.Vertex3(modelSpaceBox.Min.x, modelSpaceBox.Max.y, modelSpaceBox.Max.z);

                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Max.y, modelSpaceBox.Min.z);
                GL.Vertex3(modelSpaceBox.Max.x, modelSpaceBox.Max.y, modelSpaceBox.Max.z);

                // End drawing for this box
                GL.End();
            }

            // Restore the old modelview matrix     
            GL.PopMatrix();
        }

        public static void DrawCornerLinesForSelectionBoxes(List<ObjectSelectionBox> selectionBoxes, float boxSizeAdd, float cornerLinePercentage, Camera camera, Color cornerLineColor, Material boxLineMaterial)
        {
            // Save the current modelview matrix. We will always modify the model view matrix such that it 
            // represents the result of the multiplication between the box's transform matrix and the camera
            // view matrix.
            GL.PushMatrix();

            // Apply the material
            boxLineMaterial.SetColor("_Color", cornerLineColor);
            boxLineMaterial.SetPass(0);

            // Unity uses a left handed coordinate system, but the GL API expects us to work
            // with matrices whose transforms are relative to a right handed coordinate system.
            // So, we will need to multiply our modelview matrix with another matrix which makes
            // sure that the final resulting matrix represents the same transform but in a right
            // handed coordinate system. We do this simply by negating the sign of the Z value
            // inside an identity matrix.
            Matrix4x4 toRightHanded = Matrix4x4.identity;
            toRightHanded[2, 2] *= -1.0f;

            // Store the inverse camera transform for easy access
            Matrix4x4 cameraInverseTransform = camera.transform.worldToLocalMatrix;

            // Loop through each box and render it         
            for (int selectionBoxIndex = 0; selectionBoxIndex < selectionBoxes.Count; ++selectionBoxIndex)
            {
                // Store box for easy access
                ObjectSelectionBox objectSelectionBox = selectionBoxes[selectionBoxIndex];

                // Store the model space box and box transform matrix for easy access
                Box modelSpaceBox = objectSelectionBox.ModelSpaceBox;
                Matrix4x4 boxTransformMatrix = objectSelectionBox.TransformMatrix;
                modelSpaceBox.Size += Vector3.one * boxSizeAdd;

                Vector3 matrixScale = boxTransformMatrix.GetXYZScale();
                modelSpaceBox.Size = Vector3.Scale(modelSpaceBox.Size, matrixScale);
                modelSpaceBox.Center = Vector3.Scale(modelSpaceBox.Center, matrixScale);
                boxTransformMatrix = boxTransformMatrix.SetScaleToOneOnAllAxes();

                // Set the corrent modelview matrix. 
                // Note: In order to construct the modelview matrix, we multiply the box world transform
                //       with the camera view matrix (given to us by 'camera.transform.worldToLocalMatrix'),
                //       and then, we perform a final multiplication with the 'toRightHanded' matrix in order
                //       to ensure that the resulting matrix contains a transform which exists inside a 
                //       right handed coordinate system. This is what the GL API expects.
                GL.LoadIdentity();
                GL.MultMatrix(toRightHanded * cameraInverseTransform * boxTransformMatrix);

                // Begin drawing
                GL.Begin(GL.LINES);

                // Set the color used to draw the box lines
                GL.Color(cornerLineColor);

                // Make sure the corner line length does not exceed the half size of the box on any axis.
                float lineLengthAlongX = cornerLinePercentage * modelSpaceBox.Extents.x;
                float lineLengthAlongY = cornerLinePercentage * modelSpaceBox.Extents.y;
                float lineLengthAlongZ = cornerLinePercentage * modelSpaceBox.Extents.z;

                // Top left corner in front face
                Vector3 startPoint = new Vector3(modelSpaceBox.Min.x, modelSpaceBox.Max.y, modelSpaceBox.Min.z);
                Vector3 endPoint = startPoint + Vector3.right * lineLengthAlongX;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint - Vector3.up * lineLengthAlongY;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint + Vector3.forward * lineLengthAlongZ;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                // Top right corner in front face
                startPoint = new Vector3(modelSpaceBox.Max.x, modelSpaceBox.Max.y, modelSpaceBox.Min.z);
                endPoint = startPoint - Vector3.right * lineLengthAlongX;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint - Vector3.up * lineLengthAlongY;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint + Vector3.forward * lineLengthAlongZ;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                // Bottom right corner in front face
                startPoint = new Vector3(modelSpaceBox.Max.x, modelSpaceBox.Min.y, modelSpaceBox.Min.z);
                endPoint = startPoint - Vector3.right * lineLengthAlongX;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint + Vector3.up * lineLengthAlongY;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint + Vector3.forward * lineLengthAlongZ;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                // Bottom left corner in front face
                startPoint = new Vector3(modelSpaceBox.Min.x, modelSpaceBox.Min.y, modelSpaceBox.Min.z);
                endPoint = startPoint + Vector3.right * lineLengthAlongX;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint + Vector3.up * lineLengthAlongY;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint + Vector3.forward * lineLengthAlongZ;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                // Top left corner in back face
                startPoint = new Vector3(modelSpaceBox.Min.x, modelSpaceBox.Max.y, modelSpaceBox.Max.z);
                endPoint = startPoint + Vector3.right * lineLengthAlongX;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint - Vector3.up * lineLengthAlongY;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint - Vector3.forward * lineLengthAlongZ;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                // Top right corner in back face
                startPoint = new Vector3(modelSpaceBox.Max.x, modelSpaceBox.Max.y, modelSpaceBox.Max.z);
                endPoint = startPoint - Vector3.right * lineLengthAlongX;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint - Vector3.up * lineLengthAlongY;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint - Vector3.forward * lineLengthAlongZ;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                // Bottom right corner in back face
                startPoint = new Vector3(modelSpaceBox.Max.x, modelSpaceBox.Min.y, modelSpaceBox.Max.z);
                endPoint = startPoint - Vector3.right * lineLengthAlongX;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint + Vector3.up * lineLengthAlongY;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint - Vector3.forward * lineLengthAlongZ;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                // Bottom left corner in back face
                startPoint = new Vector3(modelSpaceBox.Min.x, modelSpaceBox.Min.y, modelSpaceBox.Max.z);
                endPoint = startPoint + Vector3.right * lineLengthAlongX;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint + Vector3.up * lineLengthAlongY;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                endPoint = startPoint - Vector3.forward * lineLengthAlongZ;
                GL.Vertex(startPoint);
                GL.Vertex(endPoint);

                // End drawing for the current box
                GL.End();
            }

            // Restore the old modelview matrix
            GL.PopMatrix();
        }

        public static void DrawGridLines(float cellSizeX, float cellSizeZ, Camera camera, Material material, Color color)
        {
            CameraViewVolume viewVolume = camera.GetViewVolume(camera.farClipPlane);
            Bounds volumeAABB = viewVolume.AABB;

            float minX = volumeAABB.min.x;
            float minZ = volumeAABB.min.z;
            float maxX = volumeAABB.max.x;
            float maxZ = volumeAABB.max.z;

            float halfCellSizeX = 0.5f * cellSizeX;
            float halfCellSizeZ = 0.5f * cellSizeZ;
            int minCellX = Mathf.FloorToInt((minX + halfCellSizeX) / cellSizeX) - 1;
            int maxCellX = Mathf.FloorToInt((maxX + halfCellSizeX) / cellSizeX) + 1;
            int minCellZ = Mathf.FloorToInt((minZ + halfCellSizeZ) / cellSizeZ) - 1;
            int maxCellZ = Mathf.FloorToInt((maxZ + halfCellSizeZ) / cellSizeZ) + 1;

            material.SetColor("_Color", color);
            material.SetPass(0);

            int minCellIndex = minCellX < minCellZ ? minCellX : minCellZ;
            int maxCellIndex = maxCellX > maxCellZ ? maxCellX : maxCellZ;

            GL.Begin(GL.LINES);
            float startZ = minCellIndex * cellSizeZ;
            float endZ = (maxCellIndex + 1) * cellSizeZ;
            float startX = minCellIndex * cellSizeX;
            float endX = (maxCellIndex + 1) * cellSizeX;
            for(int cell = minCellIndex; cell <= maxCellIndex; ++cell)
            {
                Vector3 xOffset = cell * Vector3.right * cellSizeX;
                GL.Vertex(xOffset + Vector3.forward * startZ);
                GL.Vertex(xOffset + Vector3.forward * endZ);

                Vector3 zOffset = cell * Vector3.forward * cellSizeZ;
                GL.Vertex(zOffset + Vector3.right * startX);
                GL.Vertex(zOffset + Vector3.right * endX);
            }

            /*
            float startZ = minCellZ * cellSizeZ;
            float endZ = (maxCellZ + 1) * cellSizeZ;
            for (int x = minCellX; x <= maxCellX; ++x)
            {
                Vector3 xOffset = x * Vector3.right * cellSizeX;
                GL.Vertex(xOffset + Vector3.forward * startZ);
                GL.Vertex(xOffset + Vector3.forward * endZ);
            }

            float startX = minCellX * cellSizeX;
            float endX = (maxCellX + 1) * cellSizeX;
            for (int z = minCellZ; z <= maxCellZ; ++z)
            {
                Vector3 zOffset = z * Vector3.forward * cellSizeZ;
                GL.Vertex(zOffset + Vector3.right * startX);
                GL.Vertex(zOffset + Vector3.right * endX);
            }*/
            GL.End();
        }
        #endregion
    }
}
