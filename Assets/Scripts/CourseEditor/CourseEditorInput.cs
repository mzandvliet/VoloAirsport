using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using RamjetAnvil.Impero.Util;
using RamjetAnvil.Reactive;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using Input = UnityEngine.Input;

namespace RamjetAnvil.Volo.CourseEditing {

    public static class CourseEditorInput {

        public static void InitializeCourseEditor(CourseEditorActions actions, IObservable<CourseEditorState> store,
            Camera camera, Ref<ImmutableTransform> cameraTransform, RingProps renderableProps,
            PropRenderer<PropId> propRenderer,
            IClock clock) {
            var courseUpdates = store
                .Do(state => {
                    var rProps = state.Props.Select(kvPair => {
                        var propId = kvPair.Key;
                        var prop = kvPair.Value;
                        return new KeyValuePair<PropId, RenderableProp>(
                            propId, new RenderableProp(prop.Transform, renderableProps.Factory[prop.PropType].Spawn));
                    })
                        .ToDictionary();
                    propRenderer.Update(rProps);
                }).Replay(1);
            courseUpdates.Connect();

            var keyboardEvents = UnityRxKeyboard.CreateKeyboard();

            var highlight = ObjectPlacement.ObjectHighlight(camera, LayerMaskUtil.FullMask)
                .Select(go => {
                    if (go.IsJust) {
                        var id = go.Value.GetComponent<Id>();
                        if (id != null && id.Value.StartsWith("__CourseEditor-")) {
                            return Maybe.Just(new PropId(id.Value.Replace("__CourseEditor-", "")));
                        }

                        return Maybe.Nothing<PropId>();
                    }

                    return Maybe.Nothing<PropId>();
                });

            var leftMouseClick = keyboardEvents
                .KeyDown()
                .Where(c => c == KeyCode.Mouse0)
                .Select(c => Unit.Default);

            var selection = ObjectPlacement.ObjectSelection(highlight, leftMouseClick);

            IObservable<Unit> undo;
            IObservable<Unit> redo;
            // Editor already binds to default undo/redo keys so we need a different
            // mapping for them
            if (Application.isEditor) {
                undo = UnityObservable.CreateUpdate<Unit>(observer => {
                    if (UnityEngine.Input.GetKeyDown(KeyCode.Z)) {
                        observer.OnNext(Unit.Default);
                    }
                });
                redo = UnityObservable.CreateUpdate<Unit>(observer => {
                    if (UnityEngine.Input.GetKeyDown(KeyCode.Y))
                    {
                        observer.OnNext(Unit.Default);
                    }
                });
            }
            else {
                var historyCommands = UnityObservable.CreateUpdate<string>(observer => {
                    if (UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.Y)) {
                        observer.OnNext("redo");
                    }
                    else if (UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.Z)) {
                        observer.OnNext("undo");
                    }
                });
                undo = historyCommands
                    .Where(c => c.Equals("undo"))
                    .Select(c => Unit.Default);
                redo = historyCommands
                    .Where(c => c.Equals("redo"))
                    .Select(c => Unit.Default);
            }

            var createProp = UnityObservable.CreateUpdate<Unit>(observer => {
                if (UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.F)) {
                    observer.OnNext(Unit.Default);
                }
            });
            var deleteProp = keyboardEvents
                .KeyDown()
                .Where(key => key == KeyCode.Delete)
                .Select(key => Unit.Default);

            Func<KeyCode, Vector3> key2Translation = key => {
                if (key == KeyCode.W) {
                    return new Vector3(0, 0, 1);
                }
                else if (key == KeyCode.S) {
                    return new Vector3(0, 0, -1);
                }
                else if (key == KeyCode.A) {
                    return new Vector3(-1, 0, 0);
                }
                else if (key == KeyCode.D) {
                    return new Vector3(1, 0, 0);
                }
                else if (key == KeyCode.Q) {
                    return new Vector3(0, -1, 0);
                }
                else if (key == KeyCode.E) {
                    return new Vector3(0, 1, 0);
                }
                return Vector3.zero;
            };

            Func<KeyCode, Vector3> key2Rotation = key => {
                if (key == KeyCode.W) {
                    return new Vector3(1, 0, 0);
                }
                else if (key == KeyCode.S) {
                    return new Vector3(-1, 0, 0);
                }
                else if (key == KeyCode.D) {
                    return new Vector3(0, 1, 0);
                }
                else if (key == KeyCode.A) {
                    return new Vector3(0, -1, 0);
                }
                return Vector3.zero;
            };

            var selectedTransformTool = courseUpdates
                .Select(state => state.SelectedTransformTool)
                .DistinctUntilChanged(EnumComparer<TransformTool>.Instance);

            var switchTransformTool = selectedTransformTool.Select(tool => {
                return UnityObservable.CreateUpdate<Unit>(observer => {
                    if (UnityEngine.Input.GetKeyDown(KeyCode.LeftAlt) || UnityEngine.Input.GetKeyDown(KeyCode.RightAlt)) {
                        observer.OnNext(Unit.Default);
                    }
                })
                .Select(_ => CourseEditor.TransformTools.GetNext(tool));
            }).Switch();

            var ticks = UnityRxObservables.UpdateTicks(() => clock.DeltaTime);

            var keysHeldStream =
                UnityRxKeyboard.CreateKeyboard(new[]
                {KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Q, KeyCode.E, KeyCode.Mouse1}).KeysHeld();

            var combinedTransformation = ticks.CombineLatest(
                keysHeldStream.CombineLatest(selectedTransformTool,
                    (keysHeld, transformTool) => new {KeysHeld = keysHeld, TransformTool = transformTool}),
                (deltaTime, data) => {
                    var rotationSpeed = 80f;
                    var movementSpeed = 16f;
                    var transform = ImmutableTransform.Identity;
                    // Prevent collision with spectator camera movement.
                    if (!data.KeysHeld.Contains(KeyCode.Mouse1)) {
                        if (data.TransformTool == TransformTool.Rotate) {
                            var rotation = data.KeysHeld
                                .Aggregate(Vector3.zero, (current, key) => current + key2Rotation(key))
                                .normalized * rotationSpeed * deltaTime;
                            transform = transform.Rotate(rotation);
                        } else {
                            var translation = data.KeysHeld
                                .Aggregate(Vector3.zero, (current, key) => current + key2Translation(key))
                                .normalized * movementSpeed * deltaTime;
                            transform = transform.Translate(translation);
                        }
                    }

                    return transform;
                });

            var combinedTransformation2 = combinedTransformation
                .Window(() => {
                    return combinedTransformation
                        .Where(t => t.Equals(ImmutableTransform.Identity));
                });

            var currentselectedProp = courseUpdates
                .Select(
                    state => {
                        var prop = state.SelectedProp.IsJust
                            ? Maybe.Just(state.Props[state.SelectedProp.Value])
                            : Maybe.Nothing<EditorProp>();
                        return prop;
                    })
                .DistinctUntilChanged();

            var moveProp =
                currentselectedProp.CombineLatest(combinedTransformation2, (selectedProp, transformationCommand) => {
                    if (selectedProp.IsJust) {
                        return transformationCommand.Scan(
                            new Tuple<PropId, ImmutableTransform>(selectedProp.Value.Id, selectedProp.Value.Transform),
                            (accumulatedMovement, transformation) => {
                                var transformUpdate = accumulatedMovement._2
                                    .Rotate(transformation.Rotation)
                                    .Translate(transformation.Position, cameraTransform.V.Rotation)
                                    .Scale(transformation.Scale);
                                return new Tuple<PropId, ImmutableTransform>(accumulatedMovement._1, transformUpdate);
                            });
                    }
                    else {
                        return Observable.Empty<Tuple<PropId, ImmutableTransform>>();
                    }
                });

            moveProp.Switch()
                .Subscribe(gameObjectMoveCommand => {
                    var propId = gameObjectMoveCommand._1;
                    var newTransform = gameObjectMoveCommand._2;
                    var go = propRenderer.GetProp(propId);
                    go.SetTransform(newTransform);
                });
            moveProp
                .Select(moveCommand => moveCommand.TakeLast(1))
                .Switch()
                .Subscribe(moveCommand => actions.UpdateProp.OnNext(moveCommand));

            var moveToNextProp = UnityObservable.CreateUpdate<string>(observer => {
                if (UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.Tab)) {
                    observer.OnNext("previous");
                }
                else if (UnityEngine.Input.GetKeyDown(KeyCode.Tab)) {
                    observer.OnNext("next");
                }
            });

            courseUpdates
                .Select(state => {
                    if (state.PropOrder.Count > 0) {
                        return moveToNextProp.Select(command => {
                            if (state.SelectedProp.IsJust) {
                                if (command.Equals("next")) {
                                    return state.PropOrder.GetNext(state.SelectedProp.Value);    
                                } else if (command.Equals("previous")) {
                                    return state.PropOrder.GetPrevious(state.SelectedProp.Value);
                                }
                            }
                            return state.PropOrder.First();
                        });
                    }
                    return Observable.Empty<PropId>();
                })
                .Switch()
                .Subscribe(newlySelectedProp => {
                    actions.SelectProp.OnNext(Maybe.Just(newlySelectedProp));
                    actions.MoveToProp.OnNext(newlySelectedProp);
                });


            // TODO When player presses 'V' move prop to camera perspective.


            createProp.Subscribe(_ => actions.CreateProp.OnNext(Unit.Default));
            deleteProp.Subscribe(_ => actions.DeleteSelectedProp.OnNext(Unit.Default));
            undo.Subscribe(_ => actions.Undo.OnNext(Unit.Default));
            redo.Subscribe(_ => actions.Redo.OnNext(Unit.Default));
            highlight.Subscribe(propId => actions.HighlightProp.OnNext(propId));
            selection.Subscribe(propId => actions.SelectProp.OnNext(Maybe.Just(propId)));
            switchTransformTool.Subscribe(tool => actions.SelectTransformTool.OnNext(tool));

            var moveToProp = courseUpdates.Select(state => {
                return actions.MoveToProp.Select(propId => state.Props[propId]);
            }).Switch();

            moveToProp.Subscribe(prop => {
                Vector3 cameraRotation = cameraTransform.V.Rotation.eulerAngles;
                var propRotation = prop.Transform.Rotation.eulerAngles;
                cameraTransform.V = prop.Transform
                    .TranslateLocally(new Vector3(0, 0, -30))
                    .UpdateRotation(cameraRotation.X(propRotation.x).Y(propRotation.y));
            });

            // TODO This code can be much simpler
            var guiUpdates = courseUpdates
                .Scan(new Diff<ObjectSelectionState?>(null, null), (previousDiff, state) => {
                    Func<Maybe<PropId>, Maybe<GameObject>> findGameObject = propId => {
                        return propId.IsJust
                            ? Maybe.Of(propRenderer.GetProp(propId.Value))
                            : Maybe.Nothing<GameObject>();
                    };

                    var @new = new ObjectSelectionState(findGameObject(state.HighlightedProp),
                        findGameObject(state.SelectedProp));
                    var old = previousDiff.New;
                    if (old.HasValue) {
                        if (old.Value.HighlightedObject.IsJust && old.Value.HighlightedObject.Value == null) {
                            old = new ObjectSelectionState(Maybe.Nothing<GameObject>(), old.Value.SelectedObject);
                        }
                        if (old.Value.SelectedObject.IsJust && old.Value.SelectedObject.Value == null) {
                            old = new ObjectSelectionState(old.Value.HighlightedObject, Maybe.Nothing<GameObject>());
                        }
                    }

                    return new Diff<ObjectSelectionState?>(old, @new);
                });
            guiUpdates.Subscribe(guiState => {
                if (guiState.Old.HasValue) {
                    guiState.Old.Value.HighlightedObject.Do(obj => obj.GetComponentOfInterface<IHighlightable>().UnHighlight());
                    guiState.Old.Value.SelectedObject.Do(obj => obj.GetComponentOfInterface<ISelectable>().UnSelect());
                }

                guiState.New.Value.HighlightedObject.Do(obj => obj.GetComponentOfInterface<IHighlightable>().Highlight());
                guiState.New.Value.SelectedObject.Do(obj => obj.GetComponentOfInterface<ISelectable>().Select());
            });
        }

    }
}