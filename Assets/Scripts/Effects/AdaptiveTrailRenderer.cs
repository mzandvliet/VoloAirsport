/*
 * Adapted from : http://wiki.unity3d.com/index.php?title=OptimizedTrailRenderer
 * 
 * Changes:
 * - Uses a circular buffer and cached, constant-size memory to prevent dynamic allocs.
 * 
 * Todo:
 * - Use per-node invalidation to avoid having to regenerate the entire mesh
 * - Vertex positioning in shader?
 * - Geometry shaders?
 */

using System.Collections;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.Rendering;

public class AdaptiveTrailRenderer : MonoBehaviour, ISpawnable {
    [SerializeField, Dependency]
    private CameraManager _cameraManager;
    [SerializeField, Dependency]
    private WindManager _wind;
    [SerializeField, Dependency("gameClock")]
    private AbstractUnityClock _clock;

    [SerializeField] private float _windVelocityScale = 1f;

    [SerializeField]
    private Material _material;
    private Material _instanceMaterial;

    // Lifetime of each point
    [SerializeField]
    private float _maxPointLifeTime = 1f;

    // Segments
    [SerializeField]
    private Segment[] _segments = new[]
        {
            new Segment() { Color = new Color(1f,1f,1f,1f), Width = 0f },
            new Segment() { Color = new Color(1f,1f,1f,0.5f), Width = 1f },
            new Segment() { Color = new Color(1f,1f,1f,0f), Width = 0f }
        };

    [SerializeField] private AnimationCurve _alphaCurve = new AnimationCurve(new Keyframe[] {
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1.0f),
        new Keyframe(1f, 0f) 
    });

    [SerializeField]
    private float _widthMultiplier = 1f;

    // Segment creation data
    [SerializeField]
    private int _maxPoints = 128;

    // Object
    private GameObject _trailObject;
    private Renderer _meshRenderer;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private Vector2[] _uvs;
    private int[] _triangles;
    private Color[] _meshColors;

    private Vector3 _lastTransformPosition;

    private float _opacity = 1f;

    // Points
    private Bounds _bounds;
    private CircularBuffer<Point> _points;
    private CircularBuffer<Vector3> _pointPositions;
    private CircularBuffer<Vector3> _pointWindVelocities;

    private bool _isEmitting;
    private bool _isInitialized;

    public float WidthMultiplier {
        get { return _widthMultiplier; }
        set { _widthMultiplier = value; }
    }

    public bool IsEmitting {
        get { return _isEmitting; }
    }

    public float Opacity {
        get { return _opacity; }
        set { _opacity = Mathf.Clamp01(value); }
    }

    void Awake() {
        if (_segments.Length < 3) {
            Debug.LogError("At least three segment profiles need to be defined!");            
        }

        _trailObject = new GameObject(gameObject.name + "_TrailMesh");

        Transform lineParent = AerodynamicsVisualizationManager.GetOrCreateLineParent();
        _trailObject.transform.parent = lineParent;

        MeshFilter meshFilter = _trailObject.AddComponent<MeshFilter>();
        _mesh = meshFilter.mesh;
        _mesh.MarkDynamic();

        _meshRenderer = _trailObject.AddComponent<MeshRenderer>();
        _instanceMaterial = new Material(_material);
        _meshRenderer.material = _instanceMaterial;
        _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        _meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        _meshRenderer.receiveShadows = false;
        _meshRenderer.enabled = false;

        _vertices = new Vector3[(_maxPoints + 1) * 2];
        _uvs = new Vector2[(_maxPoints + 1) * 2];
        _triangles = new int[_maxPoints * 6];
        _meshColors = new Color[(_maxPoints + 1) * 2];

        _points = new CircularBuffer<Point>(_maxPoints);
        _pointPositions = new CircularBuffer<Vector3>(_maxPoints);
        _pointWindVelocities = new CircularBuffer<Vector3>(_maxPoints);

        _isInitialized = true;

        Reset();
        Emit();
    }


    public void OnSpawn() {
        _lastTransformPosition = transform.position;
    }

    public void OnDespawn() {
    }

    public void Emit() {
        if (!_isInitialized) {
            return;
        }

        _meshRenderer.enabled = true;
        _isEmitting = true;
    }

    public void Stop() {
        if (!_isInitialized) {
            return;
        }

        _meshRenderer.enabled = false;
        _isEmitting = false;
        Reset();
    }

    public void Stop(System.Action doneAction) {
        if (!_isInitialized) {
            doneAction();
            return;
        }

        Fade(0f, 1f, () => {
                _meshRenderer.enabled = false;
                Reset();
                doneAction();
            });
        _isEmitting = false;
    }

    public void Reset() {
        if (!_isInitialized) {
            return;
        }

        _points.Clear();
        _pointPositions.Clear();
        _pointWindVelocities.Clear();
    }

    // Moving the whole world? You'll need this
    public void TranslatePoints(Vector3 offset) {
        if (!_isInitialized || !IsEmitting)
            return;

        for (int i = 0; i < _points.Count; i++) {
            _pointPositions[i] = _pointPositions[i] + offset;
            _points[i] = new Point(_points[i].Rotation, _points[i].Velocity, _points[i].TimeCreated);
        }
    }


    private void Update() {
        if (_clock.DeltaTime > 0.01f) {
            // Todo: Have a better sense of time for the rate of point creation
            UpdatePoints(_clock.DeltaTime, (float)_clock.CurrentTime);
        }

        RebuildMesh();
    }

    private void UpdatePoints(float deltaTime, double currentTime) {
        Vector3 deltaPos = transform.position - _lastTransformPosition;
        Vector3 velocity = deltaPos / deltaTime;
        _lastTransformPosition = transform.position;

        RemovePoints();
        _bounds = ConstructBounds();
        UpdatePointPhysics(deltaTime, currentTime, deltaPos);
        AddPoint(velocity);
    }

    private void UpdatePointPhysics(float deltaTime, double currentTime, Vector3 deltaPos) {
        _wind.GetWindVelocities(_pointPositions, _pointWindVelocities, _bounds);

        float lifeTimeInv = 1f / _maxPointLifeTime;

        for (int i = 0; i < _points.Count; i++) {
            Point point = _points[i];
            Vector3 pointPos = _pointPositions[i];

            pointPos += deltaPos * Mathf.Sqrt(1f - (float)(point.LifeTime(currentTime) * lifeTimeInv));
            pointPos += _pointWindVelocities[i] * deltaTime * _windVelocityScale;

            _pointPositions[i] = pointPos;
            _points[i] = point;
        }
    } 

    private void RemovePoints() {
        bool done = false;
        while (!done) {
            if (_points.Count > 0 && _points.Tail().LifeTime((float)_clock.CurrentTime) > _maxPointLifeTime) {
                _points.Dequeue();
                _pointPositions.Dequeue();
                _pointWindVelocities.Dequeue();
            }
            else {
                done = true;                
            }
        }
    }

    private void AddPoint(Vector3 velocity) {
        Point point = new Point(transform.rotation, velocity, _clock.CurrentTime);
        _points.Enqueue(point);
        _pointPositions.Enqueue(transform.position);
        _pointWindVelocities.Enqueue(Vector3.zero);
    }

    private Bounds ConstructBounds() {
        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        // Todo: could optimize this by not considering every point

        for (int i = 0; i < _pointPositions.Count; i++) {
            bounds.Encapsulate(_pointPositions[i]);
        }

        return bounds;
    }

    /* Todo: 
     * - Also use circular buffering for mesh data, in which case we only need to update changes
     */
    private void RebuildMesh() {
        if (_points.Count < 2)
            return;

        Vector3 transformPos = transform.position;

        int maxPoints = _maxPoints;
        CircularBuffer<Point> points = _points;

        Segment[] segments = _segments;
        float widthMultiplier = _widthMultiplier;

        Vector3[] vertices = _vertices;
        Vector2[] uvs = _uvs;
        int[] triangles = _triangles;
        Color[] meshColors = _meshColors;

        Point firstPoint = points[0];
        Point lastPoint = points[_points.Count - 1];
        float uvRatio = (float) (1d / (lastPoint.LifeTime(_clock.CurrentTime) - firstPoint.LifeTime(_clock.CurrentTime)));

        Vector3 viewPosition = _cameraManager.Rig.transform.position;

        for (int i = 0; i < _points.Count; i++) {
            Point point = points[i];
            Vector3 pointPos = _pointPositions[i];

            // Segment profiles are lerped along the entire length of the trail
            float normalizedIndex = (float)i / (float) points.Count;
            float segmentIndex = normalizedIndex * (float)segments.Length;
            int segmentLeftIndex = Mathf.FloorToInt(segmentIndex);

            int segmentRightIndex = Mathf.Min(segmentLeftIndex + 1, segments.Length - 1);
            float segmentLerp = segmentIndex - (float)segmentLeftIndex;

            Segment segmentLeft = segments[segmentLeftIndex];
            Segment segmentRight = segments[segmentRightIndex];

            // Two verts per point
            int vertexIndex = i * 2;

            // Color
            Color segmentColor = Color.Lerp(segmentLeft.Color, segmentRight.Color, segmentLerp);
            segmentColor.a = _alphaCurve.Evaluate(normalizedIndex) * _opacity;
            meshColors[vertexIndex] = segmentColor;
            meshColors[vertexIndex + 1] = segmentColor;

            // Orient trail mesh towards camera position by finding a decent average view direction
            // We only consider the head-point, for speed purposes.
            Vector3 trailTangent = i < _points.Count - 1 ? _pointPositions[i + 1] - pointPos : transformPos - pointPos;
            Vector3 viewDirection = (pointPos - viewPosition);
            Vector3 viewOrthogonal = Vector3.Cross(viewDirection, trailTangent).normalized;

            // Width
            float width = Mathf.Lerp(segmentLeft.Width, segmentRight.Width, segmentLerp) * widthMultiplier;
            Vector3 vertexOffset = viewOrthogonal * (width * 0.5f);
            vertices[vertexIndex] = pointPos + vertexOffset;
            vertices[vertexIndex + 1] = pointPos - vertexOffset;

            // UVs
            float uvX = (float) ((point.LifeTime(_clock.CurrentTime) - firstPoint.LifeTime(_clock.CurrentTime)) * uvRatio); // stretch uv out over entire mesh

            uvs[vertexIndex] = new Vector2(uvX, 0f); 
            uvs[vertexIndex + 1] = new Vector2(uvX, 1f);

            if (i > 0) {
                // Triangles
                int triIndex = (i - 1) * 6;
                triangles[triIndex + 0] = vertexIndex - 2;
                triangles[triIndex + 1] = vertexIndex - 1;
                triangles[triIndex + 2] = vertexIndex - 0;

                triangles[triIndex + 3] = vertexIndex + 1;
                triangles[triIndex + 4] = vertexIndex + 0;
                triangles[triIndex + 5] = vertexIndex - 1;
            }
        }

        // Zero out unused triangles
        for (int i = _points.Count; i < maxPoints; i++) {
            int triIndex = (i - 1) * 6;
            triangles[triIndex + 0] = 0;
            triangles[triIndex + 1] = 0;
            triangles[triIndex + 2] = 0;

            triangles[triIndex + 3] = 0;
            triangles[triIndex + 4] = 0;
            triangles[triIndex + 5] = 0;
        }

        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.uv = uvs;
        _mesh.colors = meshColors;
        _mesh.bounds = _bounds;
    }

    private const string TintColorPropery = "_TintColor";

    public void SetAlpha(float alpha) {
        Color color = _instanceMaterial.GetColor(TintColorPropery);
        color.a = alpha;
        _instanceMaterial.SetColor(TintColorPropery, color);
    }

    public void Fade(float targetAlpha, float time, System.Action doneAction) {
        StopAllCoroutines();
        StartCoroutine(FadeAsync(targetAlpha, time, doneAction));
    }

    private IEnumerator FadeAsync(float targetAlpha, float time, System.Action doneAction) {
        Color color = _instanceMaterial.GetColor(TintColorPropery);
        float startAlpha = color.a;
        float timer = 0f;
        while (timer < time) {
            color.a = Mathf.Lerp(startAlpha, targetAlpha, timer / time);
            _instanceMaterial.SetColor(TintColorPropery, color);

            timer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        color.a = targetAlpha;
        _instanceMaterial.SetColor(TintColorPropery, color);

        if (doneAction != null)
            doneAction();
    }

    [System.Serializable]
    public class Segment {
        public Color Color;
        public float Width;
    }

    public struct Point {
        public Quaternion Rotation;
        public Vector3 Velocity;
        public readonly double TimeCreated;

        public Point(Quaternion rotation, Vector3 velocity, double time) {
            Rotation = rotation;
            Velocity = velocity;
            TimeCreated = time;
        }

        public float LifeTime(double currentTime) {
            return (float)(currentTime - TimeCreated);
        }
    }
}
