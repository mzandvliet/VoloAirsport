using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.CourseEditing;
using RamjetAnvil.Volo.States;
using UnityEngine;

namespace RamjetAnvil.Volo.UI
{
    public static class UISketch {

        public static class MouseHighlight {

            /// <summary>
            /// </summary>
            /// <param name="cursor">
            /// <code>
            /// {"items": GameObject list,
            ///  "isInteractable": bool}
            /// </code>
            /// </param>
            /// <param name="indexCursor"><code>int?</code></param>
            public static IDisposable Initialize(ITypedDataCursor<UIListState> cursor) {
                var isInteractable = cursor.To(s => s.IsInteractable);
                var highlightedIndex = cursor.To(s => s.HighlightedIndex);
                return AddMouseHighlight(
                    updateHighlightedIndex: index => {
                        if (isInteractable.Get() && index.HasValue) {
                            highlightedIndex.Set(index.Value);
                        }
                    }, 
                    items: cursor.To(s => s.Items).Get());
            }

            private static IDisposable AddMouseHighlight(Action<int?> updateHighlightedIndex, IList<GameObject> items) {
                var disposeListeners = new CompositeDisposable();
                for (int i = 0; i < items.Count; i++) {
                    var itemIndex = i;
                    var item = items[i];
                    var mouseEvents = item.GetComponent<UIEventSource>();

                    Action highlight = () => updateHighlightedIndex(itemIndex);
                    mouseEvents.OnCursorEnter += highlight;
                    disposeListeners.Add(Disposables.Create(() => mouseEvents.OnCursorEnter -= highlight));
                }
                return disposeListeners;
            }
        }

        /// <summary>
        /// Highlights a list of game objects based on two highlighted indices:
        /// mouseIndex and controllerIndex
        /// </summary>
        public static class Highlighter {
            
            // TODO Would be even better if the highlighter doesn't know anything
            // about the different types of highlighted indices

            /// <summary>
            /// </summary>
            /// <param name="cursor">
            /// <code>
            /// {"items": GameObject list,
            ///  "highlightedIndex": int}
            /// </code>
            /// </param>
            public static IDisposable Initialize(ITypedDataCursor<UIListState> cursor) {
                return cursor.OnUpdate.Subscribe(_ => Update(cursor));
            }

            private static void Update(ITypedDataCursor<UIListState> cursor) {
                var items = cursor.To(s => s.Items).Get();
                for (int i = 0; i < items.Count; i++) {
                    var item = items[i];
                    var highlightable = item.GetComponentOfInterface<IHighlightable>();
                    highlightable.UnHighlight();
                }

                var highlightedIndex = cursor.To(s => s.HighlightedIndex).Get();
                items[highlightedIndex].GetComponentOfInterface<IHighlightable>().Highlight();    
            }
        }

        public static class ControllerHighlight {

            /// <summary>
            /// </summary>
            /// <param name="cursor">
            /// <code>
            /// {"items": GameObject list,
            ///  "isInteractable": bool}
            /// </code>
            /// </param>
            /// <param name="indexCursor"><code>int?</code></param>
            public static Action<Vector2> CreateControllerIndexUpdater(ITypedDataCursor<UIListState> cursor,
                Func<IList<GameObject>, int, Vector2, int> itemSelector) {
                // Everytime there is new keyboard input, feed it
                // the currently highlighted controller index
                var items = cursor.To(s => s.Items);
                var isInteractable = cursor.To(s => s.IsInteractable);
                var highlightedIndex = cursor.To(s => s.HighlightedIndex);
                return keyboardInput => {
                    // Magnitude check for performance reasons
                    if (keyboardInput.magnitude > 0 && isInteractable.Get()) {
                        var currentIndex = highlightedIndex.Get();
                        highlightedIndex.Set(itemSelector(items.Get(), currentIndex, keyboardInput));
                    }
                };
            }

            public static int MoveControllerIndex(Func<Vector3, Vector3> world2ScreenPoint, IList<GameObject> availableItems,
                int currentItemIndex, Vector2 inputDirection) {
                if (inputDirection == Vector2.zero) {
                    return currentItemIndex;
                }

                var currentItem = availableItems[currentItemIndex];
                inputDirection = inputDirection.normalized;
                var currentPoint = RelativeScreenPoint(world2ScreenPoint(currentItem.transform.position));

                int? selectedItemIndex = null;
                float selectedItemCloseness = 0;
                for (int i = 0; i < availableItems.Count; i++) {
                    var item = availableItems[i];
                    if (i != currentItemIndex && item.activeInHierarchy) {
                        var itemPoint = RelativeScreenPoint(world2ScreenPoint(item.transform.position));

                        var itemDistance = Vector3.Magnitude(itemPoint - currentPoint);
                        var itemDirection = (itemPoint - currentPoint).normalized;
                        var angle = Vector2.Angle(inputDirection, itemDirection);
                        var isCloseEnough = angle <= 90f;
                        var closeness = itemDistance * 4.2f + (angle / 90f);

                        GameObject newSelectedItem = null;
                        if (isCloseEnough) {
                            if (!selectedItemIndex.HasValue) {
                                newSelectedItem = item;
                            } else if (closeness < selectedItemCloseness) {
                                newSelectedItem = item;
                            }
                        }
                        
                        if (newSelectedItem != null) {
                            selectedItemIndex = i;
                            selectedItemCloseness = closeness;
                        }
                    }
                }

                return selectedItemIndex ?? currentItemIndex;
            }
        }

        public static Vector2 RelativeScreenPoint(Vector3 screenPoint) {
            return new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
        }

        public static class MouseInteraction {

            public static IDisposable LeftMouseClick(IList<GameObject> items, Action<int> selectItem) {
                var disposeListeners = new CompositeDisposable();
                for (int i = 0; i < items.Count; i++) {
                    var itemIndex = i;
                    var item = items[i];
                    var mouseEvents = item.GetComponent<UIEventSource>();
                    Action select = () => selectItem(itemIndex);
                    mouseEvents.OnCursorClick += select;
                    disposeListeners.Add(Disposables.Create(() => mouseEvents.OnCursorEnter -= select));
                }
                return disposeListeners;
            }
        }
        
        public class NavigableUIList {

            // TODO Make mouse controls and controller controls optional

            private readonly ITypedDataCursor<bool> _isInteractable;
            private readonly ITypedDataCursor<int> _highlightedIndex;
            private readonly Action<Vector2> _controllerHighlight;
            private readonly Action<int> _selectItem;

            public NavigableUIList(ITypedDataCursor<UIListState> state, Camera camera, Action<int> selectItem) {
                _isInteractable = state.To(s => s.IsInteractable);
                _selectItem = selectItem;

                _highlightedIndex = state.To(s => s.HighlightedIndex);
                MouseHighlight.Initialize(state);
                MouseInteraction.LeftMouseClick(state.To(s => s.Items).Get(), selectItem);

                _controllerHighlight = ControllerHighlight.CreateControllerIndexUpdater(
                    cursor: state, 
                    itemSelector: (items, currentIndex, inputDirection) => {
                        return ControllerHighlight.MoveControllerIndex(camera.WorldToScreenPoint, items, currentIndex, inputDirection);
                    });

                Highlighter.Initialize(state);
            }

            public void Suspend() {
                _isInteractable.Set(false);
            }

            public void Resume() {
                _isInteractable.Set(true);
            }

            public void Update(Input menuInput) {
                _controllerHighlight(menuInput.Cursor);
                if (menuInput.Confirm) {
                    var index = _highlightedIndex.Get();
                    _selectItem(index);
                }
            }

            public struct Input {
                public Vector2 Cursor;
                public bool Confirm;
            }
        }
    }
}
