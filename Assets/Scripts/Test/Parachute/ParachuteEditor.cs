using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using Assets.Scripts.Ui.GenericComponents;
using RamjetAnvil;
using RamjetAnvil.Cameras;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.InputModule;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Ui;
using RamjetAnvil.Volo.Util.UnitOfMeasure;
using RamjetAnvil.InputModule;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.CourseEditing;
using UnityEngine;
using RTEditor;
using IHighlightable = RamjetAnvil.InputModule.IHighlightable;
using InputRange = RamjetAnvil.InputModule.InputRange;
using Range = RamjetAnvil.InputModule.Range;

namespace RamjetAnvil.Volo {
    public class ParachuteEditor : MonoBehaviour {

        /*
         * Radius: The canopy shape follows an ellipse controlled by its horizontal and vertical radius (Affects canopy stability, turning capability.)
         * Area: Total area of the canopy shape. Span * Chord. (Affects lift, drag and airspeed. Smaller canopies are faster. Ignores tapering for now.)
         * RiggingAngle: Rotates the canopy around its span axis. (Changes default angle of attack in flight to be more or less aggressive.)
         * HeightOffset: Changes the line lengths to suspend the pilot closer or farther from the canopy. (Affects canopy stability and pendulum swing action.)
         * RigAttachPosition: The points on the pilot's harness that the canopy's lines connect to. (Affects weight shift behavior, and the tendency for lines to twist.)
         * PilotWeight: The weight of the pilot and the harness combined. (Affects canopy stability, canopy response speed, airspeed.)
         * WeightShiftMagnitude: The amount that the pilot can lean around in their harness. (Affects coordinated turning ability. Effect drops off with canopy size.)
         */

        public static readonly IImmutableDictionary<WidgetId, Tooltip> Tooltips = new Dictionary<WidgetId, Tooltip> {
            { WidgetId.Radius, new Tooltip("Radius", 
                "The canopy shape follows an ellipse controlled by its horizontal and vertical radius ",
                "Affects canopy stability and turning capability.")},
            { WidgetId.Area, new Tooltip("Area", 
                "Span × Chord = total area of the canopy shape",
                "Affects lift, drag and airspeed. Smaller canopies are faster. Ignores tapering for now.")},
            { WidgetId.RiggingAngle, new Tooltip("Rigging angle", 
                "Rotates the canopy around its span axis.", 
                "Changes default angle of attack in flight to be more or less aggressive.")},
            { WidgetId.HeightOffset, new Tooltip("Height offset", 
                "Changes the line lengths to suspend the pilot closer or farther from the canopy.",
                "Affects canopy stability and pendulum swing action.")},
            { WidgetId.RigAttachPosition, new Tooltip("Rig attach position", 
                "The points on the pilot's harness that the canopy's lines connect to.",
                "Affects weight shift behavior, and the tendency for lines to twist.")},
            { WidgetId.PilotWeight, new Tooltip("Pilot weight", 
                "The weight of the pilot and the harness combined.",
                "Affects canopy stability, canopy response speed, and airspeed.")},
            { WidgetId.WeightShiftMagnitude, new Tooltip("Pilot weight shift magnitude", 
                "The amount that the pilot can lean around in their harness.",
                "Affects coordinated turning ability. Effect drops off with canopy size.")}
        }.ToFastImmutableEnumDictionary();

        [SerializeField] private Vector3 _pilotTorsoScale;
        [SerializeField] private MaterialHighlight _canopyHighlight;

        [SerializeField] private GameObject _gizmosParent;
        [SerializeField] private ScaleWidget _radiusGizmo;
        [SerializeField] private RotationGizmo _riggingAngleGizmo;
        [SerializeField] private TranslateWidget _heightOffsetWidget;
        [SerializeField] private TranslateWidget _rigAttachPosition;
        [SerializeField] private ScaleWidget _pilotWeightGizmo;
        [SerializeField] private ScaleWidget _weightShiftMagnitudeGizmo;

        [SerializeField] private GLCircle _weightShiftVisualizer;

        [SerializeField] private ColorPicker _cellColorPicker;
        [SerializeField] private ParachutePropertyUi _summaryText;
        [SerializeField] private ParachutePropertyUi _riggingAngle;
        [SerializeField] private ParachutePropertyUi _heightOffsetText;
        //[SerializeField] private ParachutePropertyUi _rigAttachPositionText;
        [SerializeField] private ParachutePropertyUi _pilotWeight;
        [SerializeField] private ParachutePropertyUi _pilotWeightShiftMagnitude;

        [SerializeField] private TooltipView _tooltipView;

        [SerializeField] private GameObject _additionalSettingsParent;
        [SerializeField] private ParachuteConfigView _parachuteConfigView;

        [SerializeField][Dependency] private ActiveLanguage _activeLanguage;
        [SerializeField][Dependency] private GameSettingsProvider _gameSettingsProvider;
        [SerializeField][Dependency] private CameraManager _cameraManager;

        private float _timer;

        private bool _isInitialized;
        private ITypedDataCursor<WidgetId?> _selectedWidget;
        private ConfigDescription configDescription;

        public event Action OnMouseIsUnavailable;
        public event Action OnMouseIsAvailable;

        private void OnEnable() {
            _parachuteConfigView.gameObject.SetActive(true);
        }

        private void OnDisable() {
            _parachuteConfigView.gameObject.SetActive(false);
        }

        public void Initialize(ITypedDataCursor<EditorState> editorState, IObservable<Parachute> editorParachute) {
            if (!_isInitialized) {
                var configCursor = editorState.To(c => c.Config);

                var parachuteConfigViewModel = new ParachuteConfigViewModel(configCursor);
                _parachuteConfigView.Initialize(parachuteConfigViewModel);
                RegisterUIHover(_additionalSettingsParent.AddComponent<DefaultHoverEventSource>());

                var parachuteColor = configCursor.To(c => c.Color);
                _cellColorPicker.onValueChanged.AddListener(color => parachuteColor.Set(color));
                RegisterUIHover(_cellColorPicker.gameObject.AddComponent<DefaultHoverEventSource>());

                _selectedWidget = editorState.To(c => c.SelectedWidget);
                
                configDescription = new ConfigDescription(configCursor, _pilotTorsoScale);

                // TODO Use GUI camera?
                var mainCamera = _cameraManager.Rig.GetMainCamera();

                _radiusGizmo.InputRange = configDescription.Radius;
                _pilotWeightGizmo.InputRange = configDescription.PilotWeight;
                _weightShiftMagnitudeGizmo.InputRange = configDescription.WeightShiftMagnitude;
                _heightOffsetWidget.InputRange = configDescription.HeightOffset;
                _rigAttachPosition.InputRange = configDescription.RigAttachPosition;
                _riggingAngleGizmo.InputRange = configDescription.RiggingAngle;
                _riggingAngleGizmo.Camera = mainCamera;

                RegisterDraggable(_radiusGizmo.gameObject, WidgetId.Radius);
                RegisterDraggable(_pilotWeightGizmo.gameObject, WidgetId.PilotWeight);
                RegisterDraggable(_heightOffsetWidget.gameObject, WidgetId.HeightOffset);
                RegisterDraggable(_weightShiftMagnitudeGizmo.gameObject, WidgetId.WeightShiftMagnitude);
                RegisterGizmoHandlers(_riggingAngleGizmo, WidgetId.RiggingAngle);

                editorState.To(c => c.SelectedWidget)
                    .OnUpdate
                    .Subscribe(selectedWidget => {
                        // Render tooltip
                        //_rigAttachPositionText.gameObject.SetActive(selectedWidget == WidgetId.RigAttachPosition);

                        if (selectedWidget.HasValue) {
                            var tooltip = Tooltips[selectedWidget.Value];
                            var description = tooltip.Description + "\n\n<i>" + tooltip.Effect + "</i>";
                            _tooltipView.SetState(tooltip.Name, description);
                            _tooltipView.gameObject.SetActive(true);
                        } else {
                            _tooltipView.gameObject.SetActive(false);
                        }
                    });

                var colliderSet = ColliderSet.Box("ParachuteUICellColliders", LayerMask.NameToLayer("UI"));

                editorState.To(c => c.IsEditing)
                    .OnUpdate
                    .Subscribe(isEditing => {
                        colliderSet.Parent.SetActive(isEditing);
                        _gizmosParent.SetActive(isEditing);
                        _additionalSettingsParent.SetActive(isEditing);
                        _cellColorPicker.gameObject.SetActive(isEditing);
                    });

                configCursor.OnUpdate
                    .CombineLatest(
                        _gameSettingsProvider.SettingChanges, 
                        _activeLanguage.TableUpdates,
                        (config, settings, languageTable) => new {config, settings, languageTable})
                    .Subscribe(x => {
                        _parachuteConfigView.SetState(x.languageTable.AsFunc);

                        var props = ParachuteProperties.FromConfig(x.config);
                        if (x.settings.Gameplay.UnitSystem == UnitSystem.Metric) UpdateEditorState(x.config, props);
                        else UpdateEditorState(x.config, props.ToImperial());
                    });

                colliderSet.Parent.SetParent(gameObject);
                var canopyDragHandler = colliderSet.Parent.AddComponent<SurfaceDragHandler>();
                canopyDragHandler.Dragging += (camTransform, value) => {
                    var currentValue = configDescription.Volume.Value;
                    currentValue.x += value.x;
                    currentValue.z -= value.y;
                    configDescription.Volume.SetValue(currentValue);
                };
                _canopyHighlight.Highlightable = canopyDragHandler;

                RegisterDraggable(colliderSet.Parent, WidgetId.Area);
                editorParachute.Subscribe(p => {
                    _canopyHighlight.Renderer = p.CanopyMesh;

                    _rigAttachPosition.transform.position = p.Pilot.Torso.transform.position;
                    _rigAttachPosition.transform.rotation = p.Pilot.Torso.transform.rotation;

                    colliderSet.SetSize(p.Sections.Count);
                    for (var i = 0; i < p.Sections.Count; i++) {
                        var cell = p.Sections[i].Cell;
                        colliderSet.UpdateCollider(i, cell.Collider);

                        var isLastCell = i == p.Sections.Count - 1;
                        if (isLastCell) {
                            var gizmoTransform = cell.transform.MakeImmutable()
                                .TranslateLocally(0.8f)
                                .UpdateScale(Vector3.one)
                                .UpdateRotation(p.Root.rotation);
                            _radiusGizmo.transform.Set(gizmoTransform);
                        }
                    }
                });

//                _rigAttachPosition.OnPositionChanged += newPosition => {
//                    configDescription.RigAttachPosition.SetValue(newPosition);
//                };
//                _rigAttachPosition.ActiveAxes = ActiveAxis.X | ActiveAxis.Y | ActiveAxis.Z;
                RegisterDraggable(_rigAttachPosition.gameObject, WidgetId.RigAttachPosition);

                /*
                 * What are the problems that we need to solve:
                 * - Use a single source of truth to render the parachute
                 * - Use a single source of truth to render the GUI
                 * - The GUI widgets should not know more than just the value they need to update and
                 *   what the value is they need to render (use cursors into the app state to do this)
                 * - Use unity as a rendering engine but not as a logic engine (possibly too hard to do this now)
                 * 
                 * 
                 * 
                 * Gizmo:
                 * 
                 * - Show scaling and rotation widgets at all times and
                 *   couple them to an InputRange.
                 * - (optional) Draw box around selected thing
                 * 
                 */

                _isInitialized = true;
            }
        }

        private void UpdateEditorState<TMeasureSystem>(
            ParachuteConfig config, 
            ParachuteProperties<TMeasureSystem> props) 
            where TMeasureSystem : MeasureSystem {

            var editorOrigin = transform.MakeImmutable();
            var canopyCentroidWorld = editorOrigin.TranslateLocally(ParachuteMaths.GetCanopyCentroid(config));

            _heightOffsetWidget.transform.Set(editorOrigin.TranslateLocally(y: 1f));
            _heightOffsetText.transform.Set(editorOrigin.TranslateLocally(y: config.HeightOffset + 1f, z: -0.5f));
            _heightOffsetText.Text.Clear();
            props.HeightOffset.FormatTo(_heightOffsetText.Text, precision: 2);

            _riggingAngleGizmo.transform.Set(canopyCentroidWorld);

            _cellColorPicker.CurrentColor = config.Color;

            _summaryText.Text.Clear();
            _summaryText.Text
                .Append("Difficulty level: ")
                .Append(ParachuteMaths.GetDifficulty(config).Stringify())
                .Append("\n")
                .Append(config.NumCells)
                .Append(" cells (")
                .Append(config.NumToggleControlledCells)
                .Append(" braked), ");
            props.CanopyMass.FormatTo(_summaryText.Text, precision: 1);
            _summaryText.Text.Append("\n");
            props.Span.FormatTo(_summaryText.Text, precision: 1);
            _summaryText.Text.Append(" × ");
            props.Chord.FormatTo(_summaryText.Text, precision: 1);
            _summaryText.Text.Append(" = ");
            props.Area.FormatTo(_summaryText.Text, precision: 0);
            _summaryText.transform.Set(canopyCentroidWorld.TranslateLocally(new Vector3(-3f, 2.5f, 0f)));

            // TODO Use mutable strings
            _riggingAngle.transform.Set(canopyCentroidWorld);
            _riggingAngle.Text.Clear();
            props.RiggingAngle.FormatTo(_riggingAngle.Text, precision: 0);

//            _rigAttachPositionText.text = string.Format("{0:0.##}, {1:0.##}, {2:0.##}", 
//                props.RigAttachPosition.Value.x,
//                props.RigAttachPosition.Value.y,
//                props.RigAttachPosition.Value.z);
//            _rigAttachPositionText.transform.Set(rigAttachPositionTransform);
                    
            var pilotWeightTransform = editorOrigin.TranslateLocally(y: -0.8f);
            _pilotWeight.transform.Set(pilotWeightTransform.TranslateLocally(y: -0.4f));
            _pilotWeightGizmo.transform.Set(pilotWeightTransform);

            _pilotWeight.Text.Clear();
            props.PilotWeight.FormatTo(_pilotWeight.Text, precision: 0);
            _pilotWeight.Text.Append(" (");
            props.WingLoading.FormatTo(_pilotWeight.Text, precision: 2);
            _pilotWeight.Text.Append(")");

            var pilotWeightShiftTransform = editorOrigin;
            _weightShiftMagnitudeGizmo.transform.Set(pilotWeightShiftTransform);
            _pilotWeightShiftMagnitude.transform.Set(pilotWeightShiftTransform.TranslateLocally(x: -1.1f));
            _weightShiftVisualizer.transform.position = pilotWeightShiftTransform.Position;
            _weightShiftVisualizer.Radius = config.WeightshiftMagnitude;

            _pilotWeightShiftMagnitude.Text.Clear();
            props.WeightShiftMagnitude.FormatTo(_pilotWeightShiftMagnitude.Text, precision: 2);

            _radiusGizmo.UpdateState();
            _heightOffsetWidget.UpdateState();
            _rigAttachPosition.UpdateState();
            _pilotWeightGizmo.UpdateState();
            _weightShiftMagnitudeGizmo.UpdateState();
        }

        private void RegisterGizmoHandlers(Gizmo gizmo, WidgetId widgetId) {
            RegisterHighlightable(gizmo, widgetId);

            gizmo.OnGizmoDragStart += g => NotifyMouseUnavailability();
            gizmo.OnGizmoDragEnd += g => NotifyMouseAvailability();
        }

        private void RegisterHighlightable(IHighlightable highlightable, WidgetId widgetId) {
            highlightable.OnHighlight += () => {
                _selectedWidget.Set(widgetId);
            };
            highlightable.OnUnHighlight += () => {
                _selectedWidget.Set(null);
            };
        }

        private void RegisterDraggable(GameObject g, WidgetId widgetId) {
            var draggables = g.GetComponentsInChildren<IDraggable>();
            for (var i = 0; i < draggables.Length; i++) {
                var draggable = draggables[i];
                draggable.OnDragStart += NotifyMouseUnavailability;
                draggable.OnDragStop += NotifyMouseAvailability;
            }

            var highlightables = g.GetComponentsInChildren<IHighlightable>();
            for (var i = 0; i < highlightables.Length; i++) {
                var highlightable = highlightables[i];
                RegisterHighlightable(highlightable, widgetId);
            }
        }

        private void RegisterUIHover(IHoverEventSource eventSource) {
            eventSource.OnCursorEnter += NotifyMouseUnavailability;
            eventSource.OnCursorExit += NotifyMouseAvailability;
        }

        private void NotifyMouseUnavailability() {
            // Assumes only one gizmo is interacted with at a time
            if (OnMouseIsUnavailable != null) OnMouseIsUnavailable();
        }

        private void NotifyMouseAvailability() {
            // Assumes only one gizmo is interacted with at a time
            if (OnMouseIsAvailable != null) OnMouseIsAvailable();
        }

        private class ConfigDescription {
            private const int NumberOfSteps = 50;

            public readonly InputRange Radius;
            public readonly InputRange Volume;
            public readonly InputRange RiggingAngle;
            public readonly InputRange HeightOffset;
            public readonly InputRange RigAttachPosition;
            public readonly InputRange PilotWeight;
            public readonly InputRange WeightShiftMagnitude;

            // TODO Track updates of individual config values to improve rendering performance

            public ConfigDescription(ITypedDataCursor<ParachuteConfig> config, Vector3 pilotTorsoScale) {
                Radius = new InputRange(
                    CreateRange(-8f, 8f, config.To(c => c.RadiusVertical)),
                    CreateRange(1f, 16f, config.To(c => c.RadiusHorizontal))
                );
                Volume = new InputRange(
                    CreateRange(1f, 16f, config.To(c => c.Span)),
                    zAxisDescription: CreateRange(0.5f, 5, config.To(c => c.Chord))
                );
                RiggingAngle = new InputRange(
                    CreateRange(0, 30, config.To(c => c.RiggingAngle))
                );
                HeightOffset = new InputRange(
                    yAxisDescription: CreateRange(0f, 16f, config.To(c => c.HeightOffset))
                );
                var rigAttachCursor = config.To(c => c.RigAttachPos);
                RigAttachPosition = new InputRange(
                    RigAttachComponent(0f, 1f, rigAttachCursor.To(DefaultPath.Vector3X), pilotTorsoScale.x),
                    RigAttachComponent(0f, 1f, rigAttachCursor.To(DefaultPath.Vector3Y), pilotTorsoScale.y),
                    RigAttachComponent(-1f, 1f, rigAttachCursor.To(DefaultPath.Vector3Z), pilotTorsoScale.z)
                );
                PilotWeight = new InputRange(
                    CreateRange(40f, 200f, config.To(c => c.PilotWeight))
                );
                WeightShiftMagnitude = new InputRange(
                    CreateRange(0.1f, 0.5f, config.To(c => c.WeightshiftMagnitude))
                );
            }

            private Range RigAttachComponent(float min, float max, ITypedDataCursor<float> cursor, float scale) {
                return new Range(
                    min * scale, 
                    max * scale, 
                    () => cursor.Get() * scale, 
                    value => cursor.Set(value / scale), 
                    NumberOfSteps);
            }

            private Range CreateRange(float min, float max, ITypedDataCursor<float> cursor) {
                return new Range(min, max, cursor.Get, cursor.Set, NumberOfSteps);
            }
        }

        public class EditorState {
            public WidgetId? SelectedWidget;
            public bool IsEditing;
            public ParachuteConfig Config;

            public EditorState(ParachuteConfig config) {
                SelectedWidget = null;
                IsEditing = false;
                Config = config;
            }
        }

        public class Tooltip {
            public readonly string Name;
            public readonly string Description;
            public readonly string Effect;

            public Tooltip(string name, string description, string effect) {
                Name = name;
                Description = description;
                Effect = effect;
            }
        }

        public enum WidgetId {
            Radius, Area, RiggingAngle, RigAttachPosition, PilotWeight, WeightShiftMagnitude, HeightOffset
        }
        public static readonly IList<WidgetId> WidgetIds = EnumUtils.GetValues<WidgetId>();
    }
}

