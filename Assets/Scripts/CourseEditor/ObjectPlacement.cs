using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RamjetAnvil.Reactive;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Util.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;
using Guid = System.Guid;

namespace RamjetAnvil.Volo.CourseEditing
{
    public struct PropId {
        private string _id;

        public static PropId CreateRandomId() {
            return new PropId(Guid.NewGuid());
        }

        public string Id {
            get { return _id; }
            set { _id = value; }
        }

        public PropId(string id) {
            _id = id;
        }

        public PropId(Guid id)
        {
            _id = id.ToString();
        }

        public bool Equals(PropId other) {
            return string.Equals(_id, other._id);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PropId && Equals((PropId) obj);
        }

        public override int GetHashCode() {
            return (_id != null ? _id.GetHashCode() : 0);
        }

        public override string ToString() {
            return _id;
        }
    }

    public class CourseEditorState {
        private readonly TransientCourseEditorState _transientState;
        private readonly HistoricalCourseEditorState _historicalState;

        public CourseEditorState(HistoricalCourseEditorState historicalState, TransientCourseEditorState transientState) {
            _transientState = transientState;
            _historicalState = historicalState;
        }

        public string Id {
            get { return _historicalState.Id; }
        }

        public string CourseName {
            get { return _transientState.CourseName; }
        }

        public Maybe<PropId> HighlightedProp {
            get { return _transientState.HighlightedProp; }
        }

        public PropType SelectedPropType {
            get { return _historicalState.SelectedPropType; }
        }

        public Maybe<PropId> SelectedProp {
            get { return _historicalState.SelectedProp; }
        }

        public IImmutableDictionary<PropId, EditorProp> Props {
            get { return _historicalState.Props; }
        }

        public IImmutableList<PropId> PropOrder {
            get { return _historicalState.PropOrder; }
        } 

        public TransformTool SelectedTransformTool {
            get { return _transientState.SelectedTransformTool; }
        }

        public bool IsUndoPossible {
            get { return _historicalState.IsUndoPossibe; }
        }

        public bool IsRedoPossible {
            get {
                return _historicalState.IsRedoPossibe;
            }
        }

        protected bool Equals(CourseEditorState other) {
            return Equals(_transientState, other._transientState) && Equals(_historicalState, other._historicalState);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CourseEditorState) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((_transientState != null ? _transientState.GetHashCode() : 0)*397) ^ (_historicalState != null ? _historicalState.GetHashCode() : 0);
            }
        }

    }

    public class TransientCourseEditorState : IEquatable<TransientCourseEditorState> {
        private readonly string _courseName;
        private readonly Maybe<PropId> _highlightedProp;
        private readonly TransformTool _selectedTransformTool;

        public TransientCourseEditorState(string courseName, Maybe<PropId> highlightedProp, TransformTool selectedTransformTool) {
            _courseName = courseName;
            _highlightedProp = highlightedProp;
            _selectedTransformTool = selectedTransformTool;
        }

        public string CourseName {
            get { return _courseName; }
        }

        public Maybe<PropId> HighlightedProp {
            get { return _highlightedProp; }
        }

        public TransformTool SelectedTransformTool {
            get { return _selectedTransformTool; }
        }

        public bool Equals(TransientCourseEditorState other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_courseName, other._courseName) && _highlightedProp.Equals(other._highlightedProp) && _selectedTransformTool == other._selectedTransformTool;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TransientCourseEditorState) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (_courseName != null ? _courseName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _highlightedProp.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) _selectedTransformTool;
                return hashCode;
            }
        }

        public static bool operator ==(TransientCourseEditorState left, TransientCourseEditorState right) {
            return Equals(left, right);
        }

        public static bool operator !=(TransientCourseEditorState left, TransientCourseEditorState right) {
            return !Equals(left, right);
        }
    }

    public class HistoricalCourseEditorState : IEquatable<HistoricalCourseEditorState> {
        private readonly string _id;
        private readonly PropType _selectedPropType;
        private readonly Maybe<PropId> _selectedProp;
        private readonly IImmutableDictionary<PropId, EditorProp> _props;
        private readonly IImmutableList<PropId> _propOrder;
        private readonly bool _isUndoPossible;
        private readonly bool _isRedoPossible;

        public HistoricalCourseEditorState(string id, PropType selectedPropType, Maybe<PropId> selectedProp, IImmutableDictionary<PropId, EditorProp> props,
            IImmutableList<PropId> propOrder, bool isUndoPossible, bool isRedoPossible) {
            _id = id;
            _selectedPropType = selectedPropType;
            _selectedProp = selectedProp;
            _props = props;
            _propOrder = propOrder;
            _isUndoPossible = isUndoPossible;
            _isRedoPossible = isRedoPossible;
        }

        public string Id {
            get { return _id; }
        }

        public PropType SelectedPropType {
            get { return _selectedPropType; }
        }

        public Maybe<PropId> SelectedProp
        {
            get { return _selectedProp; }
        }

        public IImmutableList<PropId> PropOrder {
            get { return _propOrder; }
        }

        public IImmutableDictionary<PropId, EditorProp> Props
        {
            get { return _props; }
        }

        // TODO Implement undo/redo possibility check
        public bool IsUndoPossibe { get { return _isUndoPossible; } }
        public bool IsRedoPossibe { get { return _isRedoPossible; } }

        public bool Equals(HistoricalCourseEditorState other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_id, other._id) && _selectedPropType == other._selectedPropType && _selectedProp.Equals(other._selectedProp) && Equals(_props, other._props) && Equals(_propOrder, other._propOrder) && _isUndoPossible.Equals(other._isUndoPossible) && _isRedoPossible.Equals(other._isRedoPossible);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HistoricalCourseEditorState) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (_id != null ? _id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) _selectedPropType;
                hashCode = (hashCode * 397) ^ _selectedProp.GetHashCode();
                hashCode = (hashCode * 397) ^ (_props != null ? _props.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_propOrder != null ? _propOrder.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _isUndoPossible.GetHashCode();
                hashCode = (hashCode * 397) ^ _isRedoPossible.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(HistoricalCourseEditorState left, HistoricalCourseEditorState right) {
            return Equals(left, right);
        }

        public static bool operator !=(HistoricalCourseEditorState left, HistoricalCourseEditorState right) {
            return !Equals(left, right);
        }
    }

    

    public enum PropType {
        RingEasy, RingNormal, RingHard
    }

    public struct EditorProp {
        public PropId Id { get; set; }
        public PropType PropType { get; set; }
        public ImmutableTransform Transform { get; set; }

        public EditorProp UpdateTransform(ImmutableTransform t) {
            return new EditorProp { Id = Id, PropType = PropType, Transform = t };
        }

        public bool Equals(EditorProp other) {
            return Id.Equals(other.Id) && PropType == other.PropType && Transform.Equals(other.Transform);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EditorProp && Equals((EditorProp) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = Id.GetHashCode();
                hashCode = (hashCode*397) ^ (int) PropType;
                hashCode = (hashCode*397) ^ Transform.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            return string.Format("Id: {0}, PropType: {1}, Transform: {2}", Id, PropType, Transform);
        }
    }

    public struct ObjectSelectionState {
        private readonly Maybe<GameObject> _highlightedObject;
        private readonly Maybe<GameObject> _selectedObject;

        public ObjectSelectionState(Maybe<GameObject> highlightedObject, Maybe<GameObject> selectedObject) {
            _selectedObject = selectedObject;
            _highlightedObject = highlightedObject;
        }

        public Maybe<GameObject> HighlightedObject {
            get { return _highlightedObject; }
        }

        public Maybe<GameObject> SelectedObject {
            get { return _selectedObject; }
        }
    }

    public struct RenderableProp {

        private readonly ImmutableTransform _transform;
        private readonly System.Func<IPooledGameObject> _create;

        public RenderableProp(ImmutableTransform transform, System.Func<IPooledGameObject> create)
        {
            _transform = transform;
            _create = create;
        }

        public ImmutableTransform Transform {
            get { return _transform; }
        }

        public IPooledGameObject Create() {
            return _create();
        }
    }

    public interface IHighlightable : IComponent {
        void Highlight();
        void UnHighlight();
    }

    public interface ISelectable : IComponent {
        void Select();
        void UnSelect();
    }

    public struct RenderablePropId {

        private readonly PropType _propType;
        private readonly PropId _propId;

        public RenderablePropId(PropType propType, PropId propId) {
            _propType = propType;
            _propId = propId;
        }

        public PropType PropType {
            get { return _propType; }
        }

        public PropId PropId {
            get { return _propId; }
        }

        public bool Equals(RenderablePropId other) {
            return _propType == other._propType && _propId.Equals(other._propId);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is RenderablePropId && Equals((RenderablePropId) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) _propType*397) ^ _propId.GetHashCode();
            }
        }
    }

    public class PropRenderer<TId> {

        // Maybe keep state as list and dictionary for efficient updating
        // and lookup
        private readonly IDictionary<TId, IPooledGameObject> _props;
        private readonly IList<TId> _propsToRemove;
        private readonly GameObject _db;
        private readonly System.Func<TId, string> _uniqueUnityId;

        public PropRenderer(System.Func<TId, string> uniqueUnityId)
        {
            _props = new Dictionary<TId, IPooledGameObject>();
            _propsToRemove = new List<TId>();
            _db = new GameObject("__PropDatabase");
            _uniqueUnityId = uniqueUnityId;
        }

        public GameObject GetProp(TId propId) {
            IPooledGameObject pooledObject;
            if (_props.TryGetValue(propId, out pooledObject)) {
                return pooledObject.GameObject;
            }
            return null;
        }

        public void Clear() {
            _props.Clear();
            _propsToRemove.Clear();
            if (!_db.IsDestroyed()) {
                foreach (var prop in _db.GetChildren()) {
                    if (!prop.IsDestroyed()) {
                        GameObject.Destroy(prop);    
                    }
                }    
            }
        }

        public void Update(IImmutableDictionary<TId, RenderableProp> state) {
            foreach (var prop in state) {
                var isPropRendered = _props.ContainsKey(prop.Key);
                if (!isPropRendered) {
                    // Create prop
                    var name = _uniqueUnityId(prop.Key);
                    var pooledProp = prop.Value.Create();

                    pooledProp.GameObject
                        .SetName(name)
                        .SetTransform(prop.Value.Transform)
                        .SetParent(_db);
                    
                    var id = pooledProp.GameObject.GetComponent<Id>() ?? pooledProp.GameObject.AddComponent<Id>();
                    id.Value = name;
                    _props[prop.Key] = pooledProp;
                }
            }

            _propsToRemove.Clear();
            foreach (var pooledProp in _props) {
                var isPropRemoved = !state.ContainsKey(pooledProp.Key);
                if (isPropRemoved) {
                    pooledProp.Value.Dispose();
                    _propsToRemove.Add(pooledProp.Key);
                }
            }
            for (int i = 0; i < _propsToRemove.Count; i++) {
                _props.Remove(_propsToRemove[i]);
            }

            // Update all active props 
            foreach (var pooledProp in _props) {
                var prop = state[pooledProp.Key];
                pooledProp.Value.GameObject.transform.Set(prop.Transform);
            }
        }
    }

    public static class ObjectPlacement
    {
        public static IObservable<Maybe<GameObject>> ObjectHighlight(Camera camera, int layerMask) {
            var rays = UnityRxObservables
                .MouseMove()
                .Select(point => camera.ScreenPointToRay(point));

            var objectHits = UnityRxObservables
                .Raycasts(rays, layerMask, distance: 10000f)
                .Select(hit => hit.collider != null ? Maybe.Just(hit.collider.gameObject) : Maybe.Nothing<GameObject>());

            return objectHits.DistinctUntilChanged((previousObject, currentObject) => {
                return previousObject.IsNothing && currentObject.IsNothing;
            });
        }

        public static IObservable<T> ObjectSelection<T>(IObservable<Maybe<T>> objectHighlight, IObservable<Unit> mouseClick) {
            return objectHighlight
                .Select(obj => mouseClick.Select(x => obj))
                .Switch()
                .Where(obj => obj.IsJust)
                .Select(obj => obj.Value);
        } 
    }
}
