using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RamjetAnvil.Volo {

    public class ParachuteLineRenderer : MonoBehaviour {

        [SerializeField] private GameSettingsProvider _gameSettingsProvider;

        [SerializeField] private float _lineWidth = 0.02f;
        [SerializeField] private float _aeroDynamicLineWidth = 0.03f;
        [SerializeField] private Parachute _parachute;
        [SerializeField] private float _forceScale = 0.025f;

        [SerializeField] private Material _aerodynamicsMaterial;
        [SerializeField] private Material _lineMaterial;

        [SerializeField] private Color _liftColor = new Color (0f, 1f, 0f);
        [SerializeField] private Color _dragColor = new Color (1f, 0f, 0f);

        private LineSet[] _liftLines;
        private LineSet[] _dragLines;
        private LineSet[] _brakeLines;
        private LineSet[] _rearLines;
        private LineSet[] _frontLines;

        private GameObject _lineParent;
        private bool _isInitialized;
        private IDisposable _gameSettingChanges;

        void OnEnable() {
            _lineParent = _lineParent ?? new GameObject("Lines");
            _lineParent.SetActive(true);
        }

        void OnDisable() {
            _lineParent.SetActive(false);
        }

        void OnDestroy() {
            if (_gameSettingChanges != null) {
                _gameSettingChanges.Dispose();    
            }
        }

        public void Initialize(Parachute parachute, GameSettingsProvider gameSettingsProvider) {
            if (!_isInitialized) {
                _gameSettingsProvider = gameSettingsProvider;

                _lineParent = _lineParent ?? new GameObject("Lines");
                _lineParent.transform.SetParent(this.transform);

                _lineMaterial = Resources.Load<Material>("Parachutes/LineMaterial");
                _aerodynamicsMaterial = Resources.Load<Material>("AerodynamicsVisualizerMaterial");

                InstallParachute(parachute);

                _gameSettingChanges = _gameSettingsProvider.SettingChanges.Subscribe(settings => {
                    for (int i = 0; i < _parachute.Sections.Count; ++i) {
                        _liftLines[i].Renderer.gameObject.SetActive(settings.Gameplay.VisualizeAerodynamics);
                        _dragLines[i].Renderer.gameObject.SetActive(settings.Gameplay.VisualizeAerodynamics);
                    }
                });

                _isInitialized = true;
            }
        }

        void InstallParachute(Parachute parachute) {
            _parachute = parachute;

            _liftLines = new LineSet[parachute.Sections.Count];
            _dragLines = new LineSet[parachute.Sections.Count];
            _brakeLines = new LineSet[parachute.Sections.Count];
            _rearLines = new LineSet[parachute.Sections.Count];
            _frontLines = new LineSet[parachute.Sections.Count];

            for (int i = 0; i < parachute.Sections.Count; ++i) {
                var s = parachute.Sections[i];

                _liftLines[i] = CreateLineSet("Lift line " + i, _aeroDynamicLineWidth, _aerodynamicsMaterial, _liftColor);
                _dragLines[i] = CreateLineSet("Drag line " + i, _aeroDynamicLineWidth, _aerodynamicsMaterial, _dragColor);

                var sectionHasLine = !(s.FrontLine == null || s.RearLine == null);
                if (sectionHasLine) {
                    _brakeLines[i] = CreateLineSet("Brake line " + i, _lineWidth, _lineMaterial);
                    _rearLines[i] = CreateLineSet("Rear line " + i, _lineWidth, _lineMaterial);
                    _frontLines[i] = CreateLineSet("Front line " + i, _lineWidth, _lineMaterial);
                }
            }
        }

        LineSet CreateLineSet(string lineName, float width, Material material, Color? color = null) {
            var lineRenderer = new GameObject(lineName).AddComponent<LineRenderer>();
            lineRenderer.gameObject.transform.SetParent(this._lineParent.transform);
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.startColor = color ?? Color.white;
            lineRenderer.endColor = color ?? Color.white;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.material = material;
            return new LineSet(lineRenderer, new Vector3[2]);
        }

        void LateUpdate() {
            if (_parachute == null || _parachute.Pilot == null) {
                return;
            }

            for (int i = 0; i < _parachute.Sections.Count; ++i) {
                var s = _parachute.Sections[i];

                var cellPosition = s.Cell.transform.TransformPoint(s.Cell.Airfoil.ChordPressurePoint);

                // Lift ray
                var lift = cellPosition + s.Cell.Airfoil.LiftForce * _forceScale;
                var liftLine = _liftLines[i];
                liftLine.Points[0] = cellPosition;
                liftLine.Points[1] = lift;
                liftLine.Renderer.SetPositions(liftLine.Points);

                var drag = cellPosition + s.Cell.Airfoil.DragForce * _forceScale;
                var dragLine = _dragLines[i];
                dragLine.Points[0] = cellPosition;
                dragLine.Points[1] = drag;
                dragLine.Renderer.SetPositions(dragLine.Points);

                // TODO Find a better way of knowing before hand which lines to draw
                if (s.RearLine == null || s.FrontLine == null || s.RearLine.Joint == null || s.FrontLine.Joint == null ||
                    s.RearLine.Joint.connectedBody == null || s.FrontLine.Joint.connectedBody == null) {
                    continue; // Todo: this check tho :/
                }

                var brakeLine = _brakeLines[i];
                // TODO Too much indirection for performance?
                brakeLine.Points[0] = s.BrakeLine.Joint.transform.TransformPoint(s.BrakeLine.Joint.anchor);
                brakeLine.Points[1] = s.BrakeLine.Joint.connectedBody.gameObject.transform.TransformPoint(s.BrakeLine.Joint.connectedAnchor);
                float brakePull = 1f - s.BrakeLine.Pull;
                var brakeColor = new Color(brakePull, brakePull, 1f, 0.8f);
                brakeLine.Renderer.startColor = brakeColor;
                brakeLine.Renderer.endColor = brakeColor;
                brakeLine.Renderer.SetPositions(brakeLine.Points);

                var rearLine = _rearLines[i];
                // TODO Too much indirection for performance?
                rearLine.Points[0] = s.RearLine.Joint.transform.TransformPoint(s.RearLine.Joint.anchor);
                rearLine.Points[1] = s.RearLine.Joint.connectedBody.gameObject.transform.TransformPoint(s.RearLine.Joint.connectedAnchor);
                float rearPull = 1f - s.RearLine.Pull;
                var rearColor = new Color(1f, rearPull, rearPull, 0.8f);
                rearLine.Renderer.startColor = rearColor;
                rearLine.Renderer.endColor = rearColor;
                rearLine.Renderer.SetPositions(rearLine.Points);
                
                var frontLine = _frontLines[i];
                // TODO Too much indirection for performance?
                frontLine.Points[0] = s.FrontLine.Joint.transform.TransformPoint(s.FrontLine.Joint.anchor);
                frontLine.Points[1] = s.FrontLine.Joint.connectedBody.gameObject.transform.TransformPoint(s.FrontLine.Joint.connectedAnchor);
                float frontPull = 1f - s.FrontLine.Pull;
                var frontColor = new Color(frontPull, 1f, frontPull, 0.8f);
                frontLine.Renderer.startColor = frontColor;
                frontLine.Renderer.endColor = frontColor;
                frontLine.Renderer.SetPositions(frontLine.Points);
            }
        }
    }
}
