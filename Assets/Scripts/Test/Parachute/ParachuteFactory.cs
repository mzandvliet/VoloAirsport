using System;
using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.Profiling;

namespace RamjetAnvil.Volo {
    
    /// <summary>
    /// Tools for reasoning about parachutes directly and only from their configs, with as little
    /// Unity-specific stuff as dependency.
    /// </summary>
    public static class ParachuteMaths {
        /// <summary>
        /// Basic filter making sure no nonsensical parachutes get passed into the factory
        /// </summary>
        /// <param name="config"></param>
        public static void ValidateConfig(ParachuteConfig config) {
            if (config.NumCells%2 == 0) {
                config.NumCells += 1; // Todo: respect min/max
            }
            config.NumToggleControlledCells = Mathf.Clamp(config.NumToggleControlledCells, 1, config.NumCells/2);
        }

        /// <summary>
        /// Get the midpoint for all the cells in the canopy definition. Useful for rotating the cells to
        /// achieve variable rigging angle in factory state.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Vector3 GetCanopyCentroid(ParachuteConfig config) {
            Vector3 centroid = Vector3.zero;

            float circumference = config.RadiusHorizontal*Mathf.PI*2f;
            float widthRatio = config.Span/circumference;
            float phaseStep = widthRatio*Mathf.PI*2f/config.NumCells;
            float phase = -widthRatio*Mathf.PI;

            for (int i = 0; i < config.NumCells; i++) {
                Vector3 p0 = new Vector3(
                    Mathf.Sin(phase)*config.RadiusHorizontal,
                    Mathf.Cos(phase)*config.RadiusVertical + config.HeightOffset - config.RadiusVertical);
                Vector3 p1 = new Vector3(
                    Mathf.Sin(phase + phaseStep)*config.RadiusHorizontal,
                    Mathf.Cos(phase + phaseStep)*config.RadiusVertical + config.HeightOffset - config.RadiusVertical);

                centroid += Vector3.Lerp(p0, p1, 0.5f);

                phase += phaseStep;
            }

            centroid /= (float) config.NumCells;

            return centroid;
        }

        /// <summary>
        /// Get an immutable transform representing the spatial state of a single cell from a canopy
        /// in factory state
        /// </summary>
        /// <param name="config"></param>
        /// <param name="centroid"></param>
        /// <param name="cellIndex"></param>
        /// <returns></returns>
        public static ImmutableTransform GetCellTransform(ParachuteConfig config, Vector3 centroid, int cellIndex) {
            float circumference = config.RadiusHorizontal*Mathf.PI*2f;
            float widthRatio = config.Span/circumference;
            float phaseStep = widthRatio*Mathf.PI*2f/config.NumCells;
            float phase = -widthRatio*Mathf.PI;

            phase += phaseStep*cellIndex;

            Vector3 p0 = new Vector3(
                Mathf.Sin(phase)*config.RadiusHorizontal,
                Mathf.Cos(phase)*config.RadiusVertical + config.HeightOffset - config.RadiusVertical);
            Vector3 p1 = new Vector3(
                Mathf.Sin(phase + phaseStep)*config.RadiusHorizontal,
                Mathf.Cos(phase + phaseStep)*config.RadiusVertical + config.HeightOffset - config.RadiusVertical);

            Vector3 pos = Vector3.Lerp(p0, p1, 0.5f);
            Quaternion rot = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, p1 - p0));
            Vector3 scale = new Vector3(Vector3.Distance(p0, p1), config.Thickness, config.Chord); // todo: areaMultiplier

            // Apply rigging angle

            Quaternion riggingRotation = Quaternion.AngleAxis(config.RiggingAngle, Vector3.right);
            Vector3 centroidPos = pos - centroid;

            var t = new ImmutableTransform(
                centroid + riggingRotation*centroidPos,
                rot*riggingRotation,
                scale);

            return t;
        }

        /* ======= Parachute Analysis & Evaluation ====== */

        public static float GetAspectRatio(ParachuteConfig config) {
            // Todo: span^2/measured-area
            return config.Span/config.Chord;
        }

        public static float GetRectArea(ParachuteConfig config) {
            // Todo: elliptical/tapered shape
            return config.Chord*config.Span;
        }

        // Returned units: kg/m²
        public static float GetWingLoading(ParachuteConfig config) {
            return config.PilotWeight/GetRectArea(config);
        }

        // Returned units: oz/ft²
        public static float WingLoadingToImperial(float wingLoading) {
            return wingLoading/4.8717f;
        }

        public enum ExperienceLevel {
            Basic,
            Intermediate,
            Advanced,
            Expert
        }

        public static string Stringify(this ExperienceLevel experienceLevel) {
            switch (experienceLevel) {
                case ExperienceLevel.Basic:
                    return "Basic";
                case ExperienceLevel.Intermediate:
                    return "Intermediate";
                case ExperienceLevel.Advanced:
                    return "Advanced";
                case ExperienceLevel.Expert:
                    return "Expert";
                default:
                    throw new ArgumentOutOfRangeException("experienceLevel", experienceLevel, null);
            }
        }

        // From: http://pureskydive.com/canopy-wing-loading-calculator/
        public static ExperienceLevel GetDifficulty(ParachuteConfig config) {
            var wingLoading = WingLoadingToImperial(GetWingLoading(config));

            if (wingLoading > 1.80f) {
                return ExperienceLevel.Expert;
            }
            if (wingLoading > 1.35f) {
                return ExperienceLevel.Advanced;
            }
            if (wingLoading > 1.05f) {
                return ExperienceLevel.Intermediate;
            }

            return ExperienceLevel.Basic;
        }
    }

    /// <summary>
    /// Factory for constructing Unity-object structures representing parachute systems defined by
    /// their configs
    /// </summary>
    public static class UnityParachuteFactory {
        #region UnityParachuteStateCreation

        public static Parachute Create(ParachuteConfig config, ImmutableTransform transform, string name) {
            ParachuteMaths.ValidateConfig(config);
            Parachute p = CreateNewInstance(config, transform, name);

            // Todo: Differentiate between game use and editor use

            CreateCells(p);
            CreateInterCellJoints(p);
            CreateRiggingLines(p);
            CreateControlGroups(p);
            p.CalculateCanopyBounds();
            AddSounds(p);

            return p;
        }

        private static Parachute CreateNewInstance(ParachuteConfig config, ImmutableTransform transform, string name) {
            GameObject root = new GameObject("Parachute");
            root.transform.Set(transform);
            root.name = name;

            Parachute p = root.AddComponent<Parachute>();
            p.Init(config);

            return p;
        }

        public static Bounds CanopyBounds(Parachute p) {
            var bounds = p.Sections[0].Cell.Collider.bounds;
            for (int i = 1; i < p.Sections.Count; i++) {
                var section = p.Sections[i];
                bounds.Encapsulate(section.Cell.Collider.bounds);
            }
            return bounds;
        }

        private static readonly List<Collider> ColliderCache = new List<Collider>();
        public static Bounds PilotBounds(Wingsuit w) {
            ColliderCache.Clear();
            w.GetComponentsInChildren(ColliderCache);

            var bounds = ColliderCache[0].bounds;
            for (int i = 1; i < ColliderCache.Count; i++) {
                var collider = ColliderCache[i];
                bounds.Encapsulate(collider.bounds);
            }
            return bounds;
        }

        public static float OrbitDistance(Parachute p) {
            var canopyBounds = p.CanopyBounds.size;
            return Mathf.Max(canopyBounds.x, canopyBounds.y, p.Config.HeightOffset);
        }

        private static void CreateCells(Parachute p) {
            // Layout n number of cells from left to right around the top section of an ellipse
            // Todo: current algorithm does not preserve planform area as it warps the wing around the ellipse
            // Todo: GUI planform area should report post-warp area, including tapering and such

            var config = p.Config;
            var centroid = ParachuteMaths.GetCanopyCentroid(config);

            // Todo: model induced drag based on aspect ratio and shape tapering
            float aspectRatio = ParachuteMaths.GetAspectRatio(p.Config);

            for (int i = 0; i < config.NumCells; i++) {
                float areaMultiplier = 1f;//config.PlanformAreaEllipse.Evaluate(i/(float) (config.NumCells - 1));

                var cell = CreateCell(p, areaMultiplier, aspectRatio);
                var t = ParachuteMaths.GetCellTransform(config, centroid, i);
                LayoutCell(cell, t);
                cell.name = "Cell_" + i;
                p.Sections.Add(new Section() {Cell = cell});
            }
        }

        private static Cell CreateCell(Parachute p, float areaMultiplier, float aspectRatio) {
            var config = p.Config;
            var obj = new GameObject();
            var cell = obj.AddComponent<Cell>();

            obj.tag = "Parachute";

            obj.transform.parent = p.Root;

            var col = obj.AddComponent<BoxCollider>();
            obj.transform.localScale = new Vector3(p.Config.Span / p.Config.NumCells, p.Config.Thickness, p.Config.Chord) * 0.75f;

            cell.Body = obj.AddComponent<Rigidbody>();
            cell.Body.mass = config.Mass/config.NumCells; // Todo
            cell.Body.centerOfMass = new Vector3(0f, -0.1f, 0.2f);

            cell.Airfoil = obj.AddComponent<ParachuteAirfoil>();
            cell.Airfoil.ChordPressurePoint = new Vector3(0f, 0f, 0.25f);
            cell.Airfoil.Area = (config.Span*config.Chord) / config.NumCells * areaMultiplier;
            cell.Airfoil.TotalAspectRatio = aspectRatio;
            cell.Airfoil.Definition = config.AirfoilDefinition;

            return cell;
        }

        private static void LayoutCell(Cell cell, ImmutableTransform t) {
            cell.transform.localPosition = t.Position;
            cell.transform.localRotation = t.Rotation;
        }

        private static void CreateInterCellJoints(Parachute p) {
            for (int i = 0; i < p.Sections.Count - 1; i++) {
                var cell = p.Sections[i].Cell;

                cell.Joint = cell.gameObject.AddComponent<ConfigurableJoint>();
                cell.Joint.enablePreprocessing = false;

                cell.Joint.autoConfigureConnectedAnchor = false;
                cell.Joint.anchor = new Vector3(0.5f, 0f, 0.1f);
                cell.Joint.connectedAnchor = new Vector3(-0.5f, 0f, 0.1f);

                cell.Joint.xMotion = ConfigurableJointMotion.Locked;
                cell.Joint.yMotion = ConfigurableJointMotion.Locked;
                cell.Joint.zMotion = ConfigurableJointMotion.Locked;
                cell.Joint.angularXMotion = ConfigurableJointMotion.Free;
                cell.Joint.angularYMotion = ConfigurableJointMotion.Free;
                cell.Joint.angularZMotion = ConfigurableJointMotion.Free;

                cell.Joint.linearLimit = new SoftJointLimit() {
                    limit = 0.1f,
                    bounciness = 0f
                };

                cell.Joint.connectedBody = p.Sections[i + 1].Cell.Body;
            }
        }

        private static void CreateRiggingLines(Parachute p) {
            int splitIndex = p.Sections.Count/2 + 1;

            for (int i = 0; i < p.Sections.Count; i++) {
                if (i == p.Sections.Count/2) {
                    continue;
                }
                var section = p.Sections[i];

                var rigAnchor = p.Config.RigAttachPos;
                rigAnchor.x *= i < splitIndex ? -1f : 1f;

                section.BrakeLine = CreateLine(p, section.Cell, new Vector3(0f, 0f, -0.5f),
                    // Note: Brake line is not a full jointed line
                    rigAnchor - Vector3.forward*0.02f, Parachute.Brakes);
                section.RearLine = CreateLine(p, section.Cell, new Vector3(0f, 0f, -0.33f),
                    rigAnchor - Vector3.forward*0.02f, Parachute.RearRisers);
                section.FrontLine = CreateLine(p, section.Cell, new Vector3(0f, 0f, 0.4f),
                    rigAnchor + Vector3.forward*0.02f, Parachute.FrontRisers);

    //            section.RearLine.JointPullMagnitude = p.Config.RearRiserPullMagnitude;
    //            section.FrontLine.JointPullMagnitude = p.Config.FrontRiserPullMagnitude;
            }
        }

        private static Line CreateLine(Parachute p, Cell cell, Vector3 cellAnchor, Vector3 rigAnchor, int lineType) {
            var joint = cell.gameObject.AddComponent<ConfigurableJoint>();
            joint.enablePreprocessing = false;
            joint.anchor = cellAnchor;
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = rigAnchor;

            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;

            joint.linearLimit = new SoftJointLimit {
                limit = Vector3.Distance(cell.transform.TransformPoint(cellAnchor), p.Root.TransformPoint(rigAnchor))
            };

            var line = new Line(joint, cell.Airfoil, lineType);

            return line;
        }

//        private static Line CreatePhantomLine(Parachute p, Cell cell, Vector3 cellAnchor, Vector3 rigAnchor, int lineType) {
//            var line = CreateLine(p, cell, cellAnchor, rigAnchor, lineType);
//            line.Joint.xMotion = ConfigurableJointMotion.Free;
//            line.Joint.yMotion = ConfigurableJointMotion.Free;
//            line.Joint.zMotion = ConfigurableJointMotion.Free;
//
//            return line;
//        }

        private static void CreateControlGroups(Parachute p) {
            for (int i = 0; i < p.Config.NumToggleControlledCells; i++) {
                var section = p.Sections[i];
                p.LeftToggleSections.Add(section);
            }

            for (int i = 0; i < p.Config.NumToggleControlledCells; i++) {
                var section = p.Sections[p.Sections.Count - 1 - i];
                p.RightToggleSections.Add(section);
            }

            for (int i = 0; i < p.Config.NumCells/2; i++) {
                var section = p.Sections[i];
                p.LeftRiserSections.Add(section);
            }

            for (int i = 0; i < p.Config.NumCells/2; i++) {
                var section = p.Sections[p.Sections.Count - 1 - i];
                p.RightRiserSections.Add(section);
            }
        }

        private static void AddSounds(Parachute p) {
            AddSound(p.Sections[0].Cell, "event:/wingsuit/wing_left");
            AddSound(p.Sections[p.Sections.Count/2].Cell, "event:/wingsuit/wing_tail");
            AddSound(p.Sections[p.Sections.Count-1].Cell, "event:/wingsuit/wing_right");
        }

        private static ParachuteWingSound AddSound(Cell cell, string eventName) {
            var s = cell.gameObject.AddComponent<ParachuteWingSound>();
            s.Wing = cell.Airfoil;
            s.EventName = eventName;
            s.Initialize();
            s.OnSpawn();
            return s;
        }

        #endregion

        #region UnityParachuteStateManagement

        public static void SetKinematic(Parachute p) {
            for (int i = 0; i < p.Sections.Count; i++) {
                var section = p.Sections[i];
                section.SetKinematic();
            }
        }

        public static void SetPhysical(Parachute p) {
            for (int i = 0; i < p.Sections.Count; i++) {
                var section = p.Sections[i];
                section.SetPhysical();
            }
        }

        /// <summary>
        /// Takes an existing parachute system and resets it to factory state
        /// </summary>
        /// <param name="p"></param>
        public static void LayoutCells(Parachute p) {
            var config = p.Config;
            var centroid = ParachuteMaths.GetCanopyCentroid(config);

            for (int i = 0; i < config.NumCells; i++) {
                var cell = p.Sections[i].Cell;
                var t = ParachuteMaths.GetCellTransform(config, centroid, i);
                LayoutCell(cell, t);
            }
        }

        /// <summary>
        /// Takes a factory state parachute system and relaxes the lines to prevent physics spazzing
        /// </summary>
        /// <param name="p"></param>
        public static void Relax(Parachute p) {
            var config = p.Config;

            var offset = p.Root.up*0.5f;
            for (int i = 0; i < config.NumCells; i++) {
                var cell = p.Sections[i].Cell;
                cell.Body.position -= offset;
            }
        }

        #endregion
    }

    public static class UnityParachuteMeshFactory {
        private static MeshData _meshData;
        private static Pool<int[]> _loopPool;
        private static List<int[]> _edgeLoops;

        public static void Initialize(ParachuteMeshConfig cf) {
            _meshData = new MeshData();

            const int maxLoops = 256;
            Queue<int[]> q = new Queue<int[]>(maxLoops);
            for (int i = 0; i < maxLoops; i++) {
                int[] loop = new int[cf.CellChordLoops + cf.CellChordLoops - 2]; // Todo: Make more straightforward
                q.Enqueue(loop);
            }
            _loopPool = new Pool<int[]>(q, ClearLoop);

            _edgeLoops = new List<int[]>(maxLoops);
        }

        public static void CreateSkinnedMesh(Parachute p, ParachuteMeshConfig cf, Material mat) {
            if (_meshData == null) {
                throw new Exception("UnityParachuteMeshFactory needs to be initialized before use!");
            }

            Clear();

            // Todo: Reuse mesh, game objects, as much as possible. Create pool of cells.

            Profiler.BeginSample("GOCreation");
            GameObject mo = new GameObject("ParachuteMesh");
            mo.transform.parent = p.Root;
            SkinnedMeshRenderer r = mo.AddComponent<SkinnedMeshRenderer>();
            r.updateWhenOffscreen = true; // Todo: This is not great
            r.skinnedMotionVectors = true;
            r.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            Profiler.EndSample();
            
            Profiler.BeginSample("SetupBones");
            SetupBones(mo.transform, _meshData, p, cf);
            Profiler.EndSample();

            Profiler.BeginSample("EdgeLoops");
            CreateCellLoops(_meshData, _edgeLoops, p.Config, cf);
            Profiler.EndSample();

            Profiler.BeginSample("Caps");
            CreateCap(_edgeLoops[0], _meshData, cf, 0, -0.33f);
            int startIndex = _meshData.Indices.Count;
            CreateCap(_edgeLoops[_edgeLoops.Count - 1], _meshData, cf, p.Config.NumCells - 1, 0.33f);
            int endIndex = _meshData.Indices.Count;
            InvertFaces(_meshData, startIndex, endIndex);
            Profiler.EndSample();

            Profiler.BeginSample("CreateMesh");
            Mesh m = MeshData.CreateMesh(_meshData);
            Profiler.EndSample();

            Profiler.BeginSample("Finalize");
            mat.color = p.Config.Color;
            r.bones = _meshData.Bones.ToArray();
            r.sharedMesh = m;
            r.material = mat;
            p.CanopyMesh = r;
            Profiler.EndSample();
        }

        private static void Clear()
        {
            _meshData.Clear();
            for (int i = 0; i < _edgeLoops.Count; i++)
            {
                _loopPool.Return(_edgeLoops[i]);
            }
            _edgeLoops.Clear();
        }

        private static void SetupBones(Transform parent, MeshData md, Parachute p, ParachuteMeshConfig cf) {
            var c = p.Config;

            float spanStep = c.Span / c.NumCells;

            p.Bones = new Transform[c.NumCells];

            for (int i = 0; i < c.NumCells; i++) {
                var go = new GameObject("Cell_" + i);
                var t = go.transform;

                t.position = Vector3.right * (0.5f * spanStep) + Vector3.right * (spanStep * i);
                t.rotation = Quaternion.Euler(-0.5f, 0f, 0f); // Todo: align with median chord
                t.parent = parent;

                var b = t.worldToLocalMatrix * parent.localToWorldMatrix;

                md.Bones.Add(t);
                md.BindPoses.Add(b);

                p.Bones[i] = t;
            }
        }

        private static void CreateCellLoops(MeshData md, List<int[]> loops, ParachuteConfig cfg, ParachuteMeshConfig cf) {
            // Create the loops
            float spanStep = cfg.Span / cfg.NumCells / cf.CellSpanLoops;
            for (int i = 0; i < cfg.NumCells; i++) {
                for (int j = 0; j < cf.CellSpanLoops; j++) {
                    float z = spanStep * (i * cf.CellSpanLoops + j);
                    float s = cf.SpanCellCurvature.Evaluate(j / ((float)cf.CellSpanLoops));

                    var l = _loopPool.Take();
                    CreateCellEdgeLoop(md, l, cfg, cf, z, s);
                    loops.Add(l);
                }
            }

            // Create the last loop
            var ll = _loopPool.Take();
            CreateCellEdgeLoop(md, ll, cfg, cf, cfg.Span, cf.SpanCellCurvature.Evaluate(1f));
            loops.Add(ll);

            // Create the faces between the loops
            for (int i = 0; i < loops.Count - 1; i++) {
                CreateLoopFaces(loops[i], loops[i + 1], md);
            }
        }

        private static void CreateCellEdgeLoop(MeshData md, int[] loop, ParachuteConfig cfg, ParachuteMeshConfig cf, float spanPos, float scale) {
            // Note: top vertex row has the outermost chord tip verts. Bottom reuses those.

            int idx = 0;

            float zScale = (0.8f + scale * 0.2f);
            float zMin = -0.5f * cfg.Chord * zScale;

            // Top
            for (int i = 0; i < cf.CellChordLoops; i++) {
                float chordLerp = i / (float)(cf.CellChordLoops - 1);
                float c = cf.ChordLineCurvature.Evaluate(chordLerp);
                c += cf.ChordUpperThickness.Evaluate(chordLerp) * scale;
                Vector3 v = new Vector3(
                    spanPos,
                    c,
                    zMin + chordLerp * cfg.Chord * zScale
                );
                md.Vertices.Add(v);
                BoneWeight w = GetBoneWeight(v, cfg);
                md.Weights.Add(w);
                loop[idx++] = md.Vertices.Count - 1;
            }

            // Bottom
            for (int i = 1; i < cf.CellChordLoops - 1; i++) {
                float chordLerp = 1f - i / (float)(cf.CellChordLoops - 1);
                float c = cf.ChordLineCurvature.Evaluate(chordLerp);
                c += cf.ChordLowerThickness.Evaluate(chordLerp) * scale;
                Vector3 v = new Vector3(
                    spanPos,
                    c,
                    zMin + chordLerp * cfg.Chord * zScale
                );
                md.Vertices.Add(v);
                BoneWeight w = GetBoneWeight(v, cfg);
                md.Weights.Add(w);
                loop[idx++] = md.Vertices.Count - 1;
            }
        }

        private static BoneWeight GetBoneWeight(Vector3 vertPos, ParachuteConfig c) {
            float cellSpan = c.Span / c.NumCells;
            float v = -0.5f * cellSpan + vertPos.x; // vert.x in boneSpace

            int boneL = Mathf.FloorToInt(v / cellSpan);
            int boneR = boneL + 1;

            BoneWeight w = new BoneWeight();

            if (boneL >= 0) {
                float boneLPos = boneL * cellSpan;
                w.boneIndex0 = boneL;
                w.weight0 = cellSpan - (v - boneLPos);
            }

            if (boneR < c.NumCells) {
                float boneRPos = boneR * cellSpan;
                w.boneIndex1 = boneR;
                w.weight1 = cellSpan + (v - boneRPos);
            }

            if (w.weight0 < w.weight1) {
                int iTemp = w.boneIndex0;
                float wTemp = w.weight0;
                w.boneIndex0 = w.boneIndex1;
                w.weight0 = w.weight1;
                w.boneIndex1 = iTemp;
                w.weight1 = wTemp;
            }

            w = Normalize(w);

            return w;
        }

        private static BoneWeight Normalize(BoneWeight b) {
            float sum = b.weight0 + b.weight1;// + b.weight2 + b.weight3;
            b.weight0 /= sum;
            b.weight1 /= sum;
//            b.weight2 /= sum;
//            b.weight3 /= sum;
            return b;
        }

        private static void CreateLoopFaces(int[] a, int[] b, MeshData md) {
            for (int i = 0; i < a.Length - 1; i++) {
                md.Indices.Add(a[i + 0]);
                md.Indices.Add(a[i + 1]);
                md.Indices.Add(b[i + 0]);

                md.Indices.Add(a[i + 1]);
                md.Indices.Add(b[i + 1]);
                md.Indices.Add(b[i + 0]);
            }

            md.Indices.Add(a[a.Length - 1]);
            md.Indices.Add(a[0]);
            md.Indices.Add(b[a.Length - 1]);

            md.Indices.Add(a[0]);
            md.Indices.Add(b[0]);
            md.Indices.Add(b[a.Length - 1]);
        }

        private static void CreateCap(int[] loop, MeshData md, ParachuteMeshConfig cf, int cell, float bulge) {
            // Create a strip of verts through the middle of the foil.
            List<int> midVerts = new List<int>();
            midVerts.Add(loop[0]);
            for (int i = 1; i < loop.Length / 2; i++) {
                Vector3 a = md.Vertices[loop[i]];
                Vector3 b = md.Vertices[loop[loop.Length - i]];
                Vector3 v = Vector3.Lerp(a, b, 0.5f);

                v.x += cf.ChordUpperThickness.Evaluate(i / (loop.Length / 2f)) * bulge;

                md.Vertices.Add(v);

                var w = new BoneWeight() {
                    boneIndex0 = cell,
                    weight0 = 1f
                };
                md.Weights.Add(w);

                midVerts.Add(md.Vertices.Count - 1);
            }
            midVerts.Add(loop[loop.Length / 2]);

            // Apart from the front and aft points, this can all be quads
            for (int i = 1; i < midVerts.Count - 2; i++) {
                // Bot
                md.Indices.Add(midVerts[i + 0]);
                md.Indices.Add(loop[i + 1]);
                md.Indices.Add(loop[i + 0]);

                md.Indices.Add(midVerts[i + 0]);
                md.Indices.Add(midVerts[i + 1]);
                md.Indices.Add(loop[i + 1]);
                //           
                // Top
                md.Indices.Add(loop[loop.Length - (i + 0)]);
                md.Indices.Add(loop[loop.Length - (i + 1)]);
                md.Indices.Add(midVerts[i + 0]);

                md.Indices.Add(loop[loop.Length - (i + 1)]);
                md.Indices.Add(midVerts[i + 1]);
                md.Indices.Add(midVerts[i + 0]);
            }

            // Now finish up by adding 4 missing corner triangles

            // Afttop
            md.Indices.Add(midVerts[0]);
            md.Indices.Add(midVerts[1]);
            md.Indices.Add(loop[1]);

            // Aftbot
            md.Indices.Add(midVerts[1]);
            md.Indices.Add(midVerts[0]);
            md.Indices.Add(loop[loop.Length - 1]);

            // FrontTop
            md.Indices.Add(midVerts[midVerts.Count - 2]);
            md.Indices.Add(midVerts[midVerts.Count - 1]);
            md.Indices.Add(loop[loop.Length / 2 - 1]);

            // FrontBot
            md.Indices.Add(midVerts[midVerts.Count - 1]);
            md.Indices.Add(midVerts[midVerts.Count - 2]);
            md.Indices.Add(loop[loop.Length / 2] + 1);
        }

        private static void InvertFaces(MeshData md, int triStart, int triEnd) {
            int delta = triEnd - triStart;
            int tris = delta / 3;

            for (int i = 0; i < tris; i++) {
                int a = md.Indices[triStart + i * 3 + 0];
                int b = md.Indices[triStart + i * 3 + 1];
                int c = md.Indices[triStart + i * 3 + 2];

                md.Indices[triStart + i * 3 + 0] = c;
                md.Indices[triStart + i * 3 + 1] = b;
                md.Indices[triStart + i * 3 + 2] = a;
            }
        }

        private static void ClearLoop(int[] loop) {
            for (int i = 0; i < loop.Length; i++) {
                loop[i] = 0;
            }
        }
    }

    [Serializable]
    public class ParachuteMeshConfig {
        [SerializeField]
        public AnimationCurve ChordLineCurvature;
        [SerializeField]
        public AnimationCurve ChordUpperThickness;
        [SerializeField]
        public AnimationCurve ChordLowerThickness;
        [SerializeField]
        public AnimationCurve SpanCellCurvature;

        public int CellChordLoops;
        public int CellSpanLoops;
    }

    public class MeshData {
        public List<Vector3> Vertices;
        public List<int> Indices;
        public List<BoneWeight> Weights;

        public List<Transform> Bones;
        public List<Matrix4x4> BindPoses;

        public MeshData() {
            Vertices = new List<Vector3>(10000);
            Indices = new List<int>(10000);
            Weights = new List<BoneWeight>(10000);

            Bones = new List<Transform>(20);
            BindPoses = new List<Matrix4x4>(20);
        }

        public static Mesh CreateMesh(MeshData md) {
            Mesh m = new Mesh();
            m.vertices = md.Vertices.ToArray();
            m.triangles = md.Indices.ToArray();
            m.boneWeights = md.Weights.ToArray();

            m.bindposes = md.BindPoses.ToArray();

            m.RecalculateNormals();
            m.RecalculateBounds();

            return m;
        }

        public void Clear() {
            Vertices.Clear();
            Indices.Clear();
            Weights.Clear();
            Bones.Clear();
            BindPoses.Clear();
        }
    }

    /* For our procgen exploits to work in realtime, we can try pooling
     * our intermediate mesh structures.
     */ 

    public class Pool<T> {
        private readonly Queue<T> _items;
        private readonly Action<T> _clear;

        public Pool(Queue<T> items, Action<T> clear) {
            _items = items;
            _clear = clear;
        }

        public T Take() {
            if (_items.Count == 0) {
                throw new Exception("Pool is empty");
            }

            var item = _items.Dequeue();
            return item;
        }

        public void Return(T item) {
            _clear(item);
            _items.Enqueue(item);
        }
    }
}