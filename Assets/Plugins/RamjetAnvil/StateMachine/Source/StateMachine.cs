using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using RamjetAnvil.Coroutine;

namespace RamjetAnvil.StateMachine {
    [AttributeUsage(AttributeTargets.Event)]
    public class StateEvent : Attribute {
        public string Name { get; private set; }

        public StateEvent(string name) {
            Name = name;
        }
    }

    public interface IStateMachine {
        IAwaitable Transition(StateId stateId, params object[] args);
        IAwaitable TransitionToParent(params object[] args);
        bool IsTransitioning { get; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T">The type of the object that owns this StateMachine</typeparam>
    public class StateMachine<T> : IStateMachine {
        private readonly T _owner;
        private readonly ICoroutineScheduler _scheduler;
        private readonly IDictionary<StateId, StateInstance> _states;
        private readonly IteratableStack<StateInstance> _stack;

        private readonly IDictionary<string, EventInfo> _ownerEvents;
        private bool _isTransitioning;

        public StateMachine(T owner, ICoroutineScheduler scheduler) {
            _owner = owner;
            _scheduler = scheduler;

            _states = new Dictionary<StateId, StateInstance>();
            _stack = new IteratableStack<StateInstance>();

            Type type = typeof (T);

            _ownerEvents = GetStateEvents(type);
            AssertStateMethodIntegrity(type, _ownerEvents);
        }

        public StateInstance AddState(StateId stateId, State state) {
            if (_states.ContainsKey(stateId)) {
                throw new ArgumentException(string.Format("StateId '{0}' is already registered.", stateId));
            }

            var instance = new StateInstance(stateId, state, GetImplementedStateMethods(state, _ownerEvents));
            _states.Add(stateId, instance);
            return instance;
        }

        public bool IsTransitioning {
            get { return _isTransitioning; }
        }

        public IAwaitable Transition(StateId stateId, params object[] args) {
            if (_isTransitioning) {
                throw new Exception("Cannot transition while another transition is already active.");
            }

            StateInstance newState = _states[stateId];

            if (_stack.Count == 0) {
                return _scheduler.Run(EnterNewState(newState, args));
            }
            else {
                StateInstance oldState = _stack.Peek();

                var isNormalTransition = oldState.Transitions.Contains(stateId);
                var isChildTransition = !isNormalTransition && oldState.ChildTransitions.Contains(stateId);

                if (isChildTransition) {
                    return _scheduler.Run(TransitionToChild(oldState, newState, args));
                } else if (isNormalTransition) {
                    return _scheduler.Run(Transition(oldState, newState, args));
                }
                else {
                    throw new Exception(string.Format(
                        "Transition from state '{0}' to state '{1}' is not registered, transition failed",
                        oldState.StateId,
                        stateId));
                }
            }
        }

        private IEnumerator<WaitCommand> Transition(StateInstance oldState, StateInstance newState, object[] enterParams) {
            yield return WaitCommand.WaitRoutine(ExitOldState(oldState));
            yield return WaitCommand.WaitRoutine(EnterNewState(newState, enterParams));
        }

        private IEnumerator<WaitCommand> ExitOldState(StateInstance oldState) {
            _isTransitioning = true;
            UnsubscribeToStateMethods(oldState);
            _stack.Pop();
            yield return WaitCommand.WaitRoutine(InvokeStateLifeCycleMethod(oldState.OnExit));
            _isTransitioning = false;
        }

        private IEnumerator<WaitCommand> EnterNewState(StateInstance newState, object[] enterParams) {
            _isTransitioning = true;
            yield return WaitCommand.WaitRoutine(InvokeStateLifeCycleMethod(newState.OnEnter, enterParams));
            _stack.Push(newState);
            SubscribeToStateMethods(newState);
            _isTransitioning = false;
        }

        private IEnumerator<WaitCommand> TransitionToChild(StateInstance parentState, StateInstance childState, object[] enterParams) {
            _isTransitioning = true;
            UnsubscribeToStateMethods(parentState);
            yield return WaitCommand.WaitRoutine(InvokeStateLifeCycleMethod(parentState.OnSuspend));
            yield return WaitCommand.WaitRoutine(InvokeStateLifeCycleMethod(childState.OnEnter, enterParams));
            _stack.Push(childState);
            SubscribeToStateMethods(childState);
            _isTransitioning = false;
        }

        private IEnumerator<WaitCommand> TransitionToParent(StateInstance childState, StateInstance parentState, object[] resumeParams) {
            _isTransitioning = true;
            UnsubscribeToStateMethods(childState);
            _stack.Pop();
            yield return WaitCommand.WaitRoutine(InvokeStateLifeCycleMethod(childState.OnExit));
            yield return WaitCommand.WaitRoutine(InvokeStateLifeCycleMethod(parentState.OnResume, resumeParams));
            SubscribeToStateMethods(parentState);
            _isTransitioning = false;
        }

        private IEnumerator<WaitCommand> InvokeStateLifeCycleMethod(Delegate del, params object[] args) {
            // TODO Use identity wait command
            WaitCommand waitCommand = WaitCommand.DontWait;
            if (del != null) {
                try {
                    if (del.Method.ReturnType == typeof(IEnumerator<WaitCommand>)) {
                        waitCommand = WaitCommand.WaitRoutine((IEnumerator<WaitCommand>)del.DynamicInvoke(args));
                    } else {
                        del.DynamicInvoke(args);
                    }
                } catch (TargetParameterCountException e) {
                    throw new ArgumentException(GetArgumentExceptionDetails((State)del.Target, del, args), e);
                } catch (TargetInvocationException e) {
                    UnityEngine.Debug.LogError("[StateMachine] " + (del.Target != null ? del.Target.GetType().FullName : "?") +
                        "." + del.Method.Name + " threw: " + e.InnerException);
                    throw;
                }
            }
            yield return waitCommand;
        }


        public IAwaitable TransitionToParent(params object[] args) {
            if (_isTransitioning) {
                throw new Exception("Cannot transition while another transition is already active.");
            }
            if (_stack.Count <= 1) {
                throw new InvalidOperationException("Cannot transition to parent state, currently at top-level state");
            }

            // Peek only - the private TransitionToParent coroutine below does the actual
            // _stack.Pop(). Popping here too was a double-pop: it silently emptied the
            // stack one transition early, so the *next* child transition (e.g. re-opening
            // the pause menu) took the wrong code path (fresh top-level entry instead of
            // suspend-parent-then-enter-child), skipping the parent's OnSuspend/unsubscribe
            // and leaving both states subscribed at once.
            var childState = _stack.Peek();
            var parentState = _stack[_stack.Count - 2];
            return _scheduler.Run(TransitionToParent(childState, parentState, args));
        }

        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static IDictionary<string, EventInfo> GetStateEvents(Type type) {
            var events = new Dictionary<string, EventInfo>();

            var attributeType = typeof (StateEvent);
            var allEvents = type.GetEvents(Flags);
            foreach (var e in allEvents) {
                var attributes = e.GetCustomAttributes(attributeType, false);
                if (attributes.Length > 0) {
                    var a = (StateEvent) attributes[0];
                    events.Add(a.Name, e);
                }
            }

            return events;
        }

        private static void AssertStateMethodIntegrity(Type type, IDictionary<string, EventInfo> ownerEvents) {
            foreach (var pair in ownerEvents) {
                var method = type.GetMethod(pair.Key, Flags);
                if (method == null) {
                    throw new StateMachineException(string.Format(
                        "Failed to find method matching event name '{0}' in '{1}'. Please implement method '{0}', and ensure it invokes '{2}'.",
                        pair.Key,
                        type,
                        pair.Value.Name));
                }
            }
        }

        private static IDictionary<string, Delegate> GetImplementedStateMethods(State state, IDictionary<string, EventInfo> ownerEvents) {
            var implementedMethods = new Dictionary<string, Delegate>();

            Type type = state.GetType();

            var stateMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var stateMethod in stateMethods) {
                foreach (var ownerEvent in ownerEvents) {
                    if (stateMethod.Name == ownerEvent.Key) {
                        var del = ReflectionUtils.ToDelegate(stateMethod, state);
                        implementedMethods.Add(ownerEvent.Key, del);
                    }
                }
            }

            return implementedMethods;
        }

        private void UnsubscribeToStateMethods(StateInstance state) {
            // Todo: is there an easier way to clear the list of subscribers?
            foreach (var pair in _ownerEvents) {
                // Unregister delegates of the old state
                if (state != null && state.StateDelegates.ContainsKey(pair.Key)) {
                    pair.Value.RemoveEventHandler(_owner, state.StateDelegates[pair.Key]);
                }
            }
        }

        // Todo: Separate into sub/unsub pair
        private void SubscribeToStateMethods(StateInstance state) {
            // Todo: is there an easier way to clear the list of subscribers?
            foreach (var pair in _ownerEvents) {
                // Register delegates of the new state
                if (state.StateDelegates.ContainsKey(pair.Key)) {
                    pair.Value.AddEventHandler(_owner, state.StateDelegates[pair.Key]);
                }
            }
        }

        private string GetArgumentExceptionDetails(State state, Delegate del, params object[] args) {
            var expectedArgs = del.Method.GetParameters();
            string expectedArgTypes = "";
            for (int i = 0; i < expectedArgs.Length; i++) {
                expectedArgTypes += expectedArgs[i].ParameterType.Name + (i < expectedArgs.Length - 1 ? ", " : "");
            }

            string receivedArgTypes = "";
            for (int i = 0; i < args.Length; i++) {
                receivedArgTypes += args[i].GetType().Name + (i < args.Length - 1 ? ", " : "");
            }
            return String.Format(
                "Wrong arguments for transition to state '{0}', expected: {1}; received: {2}",
                state.GetType(),
                expectedArgTypes,
                receivedArgTypes);
        }
    }

    public struct StateId {
        private readonly string _value;

        public StateId(string value) {
            _value = value;
        }

        public string Value {
            get { return _value; }
        }

        public bool Equals(StateId other) {
            return string.Equals(_value, other._value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is StateId && Equals((StateId) obj);
        }

        public override int GetHashCode() {
            return (_value != null ? _value.GetHashCode() : 0);
        }

        public static bool operator ==(StateId left, StateId right) {
            return left.Equals(right);
        }

        public static bool operator !=(StateId left, StateId right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return _value;
        }
    }

    /// <summary>
    /// A state, plus metadata, which lives in the machine's stack
    /// </summary>
    /// Todo: only expose Permit interface to user, not the list of delegates etc.
    public class StateInstance {
        private readonly State _state;
        private readonly Delegate _onEnter;
        private readonly Delegate _onExit;
        private readonly Delegate _onSuspend;
        private readonly Delegate _onResume;
        private readonly IDictionary<string, Delegate> _stateDelegates;

        public Delegate OnEnter {
            get { return _onEnter; }
        }

        public Delegate OnExit {
            get { return _onExit; }
        }

        public Delegate OnSuspend {
            get { return _onSuspend; }
        }

        public Delegate OnResume {
            get { return _onResume; }
        }

        public StateInstance(StateId stateId, State state, IDictionary<string, Delegate> stateDelegates) {
            StateId = stateId;
            _state = state;
            _stateDelegates = stateDelegates;

            Transitions = new List<StateId>();
            ChildTransitions = new List<StateId>();

            _onEnter = GetDelegateByName(State, "OnEnter");
            _onExit = GetDelegateByName(State, "OnExit");
            _onSuspend = GetDelegateByName(State, "OnSuspend");
            _onResume = GetDelegateByName(State, "OnResume");
        }

        public StateId StateId { get; private set; }

        public State State {
            get { return _state; }
        }

        public IDictionary<string, Delegate> StateDelegates {
            get { return _stateDelegates; }
        }

        public IList<StateId> Transitions { get; private set; }
        public IList<StateId> ChildTransitions { get; private set; }

        public StateInstance Permit(StateId stateId) {
            Transitions.Add(stateId);
            return this;
        }

        public StateInstance PermitChild(StateId stateId) {
            ChildTransitions.Add(stateId);
            return this;
        }

        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static Delegate GetDelegateByName(State state, string name) {
            Type type = state.GetType();

            var method = type.GetMethod(name, Flags);
            if (method != null) {
                UnityEngine.Debug.Log("Found Lifecycle Method: " + name);
                return ReflectionUtils.ToDelegate(method, state);
            }
            return null;
        }
    }

    public class State {
        protected IStateMachine Machine { get; private set; }

        public State(IStateMachine machine) {
            Machine = machine;
        }
    }

    public class StateMachineException : Exception {
        public StateMachineException(string message) : base(message) {}
    }
}
