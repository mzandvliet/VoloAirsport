using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Static class which can be used to create different types of meshes procedurally.
    /// </summary>
    public static class ProceduralMeshGenerator
    {
        #region Public Static Functions
        public static Mesh CreateXZGridLineMesh(float cellSizeX, float cellSizeZ, int cellCountX, int cellCountZ)
        {
            cellSizeX = Mathf.Max(1e-4f, cellSizeX);
            cellSizeZ = Mathf.Max(1e-4f, cellSizeZ);

            cellCountX = Mathf.Max(1, cellCountX);
            cellCountZ = Mathf.Max(1, cellCountZ);

            int lineCountX = (cellCountX + 1);
            int lineCountZ = (cellCountZ + 1);

            int totalNumberOfVerts = lineCountX * lineCountZ * 2;
            Vector3[] vertexPositions = new Vector3[totalNumberOfVerts];
            Vector3[] vertexNormals = new Vector3[totalNumberOfVerts];
            for (int vIndex = 0; vIndex < totalNumberOfVerts; ++vIndex) vertexNormals[vIndex] = Vector3.up;

            int[] indices = new int[totalNumberOfVerts];
            int vertexPtr = 0;

            for(int lineIndex = 0; lineIndex < lineCountX; ++lineIndex)
            {
                Vector3 firstVertex = Vector3.right * cellSizeX * lineIndex;
                Vector3 secondVertex = firstVertex + Vector3.forward * cellSizeZ * (lineCountZ - 1);

                indices[vertexPtr] = vertexPtr;
                vertexPositions[vertexPtr++] = firstVertex;
                indices[vertexPtr] = vertexPtr;
                vertexPositions[vertexPtr++] = secondVertex;
            }

            for (int lineIndex = 0; lineIndex < lineCountZ; ++lineIndex)
            {
                Vector3 firstVertex = Vector3.forward * cellSizeZ * lineIndex;
                Vector3 secondVertex = firstVertex + Vector3.right * cellSizeX * (lineCountX - 1);

                indices[vertexPtr] = vertexPtr;
                vertexPositions[vertexPtr++] = firstVertex;
                indices[vertexPtr] = vertexPtr;
                vertexPositions[vertexPtr++] = secondVertex;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertexPositions;
            mesh.normals = vertexNormals;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);

            return mesh;
        }

        /// <summary>
        /// Creates a right angled triangle mesh. The mesh is created in such a way that the triangle
        /// sits in the XY plane where the 2 adjcent sides extend along the X and Y axes respectively.
        /// The 2 adjacent sides meet at the origin of the coordinate system.
        /// </summary>
        /// <remarks>
        /// The function will construct the normals of the triangle vertices in such a way that they
        /// will point along then negative global Z axis.
        /// </remarks>
        /// <param name="lengthOfXAxisSide">
        /// This is the length of the adjacent side which extends along the X axis.
        /// </param>
        /// <param name="lengthOfYAxisSide">
        /// This is the length of the adjacent side which extends along the Y axis.
        /// </param>
        /// <returns>
        /// The triangle mesh.
        /// </returns>
        public static Mesh CreateRightAngledTriangleMesh(float lengthOfXAxisSide, float lengthOfYAxisSide)
        {
            // Clamp dimensions
            lengthOfYAxisSide = Mathf.Max(lengthOfYAxisSide, 0.0001f);
            lengthOfXAxisSide = Mathf.Max(lengthOfXAxisSide, 0.0001f);

            // Prepare the vertex attribute arrays
            Vector3[] triangleVertexPositions = new Vector3[3];
            Vector3[] triangleVertexNormals = new Vector3[3];

            // Generate the vertex positions
            triangleVertexPositions[0] = Vector3.zero;
            triangleVertexPositions[1] = Vector3.up * lengthOfYAxisSide;
            triangleVertexPositions[2] = Vector3.right * lengthOfXAxisSide;

            // Generate the vertex normals
            triangleVertexNormals[0] = -Vector3.forward;
            triangleVertexNormals[1] = triangleVertexNormals[0];
            triangleVertexNormals[2] = triangleVertexNormals[0];

            // Generate the indices
            int[] vertexIndices = new int[3] { 0, 1, 2 };

            // Create the mesh and return it to the client code
            var triangleMesh = new Mesh();
            triangleMesh.vertices = triangleVertexPositions;
            triangleMesh.normals = triangleVertexNormals;
            triangleMesh.SetIndices(vertexIndices, MeshTopology.Triangles, 0);

            return triangleMesh;
        }

        /// <summary>
        /// Creates a box mesh.
        /// </summary>
        /// <param name="width">
        /// The box width.
        /// </param>
        /// <param name="height">
        /// The box height.
        /// </param>
        /// <param name="depth">
        /// The box depth.
        /// </param>
        /// <returns>
        /// The box mesh.
        /// </returns>
        public static Mesh CreateBoxMesh(float width, float height, float depth)
        {
            // Clamp dimensions
            width = Mathf.Max(width, 0.0001f);
            height = Mathf.Max(height, 0.0001f);
            depth = Mathf.Max(depth, 0.0001f);

            // Store half dimension values for easy access
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            float halfDepth = depth * 0.5f;

            // Prepare the vertex attribute arrays
            Vector3[] boxVertexPositions = new Vector3[24];
            Vector3[] boxVertexNormals = new Vector3[24];

            // Generate the vertices. Start with the front face
            boxVertexPositions[0] = new Vector3(-halfWidth, -halfHeight, -halfDepth);
            boxVertexPositions[1] = new Vector3(-halfWidth, halfHeight, -halfDepth);
            boxVertexPositions[2] = new Vector3(halfWidth, halfHeight, -halfDepth);
            boxVertexPositions[3] = new Vector3(halfWidth, -halfHeight, -halfDepth);

            boxVertexNormals[0] = -Vector3.forward;
            boxVertexNormals[1] = boxVertexNormals[0];
            boxVertexNormals[2] = boxVertexNormals[0];
            boxVertexNormals[3] = boxVertexNormals[0];

            // Back face
            boxVertexPositions[4] = new Vector3(-halfWidth, -halfHeight, halfDepth);
            boxVertexPositions[5] = new Vector3(halfWidth, -halfHeight, halfDepth);
            boxVertexPositions[6] = new Vector3(halfWidth, halfHeight, halfDepth);
            boxVertexPositions[7] = new Vector3(-halfWidth, halfHeight, halfDepth);

            boxVertexNormals[4] = Vector3.forward;
            boxVertexNormals[5] = boxVertexNormals[4];
            boxVertexNormals[6] = boxVertexNormals[4];
            boxVertexNormals[7] = boxVertexNormals[4];

            // Left face
            boxVertexPositions[8] = new Vector3(-halfWidth, -halfHeight, halfDepth);
            boxVertexPositions[9] = new Vector3(-halfWidth, halfHeight, halfDepth);
            boxVertexPositions[10] = new Vector3(-halfWidth, halfHeight, -halfDepth);
            boxVertexPositions[11] = new Vector3(-halfWidth, -halfHeight, -halfDepth);

            boxVertexNormals[8] = -Vector3.right;
            boxVertexNormals[9] = boxVertexNormals[8];
            boxVertexNormals[10] = boxVertexNormals[8];
            boxVertexNormals[11] = boxVertexNormals[8];

            // Right face
            boxVertexPositions[12] = new Vector3(halfWidth, -halfHeight, -halfDepth);
            boxVertexPositions[13] = new Vector3(halfWidth, halfHeight, -halfDepth);
            boxVertexPositions[14] = new Vector3(halfWidth, halfHeight, halfDepth);
            boxVertexPositions[15] = new Vector3(halfWidth, -halfHeight, halfDepth);

            boxVertexNormals[12] = Vector3.right;
            boxVertexNormals[13] = boxVertexNormals[12];
            boxVertexNormals[14] = boxVertexNormals[12];
            boxVertexNormals[15] = boxVertexNormals[12];

            // Top face
            boxVertexPositions[16] = new Vector3(-halfWidth, halfHeight, -halfDepth);
            boxVertexPositions[17] = new Vector3(-halfWidth, halfHeight, halfDepth);
            boxVertexPositions[18] = new Vector3(halfWidth, halfHeight, halfDepth);
            boxVertexPositions[19] = new Vector3(halfWidth, halfHeight, -halfDepth);

            boxVertexNormals[16] = Vector3.up;
            boxVertexNormals[17] = boxVertexNormals[16];
            boxVertexNormals[18] = boxVertexNormals[16];
            boxVertexNormals[19] = boxVertexNormals[16];

            // Bottom face
            boxVertexPositions[20] = new Vector3(-halfWidth, -halfHeight, -halfDepth);
            boxVertexPositions[21] = new Vector3(halfWidth, -halfHeight, -halfDepth);
            boxVertexPositions[22] = new Vector3(halfWidth, -halfHeight, halfDepth);
            boxVertexPositions[23] = new Vector3(-halfWidth, -halfHeight, halfDepth);

            boxVertexNormals[20] = -Vector3.up;
            boxVertexNormals[21] = boxVertexNormals[20];
            boxVertexNormals[22] = boxVertexNormals[20];
            boxVertexNormals[23] = boxVertexNormals[20];

            // Generate the indices
            int[] vertexIndices = new int[]
            {
                // Front face
                0, 1, 2, 0, 2, 3,

                // Back face
                4, 5, 6, 4, 6, 7,
            
                // Left face
                8, 9, 10, 8, 10, 11,

                // Right face
                12, 13, 14, 12, 14, 15,

                // Top face
                16, 17, 18, 16, 18, 19,

                // Bottom face
                20, 21, 22, 20, 22, 23
            };

            // Create the mesh and return it to the client code
            var boxMesh = new Mesh();
            boxMesh.vertices = boxVertexPositions;
            boxMesh.normals = boxVertexNormals;
            boxMesh.SetIndices(vertexIndices, MeshTopology.Triangles, 0);

            return boxMesh;
        }

        /// <summary>
        /// Creates a plane mesh inside the XY plane.
        /// </summary>
        /// <param name="sizeAlongX">
        /// The size of the plane along the X axis.
        /// </param>
        /// <param name="sizeAlongY">
        /// The size of the plane along the Y axis.
        /// </param>
        /// <returns>
        /// The plane mesh.
        /// </returns>
        public static Mesh CreatePlaneMesh(float sizeAlongX, float sizeAlongY)
        {
            // Clamp size values
            sizeAlongX = Mathf.Max(sizeAlongX, 0.0001f);
            sizeAlongY = Mathf.Max(sizeAlongY, 0.0001f);

            // Store half size values for easy access
            float halfSizeX = sizeAlongX * 0.5f;
            float halfSizeY = sizeAlongY * 0.5f;

            // Prepare the vertex attribute arrays
            Vector3[] planeVertexPositions = new Vector3[4];
            Vector3[] planeVertexNormals = new Vector3[4];

            // Establish vertex positions
            planeVertexPositions[0] = new Vector3(-halfSizeX, -halfSizeY, 0.0f);
            planeVertexPositions[1] = new Vector3(-halfSizeX, halfSizeY, 0.0f);
            planeVertexPositions[2] = new Vector3(halfSizeX, halfSizeY, 0.0f);
            planeVertexPositions[3] = new Vector3(halfSizeX, -halfSizeY, 0.0f);

            // Establish vertex normals
            planeVertexNormals[0] = new Vector3(0.0f, 0.0f, -1.0f);
            planeVertexNormals[1] = planeVertexNormals[0];
            planeVertexNormals[2] = planeVertexNormals[0];
            planeVertexNormals[3] = planeVertexNormals[0];

            // Establish the vertex indices
            int[] vertexIndices = new int[] { 0, 1, 2, 2, 3, 0 };

            // Create the mesh and return it to the client code
            var planeMesh = new Mesh();
            planeMesh.vertices = planeVertexPositions;
            planeMesh.normals = planeVertexNormals;
            planeMesh.SetIndices(vertexIndices, MeshTopology.Triangles, 0);

            return planeMesh;
        }

        /// <summary>
        /// Creates a sphere mesh.
        /// </summary>
        /// <param name="sphereRadius">
        /// The sphere radius.
        /// </param>
        /// <param name="numberOfVerticalSlices">
        /// The umber of vertical slices.
        /// </param>
        /// <param name="numberOfStacks">
        /// The number of stacks.
        /// </param>
        /// <returns>
        /// The sphere mesh.
        /// </returns>
        public static Mesh CreateSphereMesh(float sphereRadius, int numberOfVerticalSlices, int numberOfStacks)
        {
            // Clamp values as necessary
            sphereRadius = Mathf.Max(0.01f, sphereRadius);
            numberOfVerticalSlices = Mathf.Max(3, numberOfVerticalSlices);
            numberOfStacks = Mathf.Max(2, numberOfStacks);

            // Calculate the total number of vertices which are needed for the sphere
            int numberOfHorizontalRings = numberOfStacks + 1;
            int numberOfVerticesInOneRing = numberOfVerticalSlices + 1;
            int totalNumberOfVertices = numberOfVerticesInOneRing * numberOfHorizontalRings;

            // Create the vertex positions and normals array
            Vector3[] sphereVertexPositions = new Vector3[totalNumberOfVertices];
            Vector3[] sphereVertexNormals = new Vector3[totalNumberOfVertices];

            // Generate the vertices. We will start from the top of the sphere and move downwards towards the bottom. 
            int currentVertexIndex = 0;
            float sphereDiameter = 2.0f * sphereRadius;
            float yDecrement = sphereDiameter / numberOfStacks;     // Used to establish the Y position for the sphere vertices
            float angleStep = 360.0f / numberOfVerticalSlices;      // Used to rotate the sphere vertices around the Y axis
            for (int horizontalRingIndex = 0; horizontalRingIndex < numberOfHorizontalRings; ++horizontalRingIndex)
            {
                // Establish the Y position of the vertices which reside in the current ring 
                float currentYPosition = sphereRadius - yDecrement * horizontalRingIndex;

                // We will need this variable to calculate the vertex normal. It helps us rotate the vertex normal
                // around the X axis. This has the effect of having the vertices on the top of the sphere pointing
                // straight up along the Y axis and the vertices on the bottom of the sphere straight down on the
                // Y axis. This rotation angle is calculated by multiplying 180 degrees (full rotation) with a ratio
                // which tells us how much we have moved downwards along the Y axis.
                float downstepAngle = ((sphereRadius - currentYPosition) / sphereDiameter) * 180.0f;

                // Geneate the vertices in teh current ring (the ring of verts which reside in the XZ plane)
                for (int vertexIndex = 0; vertexIndex < numberOfVerticesInOneRing; ++vertexIndex)
                {
                    // Store the vertex normal
                    sphereVertexNormals[currentVertexIndex] = Vector3.up;
                    Quaternion normalRotationQuaternion = Quaternion.Euler(-downstepAngle, -angleStep * vertexIndex, 0.0f);
                    sphereVertexNormals[currentVertexIndex] = normalRotationQuaternion * sphereVertexNormals[currentVertexIndex];
                    sphereVertexNormals[currentVertexIndex].Normalize();

                    // Store the vertex position. 
                    // Note: The vertex position is simply the vertex normal scaled by the sphere radius.
                    sphereVertexPositions[currentVertexIndex] = sphereVertexNormals[currentVertexIndex] * sphereRadius;

                    // Next vertex
                    ++currentVertexIndex;
                }
            }

            // Calculate the total number of vertex indices
            int numberOfTriangles = numberOfStacks * numberOfVerticalSlices * 2;
            int indexCount = numberOfTriangles * 3;
            int[] vertexIndices = new int[indexCount];

            // Generate the vertex indices. The next piece of code treats the vertices of the sphere as a flat grid in the
            // YX plane. By rotating the grid vertices around the Y axis and offsetting them accordingly from the center
            // of the grid by the sphere radius, the sphere is obtained.
            int currentIndex = 0;
            for (int horizontalRingIndex = 0; horizontalRingIndex < numberOfHorizontalRings - 1; ++horizontalRingIndex)
            {
                // Loop through all vertices in the current horizontal ring
                for (int vertexIndex = 0; vertexIndex < numberOfVerticesInOneRing - 1; ++vertexIndex)
                {
                    int absoluteVertexIndex = horizontalRingIndex * numberOfVerticesInOneRing + vertexIndex;

                    // First triangle
                    vertexIndices[currentIndex++] = absoluteVertexIndex;                                  // Current vertex
                    vertexIndices[currentIndex++] = absoluteVertexIndex + numberOfVerticesInOneRing + 1;  // Vertex in row above and to the right
                    vertexIndices[currentIndex++] = absoluteVertexIndex + numberOfVerticesInOneRing;      // Vertex in row above

                    // Second triangle
                    vertexIndices[currentIndex++] = absoluteVertexIndex;                                  // Current vertex
                    vertexIndices[currentIndex++] = absoluteVertexIndex + 1;                              // The vertex next to the current one
                    vertexIndices[currentIndex++] = absoluteVertexIndex + numberOfVerticesInOneRing + 1;  // Vertex in row above and to the right
                }
            }

            // Create the mesh and return it to the client code
            var sphereMesh = new Mesh();
            sphereMesh.vertices = sphereVertexPositions;
            sphereMesh.normals = sphereVertexNormals;
            sphereMesh.SetIndices(vertexIndices, MeshTopology.Triangles, 0);
            return sphereMesh;
        }

        /// <summary>
        /// Creates a cone mesh.
        /// </summary>
        /// <param name="bottomRadius">
        /// The radius of the bottom cone cap.
        /// </param>
        /// <param name="height">
        /// The cone height.
        /// </param>
        /// <param name="numberOfVerticalSlices">
        /// The number of vertical slices.
        /// </param>
        /// <param name="numberOfHorizontalSlices">
        /// The number of horizontal slices.
        /// </param>
        /// <param name="numberOfBottomCapRings">
        /// The number of vertex rings which reside inside the bottom cone cap.
        /// </param>
        /// <returns>
        /// The cone mesh.
        /// </returns>
        public static Mesh CreateConeMesh(float bottomRadius, float height, int numberOfVerticalSlices, int numberOfHorizontalSlices, int numberOfBottomCapRings)
        {
            // Clamp values accordingly
            bottomRadius = Mathf.Abs(bottomRadius);
            height = Mathf.Abs(height);
            numberOfVerticalSlices = Mathf.Max(3, numberOfVerticalSlices);
            numberOfHorizontalSlices = Mathf.Max(1, numberOfHorizontalSlices);
            numberOfBottomCapRings = Mathf.Max(numberOfBottomCapRings, 2);

            // Calculate the total number of vertices which are needed to represent the cone given the specified parameters
            int numberOfHorizontalVertexRings = numberOfHorizontalSlices + 1;
            int numberOfVerticesInOneRing = numberOfVerticalSlices + 1;
            int numberOfVerticesInBottomCap = (numberOfVerticalSlices + 1) * numberOfBottomCapRings;
            int totalNumberOfVertices = numberOfHorizontalVertexRings * numberOfVerticesInOneRing + numberOfVerticesInBottomCap;

            // Create the vertex position and normal arrays
            Vector3[] coneVertexPositions = new Vector3[totalNumberOfVertices];
            Vector3[] coneVertexNormals = new Vector3[totalNumberOfVertices];

            // Generate the vertices (exlcuding bottom cap). We do this by looping through each horizontal ring
            // and generating its vertices.
            float radiusAdjustment = -bottomRadius / numberOfHorizontalSlices;
            float angleStep = 360.0f / numberOfVerticalSlices;
            int currentVertexIndex = 0;
            for (int horizontalRingIndex = 0; horizontalRingIndex < numberOfHorizontalVertexRings; ++horizontalRingIndex)
            {
                // Calculate the radius of the vertex ring. The radius decreases as we move upwards towards the top of the cone.
                float currentRadius = bottomRadius + radiusAdjustment * horizontalRingIndex;

                // Calculate the Y position of the vertices in the current ring. This is calculated by mulitplying the cone height
                // with the ratio between the current ring index and the index of the last vertex ring. This ratio will yield a
                // percentage value which tells us how far up we have moved.
                float currentHeight = height * horizontalRingIndex / (numberOfHorizontalVertexRings - 1);

                // Generate the ring vertices
                for (int vertexIndex = 0; vertexIndex < numberOfVerticesInOneRing; ++vertexIndex)
                {
                    // Generate position and normal
                    float x = Mathf.Cos(Mathf.Deg2Rad * angleStep * vertexIndex);
                    float z = Mathf.Sin(Mathf.Deg2Rad * angleStep * vertexIndex);
                    coneVertexPositions[currentVertexIndex] = new Vector3(x * currentRadius, currentHeight, z * currentRadius);
                    coneVertexNormals[currentVertexIndex] = new Vector3(x, 0.0f, z);
                    coneVertexNormals[currentVertexIndex].Normalize();

                    // Next vertex
                    ++currentVertexIndex;
                }
            }

            // Generate the bottom cap vertices
            radiusAdjustment = bottomRadius / (numberOfBottomCapRings - 1);
            for (int bottomCapRingIndex = 0; bottomCapRingIndex < numberOfBottomCapRings; ++bottomCapRingIndex)
            {
                // We need to calculate the radius of the current ring. This radius will increase the further
                // we move from the cap center.
                float currentRadius = bottomCapRingIndex * radiusAdjustment;

                // Loop through each vertex in the current bottom cap ring
                for (int vertexIndex = 0; vertexIndex < numberOfVerticesInOneRing; ++vertexIndex)
                {
                    // Calculate the vertex position and normal.
                    // Note: The normal will be set to point downwards for the bottom cap.
                    coneVertexPositions[currentVertexIndex] = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleStep * vertexIndex) * currentRadius, 0.0f,
                                                                          Mathf.Sin(Mathf.Deg2Rad * angleStep * vertexIndex) * currentRadius);
                    coneVertexNormals[currentVertexIndex] = -Vector3.up;

                    // Next vertex
                    ++currentVertexIndex;
                }
            }

            // Establish vertex index information
            int numberOfTrianglesInBottomCap = numberOfVerticalSlices * (numberOfBottomCapRings - 1) * 2;
            int totalNumberOfTriangles = (numberOfVerticalSlices * 2) * numberOfHorizontalSlices + numberOfTrianglesInBottomCap;
            int indexCount = totalNumberOfTriangles * 3;
            int[] vertexIndices = new int[indexCount];

            // Generate the indices (excluding bottom cap). We will use the same strategy as when generating indices
            // for a sphere mesh. We visualize the cone vertices as belonging to a grid.
            int currentIndex = 0;
            for (int horizontalRingIndex = 0; horizontalRingIndex < numberOfHorizontalVertexRings - 1; ++horizontalRingIndex)
            {
                // Loop through eacg vertex in the ring
                for (int vertexIndex = 0; vertexIndex < numberOfVerticesInOneRing - 1; ++vertexIndex)
                {
                    // Calculate the index of the vertex in the current ring
                    int absoluteVertexIndex = horizontalRingIndex * numberOfVerticesInOneRing + vertexIndex;

                    vertexIndices[currentIndex++] = absoluteVertexIndex;                                    // Current vertex
                    vertexIndices[currentIndex++] = absoluteVertexIndex + numberOfVerticesInOneRing;        // Vertex above
                    vertexIndices[currentIndex++] = absoluteVertexIndex + 1;                                // Next vertex

                    vertexIndices[currentIndex++] = absoluteVertexIndex + numberOfVerticesInOneRing;        // Vertex above
                    vertexIndices[currentIndex++] = absoluteVertexIndex + numberOfVerticesInOneRing + 1;    // Vertex next to the one above
                    vertexIndices[currentIndex++] = absoluteVertexIndex + 1;                                // Next vertex
                }
            }

            // Generate the bottom cap indices
            int firstVertexInCap = totalNumberOfVertices - numberOfVerticesInBottomCap;
            for (int bottomCapRingIndex = 0; bottomCapRingIndex < numberOfBottomCapRings - 1; ++bottomCapRingIndex)
            {
                // Loop through eacg vertex in the ring
                for (int vertexIndex = 0; vertexIndex < numberOfVerticesInOneRing - 1; ++vertexIndex)
                {
                    // Calculate the index of the vertex in the current ring
                    int absoluteVertexIndex = firstVertexInCap + bottomCapRingIndex * numberOfVerticesInOneRing + vertexIndex;

                    vertexIndices[currentIndex++] = absoluteVertexIndex;                                    // Current vertex
                    vertexIndices[currentIndex++] = absoluteVertexIndex + numberOfVerticesInOneRing;        // Vertex above
                    vertexIndices[currentIndex++] = absoluteVertexIndex + 1;                                // Next vertex

                    vertexIndices[currentIndex++] = absoluteVertexIndex + numberOfVerticesInOneRing;        // Vertex above
                    vertexIndices[currentIndex++] = absoluteVertexIndex + numberOfVerticesInOneRing + 1;    // Vertex next to the one above
                    vertexIndices[currentIndex++] = absoluteVertexIndex + 1;                                // Next vertex
                }
            }

            // Create the mesh and return it to the client code
            var coneMesh = new Mesh();
            coneMesh.vertices = coneVertexPositions;
            coneMesh.normals = coneVertexNormals;
            coneMesh.SetIndices(vertexIndices, MeshTopology.Triangles, 0);

            return coneMesh;
        }
        #endregion
    }
}

