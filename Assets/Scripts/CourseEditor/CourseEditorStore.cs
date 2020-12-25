using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Assets.Scripts.CourseEditor;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.CourseEditing
{
    public static class CourseEditorStore {

        public static IObservable<CourseEditorState> Create(CourseEditorActions actions, CourseData courseData, 
            Ref<ImmutableTransform> cameraTransform) {
            var commands = actions.CreateProp.Select(StoreAction.Create<Unit>("createProp"))
                .Merge(actions.CreatePropOnLocation.Select(StoreAction.Create<ImmutableTransform>("createPropOnLocation")))
                .Merge(actions.DeleteProp.Select(StoreAction.Create<PropId>("deleteProp")))
                .Merge(actions.DeleteSelectedProp.Select(StoreAction.Create<Unit>("deleteSelectedProp")))
                .Merge(actions.UpdateProp.Select(StoreAction.Create<Tuple<PropId, ImmutableTransform>>("updateProp")))
                .Merge(actions.HighlightProp.Select(StoreAction.Create<Maybe<PropId>>("highlightProp")))
                .Merge(actions.SelectProp.Select(StoreAction.Create<Maybe<PropId>>("selectProp")))
                .Merge(actions.SelectPropType.Select(StoreAction.Create<PropType>("selectPropType")))
                .Merge(actions.ReorderProps.Select(StoreAction.Create<IImmutableList<PropId>>("reorderProps")))
                .Merge(actions.Undo.Select(StoreAction.Create<Unit>("undo")))
                .Merge(actions.Redo.Select(StoreAction.Create<Unit>("redo")));

            var initialHistoricalState = courseData.ToCourseEditorState();
            var updatesWithHistory = commands
                .Scan(new AppState<HistoricalCourseEditorState>(initialHistoricalState), (currentAppState, command) => {
                    var currentState = currentAppState.CurrentState;
                    AppState<HistoricalCourseEditorState> newAppState = currentAppState;
                    if (command.Id.Equals("createProp")) {
                        var propTransform = cameraTransform.Deref().TranslateLocally(new Vector3(0, 0, 30));
                        newAppState = History.AddNewState(currentAppState, currentState.AddProp(propTransform));
                    } 
                    else if (command.Id.Equals("createPropOnLocation")) {
                        newAppState = History.AddNewState(currentAppState, currentState.AddProp((ImmutableTransform)command.Arguments));
                    } 
                    else if (command.Id.Equals("updateProp")) {
                        var args = (Tuple<PropId, ImmutableTransform>) command.Arguments;
                        var prop = currentState.Props[args._1];
                        var isTransformChanged = !prop.Transform.Equals(args._2);
                        if (isTransformChanged) {
                            var updatedProp = prop.UpdateTransform(args._2);
                            newAppState = History.AddNewState(currentAppState, currentState.UpdateProp(updatedProp));
                        } else {
                            newAppState = currentAppState;
                        }
                    }
                    else if (command.Id.Equals("deleteProp")) {
                        var propId = (PropId) command.Arguments;
                        if (currentState.PropOrder.Contains(propId)) {
                            newAppState = History.AddNewState(currentAppState, currentState.DeleteProp(propId));    
                        }
                    }
                    else if (command.Id.Equals("deleteSelectedProp")) {
                        if (currentState.SelectedProp.IsJust) {
                            newAppState = History.AddNewState(currentAppState, currentState.DeleteProp(currentState.SelectedProp.Value));    
                        }
                    }
                    else if (command.Id.Equals("selectProp")) {
                        var selectedPropId = (Maybe<PropId>)command.Arguments;
                        if (selectedPropId.IsNothing ||
                            (selectedPropId.IsJust && currentState.Props.ContainsKey(selectedPropId.Value))) {
                            newAppState = History.AddNewState(currentAppState, currentState.SelectProp(selectedPropId));
                        }
                    }
                    else if (command.Id.Equals("selectPropType")) {
                        newAppState = History.AddNewState(currentAppState, currentState.SelectPropType((PropType)command.Arguments));
                    }
                    else if (command.Id.Equals("reorderProps")) {
                        newAppState = History.AddNewState(currentAppState, currentState.UpdatePropOrder((IImmutableList<PropId>)command.Arguments));
                    }
                    else if (command.Id.Equals("undo")) {
                        newAppState = History.Undo(currentAppState);
                    }
                    else if (command.Id.Equals("redo")) {
                        newAppState = History.Redo(currentAppState);
                    }
                    else {
                        newAppState = currentAppState;
                    }

                    return newAppState;
                })
                .Select(appState => {
                    return appState.CurrentState.UpdateUndoRedoAvailability(
                        !appState.UndoStack.IsEmpty, !appState.RedoStack.IsEmpty);
                })
                .StartWith(initialHistoricalState)
                .Publish();

            var initialTransientState = new TransientCourseEditorState(courseData.Name, Maybe.Nothing<PropId>(), TransformTool.Move);
            var transientUpdates = actions.HighlightProp.Select(StoreAction.Create<Maybe<PropId>>("highlightProp"))
                .Merge(actions.SelectTransformTool.Select(StoreAction.Create<TransformTool>("selectTransformTool")))
                .Merge(actions.UpdateName.Select(StoreAction.Create<string>("updateName")))
                .Scan(initialTransientState, (state, command) => {
                    if (command.Id.Equals("highlightProp")) {
                        return state.HighlightProp((Maybe<PropId>)command.Arguments);
                    }
                    else if (command.Id.Equals("selectTransformTool")) {
                        // TODO Prevent transform tool selection when no prop is selected
                        return state.SelectTransformTool((TransformTool)command.Arguments);
                    }
                    else if (command.Id.Equals("updateName")) {
                        return state.UpdateName((string) command.Arguments);
                    }
                    return state;
                })
                .DistinctUntilChanged()
                .StartWith(initialTransientState)
                .CombineLatest(updatesWithHistory, (transientState, histState) => {
                    // Check if the highlighted prop still exists
                    var highlightedProp = transientState.HighlightedProp;
                    var isHighlightedItemDeleted = highlightedProp.IsJust &&
                                                   !histState.Props.ContainsKey(highlightedProp.Value);

                    if (isHighlightedItemDeleted) {
                        return transientState.HighlightProp(Maybe.Nothing<PropId>());
                    }
                    return transientState;
                });

            var updates = updatesWithHistory.CombineLatest(
                transientUpdates, (histState, transientState) => new CourseEditorState(histState, transientState))
                .DistinctUntilChanged()
                .Replay(1);

            updates.Connect();
            updatesWithHistory.Connect();

            return updates;
        }
    }

    public static class CourseEditor
    {
        public static readonly IImmutableList<TransformTool> TransformTools = 
            ImmutableList.Create(TransformTool.Move, TransformTool.Rotate); 

        public static HistoricalCourseEditorState UpdateUndoRedoAvailability(this HistoricalCourseEditorState state, bool isUndoPossible, 
            bool isRedoPossible) {
            return new HistoricalCourseEditorState(state.Id, state.SelectedPropType, state.SelectedProp, state.Props, state.PropOrder,
                isUndoPossible, isRedoPossible);
        }

        private static HistoricalCourseEditorState Update(this HistoricalCourseEditorState state, Maybe<PropId> selectedPropId,
            IImmutableDictionary<PropId, EditorProp> props, IImmutableList<PropId> propOrder) {
            return new HistoricalCourseEditorState(state.Id, state.SelectedPropType, selectedPropId, props, propOrder,
                state.IsUndoPossibe, state.IsRedoPossibe);
        }

        public static HistoricalCourseEditorState SelectPropType(this HistoricalCourseEditorState state, PropType propType) {
            var newProps = ImmutableDictionary<PropId, EditorProp>.Empty;
            var newPropOrder = ImmutableList<PropId>.Empty;
            var newSelectedProp = Maybe.Nothing<PropId>();
            for (int i = 0; i < state.PropOrder.Count; i++) {
                var oldProp = state.Props[state.PropOrder[i]];

                var newProp = new EditorProp { Id = PropId.CreateRandomId(), PropType = propType, Transform = oldProp.Transform };
                newProps = newProps.Add(newProp.Id, newProp);
                newPropOrder = newPropOrder.Add(newProp.Id);

                if (state.SelectedProp.Equals(Maybe.Just(oldProp.Id))) {
                    newSelectedProp = Maybe.Just(newProp.Id);
                }
            }
            return new HistoricalCourseEditorState(state.Id, propType, newSelectedProp, newProps, newPropOrder,
                state.IsUndoPossibe, state.IsRedoPossibe);
        }

        public static HistoricalCourseEditorState AddProp(this HistoricalCourseEditorState state, ImmutableTransform transform) {
            var propId = PropId.CreateRandomId();
            var newProp = new EditorProp { Id = propId, PropType = state.SelectedPropType, Transform = transform };
            // Immediately select the newly created prop
            var selectedProp = Maybe.Just(propId);
            return state.Update(selectedProp, state.Props.Add(newProp.Id, newProp), state.PropOrder.Add(propId));
        }

        public static HistoricalCourseEditorState UpdateProp(this HistoricalCourseEditorState state, EditorProp prop) {
            if (state.Props.ContainsKey(prop.Id)) {
                return state.Update(state.SelectedProp, state.Props.SetItem(prop.Id, prop), state.PropOrder);
            } else {
                return state;
            }
        }

        public static HistoricalCourseEditorState DeleteProp(this HistoricalCourseEditorState state, PropId id) {
            var selectedProp = state.SelectedProp.Equals(Maybe.Just(id)) ? Maybe.Nothing<PropId>() : state.SelectedProp;
            return state.Update(selectedProp, state.Props.Remove(id), state.PropOrder.Remove(id));
        }

        public static TransientCourseEditorState HighlightProp(this TransientCourseEditorState state, Maybe<PropId> id) {
            return new TransientCourseEditorState(state.CourseName, id, state.SelectedTransformTool);
        }

        public static HistoricalCourseEditorState SelectProp(this HistoricalCourseEditorState state, Maybe<PropId> selectedPropId)
        {
            return state.Update(selectedPropId, state.Props, state.PropOrder);
        }

        public static HistoricalCourseEditorState UpdatePropOrder(this HistoricalCourseEditorState state, IImmutableList<PropId> newPropOrder) {
            return state.Update(state.SelectedProp, state.Props, newPropOrder);
        }

        public static TransientCourseEditorState SelectTransformTool(this TransientCourseEditorState state,
            TransformTool tool) {
                return new TransientCourseEditorState(state.CourseName, state.HighlightedProp, tool);
        }

        public static TransientCourseEditorState UpdateName(this TransientCourseEditorState state, string name) {
            return new TransientCourseEditorState(name, state.HighlightedProp, state.SelectedTransformTool);
        }
    }

    public enum TransformTool {
        Move, Rotate
    }
}
