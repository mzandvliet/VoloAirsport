using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Volo.CourseEditing
{
    public abstract class UpdateAppState<T> {

        public sealed class Undo : UpdateAppState<T> {
            public static readonly Undo Instance = new Undo();
            private Undo() { }
        }

        public sealed class Redo : UpdateAppState<T> {
            public static readonly Redo Instance = new Redo();
            private Redo() { }
        }

        public sealed class AddNew : UpdateAppState<T> {
            private readonly T _value;

            public AddNew(T value) {
                _value = value;
            }

            public T Value {
                get { return _value; }
            }
        }
    }

    public class AppState<T> {
        private readonly T _currentState;
        private readonly IImmutableStack<T> _undoStack;
        private readonly IImmutableStack<T> _redoStack;

        public AppState(T currentState) : this(currentState, ImmutableStack<T>.Empty, ImmutableStack<T>.Empty) {}

        public AppState(T currentState, IImmutableStack<T> undoStack, IImmutableStack<T> redoStack) {
            _currentState = currentState;
            _undoStack = undoStack;
            _redoStack = redoStack;
        }

        public T CurrentState {
            get { return _currentState; }
        }

        public IImmutableStack<T> UndoStack {
            get { return _undoStack; }
        }

        public IImmutableStack<T> RedoStack {
            get { return _redoStack; }
        }

        public override string ToString() {
            return string.Format("CurrentState: {0}, UndoStack: {1}, RedoStack: {2}", _currentState, _undoStack, _redoStack);
        }
    }

    public static class History
    {
        public static AppState<T> Undo<T>(AppState<T> appState) {
            if (!appState.UndoStack.IsEmpty) {
                T newCurrentState;
                var newUndoStack = appState.UndoStack.Pop(out newCurrentState);
                var newRedoStack = appState.RedoStack.Push(appState.CurrentState);
                return new AppState<T>(newCurrentState, newUndoStack, newRedoStack);
            }
            return appState;
        }

        public static AppState<T> Redo<T>(AppState<T> appState) {
            if (!appState.RedoStack.IsEmpty) {
                T newCurrentState;
                var newRedoStack = appState.RedoStack.Pop(out newCurrentState);
                var newUndoStack = appState.UndoStack.Push(appState.CurrentState);
                return new AppState<T>(newCurrentState, newUndoStack, newRedoStack);
            }
            return appState;
        }

        public static AppState<T> AddNewState<T>(AppState<T> appState, T newState) {
            if (!newState.Equals(appState.CurrentState)) {
                var undoStack = appState.UndoStack.Push(appState.CurrentState);
                return new AppState<T>(newState, undoStack, redoStack: ImmutableStack<T>.Empty);    
            }
            return appState;
        }

        public static IObservable<T> AddHistoryManipulation<T>(T initialState, IObservable<T> newStates, 
                IObservable<Unit> undo, IObservable<Unit> redo) {
            return newStates
                .Select(value => new UpdateAppState<T>.AddNew(value) as UpdateAppState<T>)
                .Merge(undo.Select(x => UpdateAppState<T>.Undo.Instance as UpdateAppState<T>))
                .Merge(redo.Select(x => UpdateAppState<T>.Redo.Instance as UpdateAppState<T>))
                .Scan(new AppState<T>(initialState), (appState, operation) => {
                    // switch over options: add new state, undo, redo
                    if (operation is UpdateAppState<T>.Undo) {
                        return Undo(appState);
                    } else if (operation is UpdateAppState<T>.Redo) {
                        return Redo(appState);
                    } else {
                        return AddNewState(appState, (operation as UpdateAppState<T>.AddNew).Value);
                    }
                })
                .Select(appState => appState.CurrentState);
        }
    }
}
