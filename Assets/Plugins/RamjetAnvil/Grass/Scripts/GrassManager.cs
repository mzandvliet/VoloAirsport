using System;
using System.Collections.Generic;
using System.IO;
using Priority_Queue;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Threading;
using RamjetAnvil.Unity.Landmass;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Unity.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

/* Todo:
 * - Optimize
 * - Implement a player-facing performance warning when experiencing streamer underruns
 * - A middleman coordinating load throughput, caching, etc. Regulating throughput by blocking on IO thread is a stupid idea.
    QT traverse should yield set of ideally loaded data. Middleman actually tries to achieve this optimally, given finite streaming throughput.

 * - Stack-based traversal for better perf, profiling and debugging
 * - Teleporting game logic whould coordinate with streamer, requesting the teleport and waiting until streamer reports OK before continuing
 * - Improve unity-side performance by optimizing tree traversal and mesh completion
 * - Geometry shaders
 * - Instancing (Unity 5.4)
 * - Since wind is so expensive, apply LOD. Could base amount of octaves samples and spatial sampling res based on viewer distance. Could sample on GPU.
 * - Could use cheap gradient noise implementation (e.g. non continuous derivative) because it's only used to animate stuff
 * - Using time to offset point used for noise lookup is cool, but make sure to wrap time val at noise func period to avoid floating point issues.
 * - Runtime changing of streaming radius param (this is really hard, as it requires waiting on asynch processes to complete. Same problem for delaying a respawn until streaming has completed)
 */


namespace RamjetAnvil.Landmass {
    [System.Serializable]
    public struct GrassManagerConfig {
        [SerializeField] public bool Enabled;
        [SerializeField] public int StreamRadius;
        [SerializeField] public int Density; // per square meter

        public GrassManagerConfig(bool enabled, int streamRadius, int density) {
            Enabled = enabled;
            StreamRadius = streamRadius;
            Density = density;
        }

        public int PatchesInSquare() {
            return (StreamRadius * 2) * (StreamRadius * 2);
        }

        public override string ToString() {
            return "Enabled: " + Enabled + ", StreamRadius: " + StreamRadius + ", Density: " + Density;
        }
    }

    /// <summary>
    /// Notes: In case of underrun, patches will mostly reside in loading request queue and be harmlessly skipped
    /// </summary>
    public class GrassManager : MonoBehaviour {
        [SerializeField, Dependency("rigTransform")]
        private Transform _subject;
        [SerializeField, Dependency]
        private StaticTiledTerrain _terrain;
        [SerializeField, Dependency]
        private WindManager _wind;
        [SerializeField] private Material _material;

        [SerializeField] private GrassManagerConfig _config = new GrassManagerConfig() {
            Density = 4,
            Enabled = true,
            StreamRadius = 4
        };

        [SerializeField] private float _leadTime = 1f;

        [SerializeField] private bool _initializeSelf;

        private JobManager _jobManager;
        private BinaryReader _terrainReader;

        private QuadTree<GrassPatch> _tree;
        private QtNodeVisitor<GrassPatch, TraversalData> _nodeStreamingVisitor;
//        private QtNodeVisitor<GrassPatch, TraversalData> _nodeGizmoVisitor;

        private Queue<JobData> _jobDataPool;
        private Queue<GrassMesh> _meshPool;
        private int _jobDataPoolCapacity;
        private int _patchPoolCapacity;
        private IPriorityQueue<GrassPatch> _loadRequests;

        private Vector3 _lastSubjectPosition;
        private Vector3 _lastSubjectVelocity;
        private Vector3 _predictedSubjectPosition;
        private IntVector3 _lastPatch;

        private Func<object, ICancelToken, object> _loadTerrainFunc;
        private Func<object, ICancelToken, object> _generateMeshFunc;
        private Func<object, ICancelToken, object> _completeMeshFunc;
        private Action<Job> _onJobCanceled;

        private TraversalData _traversalData;

        private bool _initialized;

        public Transform Subject {
            get { return _subject; }
            set { _subject = value; }
        }

        public WindManager Wind {
            get { return _wind; }
            set { _wind = value; }
        }

        public GrassManagerConfig Config {
            get { return _config; }
            set { _config = value; }
        }

        public void Initialize() {
            if (!_initialized) {
                Debug.Log("GrassManager: Initializing with config: " + _config);
                if (_config.Enabled) {
                    OpenTerrainDataFileStream();
                    SetupQuadTree();
                    Allocate();
                    _initialized = true;
                }
            }
        }

        public void OnSubjectTeleported() {
            _lastSubjectPosition = _subject.position;
            _predictedSubjectPosition = _subject.position;
            _lastSubjectVelocity = Vector3.zero;
        }

        private void Awake() {
            _loadTerrainFunc = LoadTerrainData;
            _generateMeshFunc = GenerateMesh;
            _completeMeshFunc = CompleteLoad;
            _onJobCanceled = OnJobCanceled;

            _jobManager = gameObject.AddComponent<JobManager>();
        }

        private void Start() {
            _nodeStreamingVisitor = TraverseAndStream;
//            _nodeGizmoVisitor = TraverseAndDrawGizmos;

            if (_initializeSelf) {
                Initialize();
            }
        }

        private void OnDestroy() {
            if (_terrainReader != null) {
                _terrainReader.Close();
            }
        }

        private void SetupQuadTree() {
            Vector2 bottomLeft = Vector2.one * (_terrain.Config.TotalTerrainSize * -0.5f);

            _tree = QuadTreeUtils.Create<GrassPatch>(
                _terrain.Config.TotalTerrainSize,
                _terrain.Config.PatchSize,
                bottomLeft,
                (node) => {
                    // Construct a node, lookup and cache its center terrain height
                    Vector2 center = node.GetCenter();
                    Vector3 position = To3D(center);
                    TerrainTile tile = _terrain.GetTile(position);
                    Vector2 terrainCoord = new Vector2(
                        (center.x - tile.transform.position.x) / _terrain.Config.TileSize,
                        (center.y - tile.transform.position.z) / _terrain.Config.TileSize);
                    float height = tile.Terrain.terrainData.GetInterpolatedHeight(terrainCoord.x, terrainCoord.y);
                    var patch = new GrassPatch(node.Coord, height);
                    return patch;
                });

            _traversalData = new TraversalData(_tree.MaxDepth, _terrain.Config.PatchSize, _config.StreamRadius);
        }

        private void OpenTerrainDataFileStream() {
            string terrainDataPath = "";
#if UNITY_EDITOR
            terrainDataPath = UPath.GetAbsolutePath("Assets/Terrains/SwissAlps/swissalps.land");
#else
            terrainDataPath = Application.dataPath + "/swissalps.land";
#endif
            Debug.Log("Loading terrain file: " + terrainDataPath);
            _terrainReader = new BinaryReader(File.Open(terrainDataPath, FileMode.Open, FileAccess.Read));
        }

        private void Allocate() {
            AllocateJobData();
            AllocatePatches();

            _loadRequests = new HeapPriorityQueue<GrassPatch>(_config.PatchesInSquare());

            Shader.SetGlobalFloat("_GrassDrawRange", _config.StreamRadius * _terrain.Config.PatchSize);
        }

        // Todo: separate pooling of terraindata and meshdata for increased throughput?
        private void AllocateJobData() {
            _jobDataPoolCapacity = Mathf.Max(4, Mathf.FloorToInt(_config.PatchesInSquare() * 0.2f));
            _jobDataPool = new Queue<JobData>(_jobDataPoolCapacity);

            const int maxVerts = 65534;
            const int maxInst = maxVerts / 4;

            var surface = _terrain.Config.PatchSize * _terrain.Config.PatchSize;
            int maxInstances = Mathf.Min(_config.Density * surface, maxInst);

            for (int i = 0; i < _jobDataPoolCapacity; i++) {
                    var terrainData = new TerrainData(_terrain.Config, _terrainReader);
                    var meshData = new MeshData(maxInstances, _terrain.Config.PatchSize);
                    var jobData = new JobData(terrainData, meshData); 
                    _jobDataPool.Enqueue(jobData);
                }
        }

        private void AllocatePatches() {
            _patchPoolCapacity = Mathf.Max(4, Mathf.FloorToInt(_config.PatchesInSquare()));
            _meshPool = new Queue<GrassMesh>(_patchPoolCapacity);

            for (int i = 0; i < _patchPoolCapacity; i++) {
                var patch = AllocatePatch("GrassPatch_" + i);
                _meshPool.Enqueue(patch);
            }
        }

        private GrassMesh AllocatePatch(string objectName) {
            var patch = new GameObject(objectName).AddComponent<GrassMesh>();
            patch.transform.parent = transform;
            patch.gameObject.SetActive(false);
            patch.Allocate(_material, _terrainReader);

            return patch;
        }

        private void Update() {

            UnityEngine.Profiling.Profiler.BeginSample("Update");

            if (!_initialized || _subject == null) {
                return;
            }

            if (Time.deltaTime > 0f) {
                Vector3 velocity = (_subject.position - _lastSubjectPosition) / Time.deltaTime;
                velocity = Vector3.ClampMagnitude(velocity, 75f);
                _lastSubjectPosition = _subject.position;
                velocity = Vector3.Lerp(_lastSubjectVelocity, velocity, Time.deltaTime);
                _lastSubjectVelocity = velocity;
                _predictedSubjectPosition = _subject.position + velocity * _leadTime; // Stream ahead of camera
                // Todo: Use _predictedSubjectPosition to clip pixels in shader, not camera position
            }

            IntVector3 currentPatch = WorldPointToCoords3D(_terrain.Config.PatchSize, _predictedSubjectPosition);
            bool patchChanged = currentPatch != _lastPatch;
            _lastPatch = currentPatch;

            if (patchChanged) {
                Stream();
            }

            ProcessLoadRequests();

            UnityEngine.Profiling.Profiler.EndSample();

        }

        private void Stream() {
            _traversalData.Clear();
            _traversalData.SubjectPosition = _predictedSubjectPosition;


            UnityEngine.Profiling.Profiler.BeginSample("TraverseWithStack");
            _tree.TraverseWithStack(_nodeStreamingVisitor, _traversalData);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("RequestUnload");

            for (int i = 0; i < _traversalData.UnloadRequests.Count; i++) {
                RequestUnload(_traversalData.UnloadRequests[i].Value);
            }

            UnityEngine.Profiling.Profiler.EndSample();


            UnityEngine.Profiling.Profiler.BeginSample("RequestLoad");

            for (int i = 0; i < _traversalData.LoadRequests.Count; i++) {
                RequestLoad(_traversalData.LoadRequests[i].Value);
            }

            UnityEngine.Profiling.Profiler.EndSample();

        }

        private static bool TraverseAndStream(QtNode<GrassPatch> node, TraversalData data) {
            Vector3 position = To3D(node.GetCenter());
            position.y = node.Value.Height;

            bool isInRange = Vector3.SqrMagnitude(data.SubjectPosition - position) <= data.LodRanges[node.Depth];

            // Todo: save memory by only assigning values to nodes that will actually carry a payload; the leaves

            if (!node.HasChildren) {
                if (isInRange) {
                    data.LoadRequests.Add(node);
                } else {
                    data.UnloadRequests.Add(node);
                }
            }

            bool stateChange = node.Value.WasInRange != isInRange;
            node.Value.WasInRange = isInRange;

            return isInRange || stateChange;
        }

        private void RequestLoad(GrassPatch node) {
            if (node.State != NodeState.Unloaded) {
                return;
            }

            if (_loadRequests.Count == _loadRequests.MaxSize) {
                Debug.LogError("Reached max load requests, skipping " + node.Coord);
                return;
            }

            _loadRequests.Enqueue(node, 1);
            node.State = NodeState.LoadingQueued;
        }

        private void RequestUnload(GrassPatch node) {
            switch (node.State) {
                case NodeState.LoadingQueued:
                    _loadRequests.Remove(node);
                    node.State = NodeState.Unloaded;
                    break;
                case NodeState.Loading:
                    node.RunningJob.Dispose();
                    node.State = NodeState.Unloading;
                    break;
                case NodeState.Loaded:
                    Unload(node);
                    break;
            }
        }

        private void ProcessLoadRequests() {

            UnityEngine.Profiling.Profiler.BeginSample("ProcessLoadRequests");

            // We only start load jobs this frame when we have jobdata available
            int numRequests = Mathf.Min(_loadRequests.Count, _jobDataPool.Count);

            for (int i = 0; i < numRequests; i++) {
                var node = _loadRequests.Dequeue();
                Load(node);
            }

            UnityEngine.Profiling.Profiler.EndSample();

        }

        private void Load(GrassPatch node) {

            UnityEngine.Profiling.Profiler.BeginSample("Load");

            if (_jobDataPool.Count == 0) {
                Debug.LogError("JobData pool is empty (shouldn't happen)");
                return;
            }

            JobData jobData = _jobDataPool.Dequeue();
            node.JobData = jobData;

            node.SnowAltitude = _terrain.TerrainConfiguration.SnowAltitude;

            var job = _jobManager.CreateJob(node)
                .AddTask(_loadTerrainFunc, JobTaskType.AsyncIo)
                .AddTask(_generateMeshFunc, JobTaskType.Async)
                .AddTask(_completeMeshFunc, JobTaskType.UnityThread)
                .OnCancel(_onJobCanceled);

            var disposeJob = _jobManager.StartJob(job);
            node.RunningJob = disposeJob;

            node.State = NodeState.Loading;

            UnityEngine.Profiling.Profiler.EndSample();
        }

        private static object LoadTerrainData(object input, ICancelToken token) {
            GrassPatch node = (GrassPatch)input;
            if (node == null) {
                throw new ArgumentException("GrassNode is null");
            }

            var terrainData = node.JobData.TerrainData;
            var reader = terrainData.Reader;

            if (reader == null) {
                throw new ArgumentException("cannot be null", "reader");
            }

            if (terrainData == null) {
                throw new ArgumentException("cannot be null", "terrainData");
            }

            var cfg = terrainData.Config;

            // Offset coords by half world size, since serialized state is indexed from 0 up, not from -worldRadius
            int totalPatches = cfg.PatchesPerTile * cfg.NumTiles;
//            coords += new IntVector2(totalPatches / 2, totalPatches / 2);


            long linearPatchIndex = node.Coord.X * totalPatches + node.Coord.Y;
            long startPos = linearPatchIndex * (
                cfg.PatchHeightRes * cfg.PatchHeightRes * 2 +
                cfg.PatchHeightRes * cfg.PatchHeightRes * 3 +
                cfg.PatchSplatRes * cfg.PatchSplatRes * 2);

            reader.BaseStream.Seek(startPos, SeekOrigin.Begin);

            

            // Read heights
            for (int x = 0; x < terrainData.Config.PatchHeightRes; x++) {
                for (int z = 0; z < terrainData.Config.PatchHeightRes; z++) {
                    // Todo: Understand why I have to do swizzled x/z reads here
                    terrainData.Heights[z, x] = reader.ReadUInt16() / 65000f * cfg.TerrainHeight;
//                    terrainData.Heights[z, x] = node.Height;
                }
            }

            // Read normals
            for (int x = 0; x < terrainData.Config.PatchHeightRes; x++) {
                for (int z = 0; z < terrainData.Config.PatchHeightRes; z++) {
                    Vector3 n = Vector3.zero;
                    n.x = (reader.ReadByte() / 250f) * 2.0f - 1.0f;
                    n.y = (reader.ReadByte() / 250f) * 2.0f - 1.0f;
                    n.z = (reader.ReadByte() / 250f) * 2.0f - 1.0f;
                    terrainData.Normals[x, z] = n;
//                    terrainData.Normals[x, z] = Vector3.up;
                }
            }

            // Read splats
            for (int x = 0; x < terrainData.Config.PatchSplatRes; x++) {
                for (int z = 0; z < terrainData.Config.PatchSplatRes; z++) {
                    terrainData.Splats[x, z] = new Vector2(
                        Mathf.Pow(reader.ReadByte() / 250f, 0.66f),
                        reader.ReadByte() / 250f);
//                    terrainData.Splats[x, z] = new Vector2(1f, 0f);
                }
            }

            return node;
        }
        private static object GenerateMesh(object input, ICancelToken token) {
            GrassPatch node = (GrassPatch)input;
            if (node == null) {
                throw new ArgumentException("GenerateMesh Failed: Node is null");
            }

            TerrainData terrainData = node.JobData.TerrainData;
            MeshData mesh = node.JobData.MeshData;

            // Todo: Initialize both of these with average patch height or something
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            float dimensions = Mathf.Sqrt(mesh.MaxInstances);
            float dimInv = 1f / dimensions;
            int dimensionsInt = (int)dimensions;
            int i = 0;
            for (int x = 0; x < dimensionsInt && !token.IsCanceled; x++) {
                for (int z = 0; z < dimensionsInt && !token.IsCanceled; z++) {
                    // Todo: move magic numbers to a config struct, expose in inspector

                    Vector2 localIndex = new Vector2(x * dimInv, z * dimInv);
                    localIndex.x += mesh.Random.NextSingle() * dimInv;
                    localIndex.y += mesh.Random.NextSingle() * dimInv;
                    Vector3 localPos = new Vector3(localIndex.x * mesh.Size, 0f, localIndex.y * mesh.Size);

                    float height = terrainData.SampleInterpolatedHeight(localIndex);
                    float snowHighFalloff = Mathf.InverseLerp(node.SnowAltitude + 300f, node.SnowAltitude + 200f, height);
                    float snowLowFalloff = Mathf.InverseLerp(node.SnowAltitude - 50f, node.SnowAltitude - 250f, height);

                    Vector2 splat = terrainData.SampleInterpolatedSplat(localIndex);
                    // Massage splat values to make them more suitable for rendering grass sprites
                    splat.x = Mathf.Pow(splat.x, 0.5f);
                    splat.x = Mathf.Max(0f, splat.x - splat.y * (1f - snowLowFalloff) * 2f);
                    splat.x *= snowHighFalloff;
                    
                    if (splat.x > 0.63f + mesh.Random.NextSingle() * 0.37f) {
                        localPos.y = height;

                        minHeight = Mathf.Min(minHeight, height);
                        maxHeight = Mathf.Max(maxHeight, height);

                        Quaternion rot = Quaternion.Euler(-60f + mesh.Random.NextSingle() * 120f, mesh.Random.NextSingle() * 180f, 0f);
                        Vector3 normal = terrainData.SampleInterpolatedNormal(localIndex);
                        Vector2 scale = new Vector2(0.5f + mesh.Random.NextSingle() * 0.5f, 0.5f + mesh.Random.NextSingle() * 0.5f) * Mathf.Min(1f, splat.x * 3f);

                        CreateQuad(mesh, i++, localPos, rot, normal, scale);
                    }
                }
            }

            // If a patch doesn't feature any grass we need to still create valid bounds
            if (minHeight == float.MaxValue) {
                minHeight = 0f;
            }
            if (maxHeight == float.MinValue) {
                maxHeight = 1f;
            }

            // Zero out unused verts
            for (; i < mesh.MaxInstances ; i++) {
                int vertIndex = i * MeshData.VertsPerQuad;
                mesh.Vertices[vertIndex + 0] = Vector3.zero;
                mesh.Vertices[vertIndex + 1] = Vector3.zero;
                mesh.Vertices[vertIndex + 2] = Vector3.zero;
                mesh.Vertices[vertIndex + 3] = Vector3.zero;
            }

            mesh.Bounds = new Bounds(
                new Vector3(mesh.Size * 0.5f, Mathf.Lerp(minHeight, maxHeight, 0.5f), mesh.Size * 0.5f),
                new Vector3(mesh.Size, maxHeight - minHeight, mesh.Size));

            return node;
        }

        private static void CreateQuad(MeshData data, int quadIndex, Vector3 pos, Quaternion rot, Vector3 normal, Vector2 scale) {
            Vector3 right = rot * Vector3.right * 0.5f * scale.x;
            Vector3 up = rot * normal * scale.y;

            int vertIndex = quadIndex * MeshData.VertsPerQuad;
            int triIndex = quadIndex * MeshData.TrisPerQuad;

            data.Vertices[vertIndex + 0] = pos - right;
            data.Vertices[vertIndex + 1] = pos + right;
            data.Vertices[vertIndex + 2] = pos - right + up;
            data.Vertices[vertIndex + 3] = pos + right + up;

            data.Uvs[vertIndex + 0] = new Vector2(0, 0);
            data.Uvs[vertIndex + 1] = new Vector2(1, 0);
            data.Uvs[vertIndex + 2] = new Vector2(0, 1);
            data.Uvs[vertIndex + 3] = new Vector2(1, 1);

            data.Normals[vertIndex + 0] = normal;
            data.Normals[vertIndex + 1] = normal;
            data.Normals[vertIndex + 2] = normal;
            data.Normals[vertIndex + 3] = normal;

            data.Triangles[triIndex + 0] = vertIndex + 0;
            data.Triangles[triIndex + 1] = vertIndex + 1;
            data.Triangles[triIndex + 2] = vertIndex + 2;
            data.Triangles[triIndex + 3] = vertIndex + 2;
            data.Triangles[triIndex + 4] = vertIndex + 1;
            data.Triangles[triIndex + 5] = vertIndex + 3;
        }

        private object CompleteLoad(object input, ICancelToken token) {

            UnityEngine.Profiling.Profiler.BeginSample("CompleteLoad");

            GrassPatch node = (GrassPatch)input;

            if (node == null) {
                Debug.LogError("CompleteMesh: Cannot complete, node is null");
                return null;
            }

            if (_meshPool.Count == 0) {
                Debug.LogError("CompleteMesh: PatchPool is empty. This shouldn't happen!");
                // Cleanup
                RecycleJobData(node);
                node.State = NodeState.Unloaded;
                return node;
            }

            var patch = _meshPool.Dequeue();
            node.Mesh = patch;

            MeshData meshData = node.JobData.MeshData;
            node.Mesh.Mesh.vertices = meshData.Vertices;
            patch.Mesh.triangles = meshData.Triangles;
            patch.Mesh.normals = meshData.Normals;
            patch.Mesh.uv = meshData.Uvs;
            patch.Mesh.bounds = meshData.Bounds;
            patch.Mesh.UploadMeshData(false);

            // Offset coords because world center is in middle of quadtree extents
            // Todo: The quadtree could help us do pos conversion
            var coord = node.Coord;
            int totalPatches = _terrain.Config.PatchesPerTile * _terrain.Config.NumTiles;
            coord -= new IntVector2(totalPatches / 2, totalPatches / 2);
            patch.Transform.position = CoordsToWorldPoint2D(_terrain.Config.PatchSize, coord);

            patch.gameObject.SetActive(true);

            RecycleJobData(node);
            node.State = NodeState.Loaded;

            UnityEngine.Profiling.Profiler.EndSample();


            return node;
        }

        private void OnJobCanceled(Job job) {

            UnityEngine.Profiling.Profiler.BeginSample("OnJobCanceled");

            GrassPatch node = (GrassPatch)job.InitialInput;
            if (node == null) {
                throw new ArgumentException("OnJobCanceled Failed: Node is null");
            }

            Unload(node);

            UnityEngine.Profiling.Profiler.EndSample();

        }

        private void Unload(GrassPatch node) {

            UnityEngine.Profiling.Profiler.BeginSample("Unload");

            RecycleJobData(node);
            RecycleMesh(node);
            node.State = NodeState.Unloaded;

            UnityEngine.Profiling.Profiler.EndSample();

        }

        private void RecycleMesh(GrassPatch node) {
            if (node.Mesh != null) {
                node.Mesh.gameObject.SetActive(false);
                _meshPool.Enqueue(node.Mesh);
                node.Mesh = null;
            }
        }

        private void RecycleJobData(GrassPatch node) {
            node.RunningJob = null;

            if (node.JobData != null) {
                _jobDataPool.Enqueue(node.JobData);
                node.JobData = null;
            }
        }

        public static Vector3 To3D(Vector2 point) {
            return new Vector3(point.x, 0f, point.y);
        }
        
        public static void To3D(ref Vector3 point) {
            point.z = point.y;
            point.y = 0f;
        }

        public static void To2D(ref Vector3 point) {
            point.y = point.z;
            point.z = 0f;
        }
        public static Vector3 To2D(Vector3 point) {
            return new Vector2(point.x, point.z);
        }

        public static IntVector3 WorldPointToCoords3D(float patchSize, Vector3 worldPosition) {
            return new IntVector3(
                Mathf.FloorToInt(worldPosition.x / patchSize),
                Mathf.FloorToInt(worldPosition.y / patchSize),
                Mathf.FloorToInt(worldPosition.z / patchSize));
        }

        public static Vector3 CoordsToWorldPoint3D(float patchSize, IntVector3 patchCoords) {
            return new Vector3(patchCoords.X * patchSize, patchCoords.Y * patchSize, patchCoords.Z * patchSize);
        }

        public static IntVector2 WorldPointToCoords2D(float patchSize, Vector3 worldPosition) {
            return new IntVector2(
                Mathf.FloorToInt(worldPosition.x / patchSize),
                Mathf.FloorToInt(worldPosition.z / patchSize));
        }

        public static Vector3 CoordsToWorldPoint2D(float patchSize, IntVector2 patchCoords) {
            return new Vector3(patchCoords.X * patchSize, 0f, patchCoords.Y * patchSize);
        }

//#if UNITY_EDITOR
//        private void OnGUI() {
//            if (!_initialized || !_config.Enabled) {
//                return;
//            }
//
//            GUILayout.BeginVertical(GUI.skin.box);
//            GUILayout.Label(
//                "LoadRequests: "     + _loadRequests.Count  + "/" + _loadRequests.MaxSize +
//                ", JobDataPool: "    + _jobDataPool.Count   + "/" + _jobDataPoolCapacity +
//                ", PatchPool: "      + _meshPool.Count     + "/" + _patchPoolCapacity
//            );
//            GUILayout.EndVertical();
//        }
//
//        private void OnDrawGizmos() {
//            if (_initialized && Application.isPlaying) {
//                _traversalData.Clear();
//                _tree.TraverseWithStack(_nodeGizmoVisitor, _traversalData);
//            }
//        }
//#endif

        private static readonly Color[] NodeStateColors = {
            Color.black, // Unloaded
            Color.white, // LoadingQueued
            Color.blue, // Loading
            Color.cyan, // Loaded
            Color.red // Unloading
        };

        private static bool TraverseAndDrawGizmos(QtNode<GrassPatch> node, TraversalData data) {
            if (node.Value.State != NodeState.Unloaded) {
                Vector3 position = To3D(node.GetCenter());
                position.y = node.Value.Height;
                Gizmos.color = NodeStateColors[(int)node.Value.State];
                Gizmos.DrawWireCube(position, new Vector3(node.Size, 1f, node.Size));
            }
            return node.Value.WasInRange;
        }
    }

    public class TraversalData {
        public readonly float[] LodRanges;
        public Vector3 SubjectPosition;
        public readonly List<QtNode<GrassPatch>> LoadRequests;
        public readonly List<QtNode<GrassPatch>> UnloadRequests;

        public TraversalData(int maxDepth, int patchSize, int streamRadius) {
            LodRanges = new float[maxDepth+1];

            for (int i = 0; i <= maxDepth; i++) {
                float range = patchSize * streamRadius * Mathf.Pow(2f, maxDepth - i);
                range *= range;
                LodRanges[i] = range;
            }

            LoadRequests = new List<QtNode<GrassPatch>>(256);
            UnloadRequests = new List<QtNode<GrassPatch>>(256);
        }

        public void Clear() {
            LoadRequests.Clear();
            UnloadRequests.Clear();
        }
    }

    public class GrassMesh : MonoBehaviour {
        private Transform _transform;
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        public Transform Transform {
            get { return _transform; }
        }

        public Mesh Mesh {
            get { return _mesh; }
        }

        public MeshFilter MeshFilter {
            get { return _meshFilter; }
        }

        public MeshRenderer MeshRenderer {
            get { return _meshRenderer; }
        }

        private void Awake() {
            _transform = gameObject.GetComponent<Transform>();
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        public void Allocate(Material material, BinaryReader reader) {
            _mesh = new Mesh();
            _mesh.MarkDynamic();
            _meshFilter.mesh = _mesh;
            _meshRenderer.material = material;
        }
    }

    public class JobData {
        public readonly TerrainData TerrainData;
        public readonly MeshData MeshData;

        public JobData(TerrainData terrainData, MeshData meshData) {
            TerrainData = terrainData;
            MeshData = meshData;
        }
    }

    public class MeshData {
        public const int VertsPerQuad = 4;
        public const int TrisPerQuad = 6;

        public readonly int MaxInstances;
        public readonly int Size;

        public readonly XorShiftRandom Random;

        public readonly Vector3[] Vertices;
        public readonly int[] Triangles;
        public readonly Vector2[] Uvs;
        public readonly Vector3[] Normals;
        public Bounds Bounds;

        public MeshData(int maxInstances, int size) {
            MaxInstances = maxInstances;
            Size = size;

            Random = new XorShiftRandom();

            Vertices = new Vector3[VertsPerQuad * maxInstances];
            Triangles = new int[TrisPerQuad * maxInstances];
            Uvs = new Vector2[VertsPerQuad * maxInstances];
            Normals = new Vector3[VertsPerQuad * maxInstances];
            Bounds = new Bounds();
        }
    }

    public class TerrainData {
        public readonly TiledTerrainConfig Config;
        public readonly BinaryReader Reader;

        public readonly float[,] Heights;
        public readonly Vector3[,] Normals;
        public readonly Vector2[,] Splats;

        public TerrainData(TiledTerrainConfig config, BinaryReader reader) {
            Config = config;
            Reader = reader;

            Heights = new float[Config.PatchHeightRes, Config.PatchHeightRes];
            Normals = new Vector3[Config.PatchHeightRes, Config.PatchHeightRes];
            Splats = new Vector2[Config.PatchSplatRes, Config.PatchSplatRes];
        }

        public float SampleHeight(Vector2 pos) {
            int x = (int)(pos.x * Config.PatchHeightRes);
            int y = (int)(pos.y * Config.PatchHeightRes);
            return Heights[x, y];
        }

        public float SampleInterpolatedHeight(Vector2 pos) {
            float fracX = pos.x * (Config.PatchHeightRes - 1);
            float fracY = pos.y * (Config.PatchHeightRes - 1);
            int x = Mathf.FloorToInt(fracX);
            int y = Mathf.FloorToInt(fracY);
            fracX -= x;
            fracY -= y;

            var b = Mathf.Lerp(Heights[x, y], Heights[x + 1, y], fracX); // Bug:  System.IndexOutOfRangeException: Array index is out of range. Why?
            var t = Mathf.Lerp(Heights[x, y + 1], Heights[x + 1, y + 1], fracX);
            return Mathf.Lerp(b, t, fracY);
        }

        public Vector3 SampleNormal(Vector2 pos) {
            int x = (int)(pos.x * Config.PatchHeightRes);
            int y = (int)(pos.y * Config.PatchHeightRes);
            return Normals[x, y];
        }

        public Vector3 SampleInterpolatedNormal(Vector2 pos) {
            float fracX = pos.x * (Config.PatchHeightRes - 1);
            float fracY = pos.y * (Config.PatchHeightRes - 1);
            int x = Mathf.FloorToInt(fracX);
            int y = Mathf.FloorToInt(fracY);
            fracX -= x;
            fracY -= y;

            var b = Vector3.Lerp(Normals[x, y], Normals[x + 1, y], fracX); // Bug:  System.IndexOutOfRangeException: Array index is out of range. Why?
            var t = Vector3.Lerp(Normals[x, y + 1], Normals[x + 1, y + 1], fracX);
            return Vector3.Lerp(b, t, fracY);
        }

        public Vector2 SampleSplat(Vector2 pos) {
            int x = (int)(pos.x * Config.PatchSplatRes);
            int y = (int)(pos.y * Config.PatchSplatRes);
            return Splats[x, y];
        }

        public Vector2 SampleInterpolatedSplat(Vector2 pos) {
            float fracX = pos.x * (Config.PatchSplatRes - 1);
            float fracY = pos.y * (Config.PatchSplatRes - 1);
            int x = Mathf.FloorToInt(fracX);
            int y = Mathf.FloorToInt(fracY);
            fracX -= x;
            fracY -= y;

            var b = Vector2.Lerp(Splats[x, y], Splats[x + 1, y], fracX); // Bug:  System.IndexOutOfRangeException: Array index is out of range. Why?
            var t = Vector2.Lerp(Splats[x, y + 1], Splats[x + 1, y + 1], fracX);
            return Vector2.Lerp(b, t, fracY);
        }
    }

    public enum NodeState {
        Unloaded = 0,
        LoadingQueued = 1,
        Loading = 2,
        Loaded = 3,
        Unloading = 4
    }

    public class GrassPatch : PriorityQueueNode {
        public readonly IntVector2 Coord; // Todo: Don't want to dup this QTNode state, but don't want to make QTNode extend PriorityQueueNode either
        public readonly float Height;

        public float SnowAltitude;
        
        public bool WasInRange;
        public NodeState State = NodeState.Unloaded;
        public IDisposable RunningJob;
        public JobData JobData;
        public GrassMesh Mesh;
       
        public GrassPatch(IntVector2 coord, float height) {
            Coord = coord;
            Height = height;
        }
    }
}