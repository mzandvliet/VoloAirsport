using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RamjetAnvil.Coroutine {
    public interface ICoroutineScheduler {
        IAwaitable Run(IEnumerator<WaitCommand> fibre,
            [CallerMemberName] string callerMember = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0);
    }

    public class CoroutineScheduler : ICoroutineScheduler {
        private readonly RoutinePool<Routine> _routinePool;
        private readonly IList<Routine> _routines;

        private long _prevFrame;
        private double _prevTime;

        public CoroutineScheduler(int initialCapacity = 10, int growthStep = 10) {
            _routinePool = new RoutinePool<Routine>(factory: () => new Routine(CreateRoutine, RecycleRoutine), growthStep: growthStep);
            _routines = new List<Routine>(capacity: initialCapacity);
            _prevFrame = -1;
            _prevTime = 0f;
        }

        public IAwaitable Run(IEnumerator<WaitCommand> fibre,
            [CallerMemberName] string callerMember = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) {
            return RunInternal(fibre, callerMember, callerFile, callerLine);
        }

        private IAwaitable RunInternal(IEnumerator<WaitCommand> fibre, string callerMember, string callerFile, int callerLine) {
            var coroutine = CreateRoutine(fibre, callerMember, callerFile, callerLine);
            _routines.Add(coroutine);
            // Hand out a generation-stamped handle, not the pooled Routine directly - once
            // this coroutine finishes, the scheduler recycles and reuses the same object for
            // something else (routinely, within a frame or two - CoroutineScheduler.Run()
            // even pumps Update() as its first step). Anything holding an IAwaitable across
            // more than one frame (e.g. WaitUntilDone()) needs IsDone to reliably mean "the
            // thing I was told to wait for is done", not "whatever this object currently
            // represents is done".
            return new RoutineHandle(coroutine, coroutine.Generation);
        }

        // Used as the subroutine factory (see the RoutinePool factory above) - a routine
        // spawned from within another coroutine's yield has no meaningful "caller site" of
        // its own, so it falls back to identifying itself by the fibre's own type name.
        private Routine CreateRoutine(IEnumerator<WaitCommand> fibre) {
            return CreateRoutine(fibre, callerMember: null, callerFile: null, callerLine: 0);
        }

        private Routine CreateRoutine(IEnumerator<WaitCommand> fibre, string callerMember, string callerFile, int callerLine) {
            if (fibre == null) {
                throw new Exception("Routine cannot be null");
            }

            var coroutine = _routinePool.Take();
            coroutine.Initialize(fibre, callerMember, callerFile, callerLine);
            return coroutine;
        }

        private void RecycleRoutine(Routine r) {
            _routinePool.Return(r);
        }

        public void Update(long currentFrame, double currentTime) {
            var timePassed = new Duration(
                frameCount: (int) (currentFrame - _prevFrame),
                seconds: (float) (currentTime - _prevTime));

            for (int i = _routines.Count - 1; i >= 0; i--) {
                var routine = _routines[i];

                if (routine.IsDone) {
                    _routines.RemoveAt(i);
                    RecycleRoutine(routine);
                } else {
                    routine.Update(timePassed);
                }
            }

            _prevFrame = currentFrame;
            _prevTime = currentTime;
        }
    }

    public struct Duration : IEquatable<Duration> {
        public readonly float Seconds;
        public readonly int FrameCount;

        public Duration(float seconds = 0f, int frameCount = 0) {
            Seconds = seconds;
            FrameCount = frameCount;
        }

        public static Duration operator -(Duration t1, Duration t2) {
            return new Duration(
                frameCount: Math.Max(t1.FrameCount - t2.FrameCount, 0),
                seconds: Math.Max(t1.Seconds - t2.Seconds, 0f));
        }

        public static Duration operator +(Duration t1, Duration t2) {
            return new Duration(
                frameCount: t1.FrameCount + t2.FrameCount,
                seconds: t1.Seconds + t2.Seconds);
        }

        public static Duration Min(Duration t1, Duration t2) {
            return new Duration(
                seconds: Math.Min(t1.Seconds, t2.Seconds),
                frameCount: Math.Min(t1.FrameCount, t2.FrameCount));
        }

        public static Duration Max(Duration t1, Duration t2) {
            return new Duration(
                seconds: Math.Max(t1.Seconds, t2.Seconds),
                frameCount: Math.Max(t1.FrameCount, t2.FrameCount));
        }

        public bool Equals(Duration other) {
            return Seconds.Equals(other.Seconds) && FrameCount == other.FrameCount;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Duration && Equals((Duration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (Seconds.GetHashCode() * 397) ^ FrameCount;
            }
        }

        public static bool operator ==(Duration left, Duration right) {
            return left.Equals(right);
        }

        public static bool operator !=(Duration left, Duration right) {
            return !left.Equals(right);
        }

        public bool IsTimeLeft {
            get { return Seconds > 0f || FrameCount > 0; }
        }

        public bool IsTimeUp {
            get { return !IsTimeLeft; }
        }

        public override string ToString() {
            return "(Seconds: " + Seconds + ", Frames: " + FrameCount + ")";
        }
    }

    public struct WaitCommand {
        private static readonly IEnumerator<WaitCommand>[] EmptyRoutines = {};

        public readonly Duration? Duration;
        // TODO Optimize for single routine wait commands
        public readonly IEnumerator<WaitCommand>[] Routines;

        private WaitCommand(Duration duration) {
            Duration = duration;
            Routines = EmptyRoutines;
        }

        private WaitCommand(IEnumerator<WaitCommand>[] routines) {
            Duration = null;
            Routines = routines;
        }

        public static WaitCommand Wait(TimeSpan duration) {
            return new WaitCommand(new Duration(seconds: (float) duration.TotalSeconds));
        }

        public static WaitCommand WaitSeconds(float seconds) {
            return new WaitCommand(new Duration(seconds: seconds));
        }

        public static WaitCommand WaitFrames(int frameCount) {
            return new WaitCommand(new Duration(frameCount: frameCount));
        }

        public static WaitCommand WaitForNextFrame {
            get { return WaitFrames(1); }
        }

        public static WaitCommand DontWait {
            get { return new WaitCommand(new Duration(seconds: 0f, frameCount: 0)); }
        }

        public static WaitCommand WaitRoutine(IEnumerator<WaitCommand> routine) {
            return new WaitCommand(new[] { routine });
        }

        public static WaitCommand Interleave(params IEnumerator<WaitCommand>[] routines) {
            return new WaitCommand(routines);
        }

        public static WaitCommand operator -(WaitCommand command, Duration duration) {
            if (command.Duration.HasValue) {
                return new WaitCommand(command.Duration.Value - duration);
            }
            throw new ArgumentException("Cannot subtract time from a routine wait command");
        }

        public static WaitCommand operator +(WaitCommand command, Duration duration) {
            if (command.Duration.HasValue) {
                return new WaitCommand(command.Duration.Value + duration);
            }
            throw new ArgumentException("Cannot add time to a routine wait command");
        }

        public bool IsRoutine {
            get { return !Duration.HasValue; }
        }

        public bool IsFinished {
            get {
                if (Duration.HasValue) {
                    return Duration.Value.IsTimeUp;
                }
                return false;
            }
        }

        public IEnumerator<WaitCommand> AsRoutine {
            get {
                if (IsRoutine && Routines.Length == 1) {
                    return Routines[0];
                }
                return AsRoutineInternal();
            }
        }

        private IEnumerator<WaitCommand> AsRoutineInternal() {
            yield return this;
        }

        public override string ToString() {
            if (IsRoutine) {
                return "WaitCommand(Routine)";
            }
            return "WaitCommand(" + Duration + ")";
        }
    }

    public delegate bool Predicate();

    public static class WaitCommandExtensions {
        public static WaitCommand AsWaitCommand(this IEnumerator<WaitCommand> coroutine) {
            return WaitCommand.WaitRoutine(coroutine);
        }

        public static WaitCommand WaitUntilDone(this IAwaitable awaitable) {
            return WaitUntilDoneInternal(awaitable).AsWaitCommand();
        }
        private static IEnumerator<WaitCommand> WaitUntilDoneInternal(IAwaitable awaitable) {
            while(!awaitable.IsDone) {
                yield return WaitCommand.WaitForNextFrame;
            }
        }

        public static IEnumerator<WaitCommand> WaitUntil(this WaitCommand waitCommand, Predicate predicate) {
            return WaitUntil(waitCommand.AsRoutine, predicate);
        }

        public static IEnumerator<WaitCommand> WaitUntil(this IEnumerator<WaitCommand> routine, Predicate predicate) {
            while(!predicate()) {
                yield return WaitCommand.WaitForNextFrame;
            }
            yield return routine.AsWaitCommand();
        }

        public static IEnumerator<WaitCommand> WaitWhile(this WaitCommand waitCommand, Predicate predicate) {
            return WaitWhile(waitCommand.AsRoutine, predicate);
        }

        public static IEnumerator<WaitCommand> WaitWhile(this IEnumerator<WaitCommand> routine, Predicate predicate) {
            while(predicate()) {
                yield return WaitCommand.WaitForNextFrame;
            }
            yield return routine.AsWaitCommand();
        }

        public static IEnumerator<WaitCommand> RunWhile(this WaitCommand waitCommand, Predicate predicate) {
            return RunWhile(waitCommand.AsRoutine, predicate);
        }

        public static IEnumerator<WaitCommand> RunWhile(this IEnumerator<WaitCommand> routine, Predicate predicate) {
            while (routine.MoveNext() && predicate()) {
                // Recursively skip subroutines
                var currentInstruction = routine.Current;
                if (currentInstruction.IsRoutine) {
                    var instructionRoutines = routine.Current.Routines;

                    var wrappedRoutines = new IEnumerator<WaitCommand>[instructionRoutines.Length];
                    for (int i = 0; i < instructionRoutines.Length; i++) {
                        wrappedRoutines[i] = RunWhile(instructionRoutines[i], predicate);
                    }

                    yield return WaitCommand.Interleave(wrappedRoutines);
                } else {
                    yield return currentInstruction;
                }
            }
        }

        public static IEnumerator<WaitCommand> RunUntil(this IEnumerator<WaitCommand> routine, Predicate predicate) {
            return RunWhile(routine, () => !predicate());
        }

        public static IEnumerator<WaitCommand> RunUntil(this WaitCommand waitCommand, Predicate predicate) {
            return RunUntil(waitCommand.AsRoutine, predicate);
        }

        public static IEnumerator<WaitCommand> AndThen(this IEnumerator<WaitCommand> first, WaitCommand second) {
            return AndThen(first.AsWaitCommand(), second);
        }

        public static IEnumerator<WaitCommand> AndThen(this IEnumerator<WaitCommand> first, IEnumerator<WaitCommand> second) {
            return AndThen(first.AsWaitCommand(), second.AsWaitCommand());
        }

        public static IEnumerator<WaitCommand> AndThen(this WaitCommand first, IEnumerator<WaitCommand> second) {
            return AndThen(first, second.AsWaitCommand());
        }

        public static IEnumerator<WaitCommand> AndThen(this WaitCommand first, WaitCommand second) {
            yield return first;
            yield return second;
        }

        public static void Skip(this IEnumerator<WaitCommand> routine) {
            while (routine.MoveNext()) {
                // Recursively skip subroutines
                var instructionRoutines = routine.Current.Routines;
                for (int i = 0; i < instructionRoutines.Length; i++) {
                    instructionRoutines[i].Skip();
                }
            }
        }

        // TODO Find a proper

        public static IEnumerator<WaitCommand> Visit(this IEnumerator<WaitCommand> routine, Action<WaitCommand> visit) {
            while (routine.MoveNext()) {
                // Recursively skip subroutines
                var currentInstruction = routine.Current;
                if (currentInstruction.IsRoutine) {
                    var instructionRoutines = routine.Current.Routines;

                    var wrappedRoutines = new IEnumerator<WaitCommand>[instructionRoutines.Length];
                    for (int i = 0; i < instructionRoutines.Length; i++) {
                        wrappedRoutines[i] = Visit(instructionRoutines[i], visit);
                    }

                    yield return WaitCommand.Interleave(wrappedRoutines);
                } else {
                    visit(currentInstruction);
                    yield return currentInstruction;
                }
            }
        }
    }

    public class AsyncResult<T> {
        private T _result;
        private bool _isResultAvailable;

        public void SetResult(T result) {
            _result = result;
            _isResultAvailable = true;
        }

        public T Result {
            get { return _result; }
        }

        public bool IsResultAvailable {
            get { return _isResultAvailable; }
        }

        public static AsyncResult<T> FromCallback(Action<Action<T>> invoke) {
            var asyncResult = new AsyncResult<T>();
            invoke(asyncResult.SetResult);
            return asyncResult;
        }

        public static EventResult SingleResultFromEvent(Event @event, Func<T, bool> predicate = null) {
            var asyncResult = new AsyncResult<T>();
            var awaitResult = Wait(asyncResult, @event, predicate);
            awaitResult.MoveNext();
            return new EventResult(asyncResult, awaitResult.AsWaitCommand());
        }

        private static IEnumerator<WaitCommand> Wait(AsyncResult<T> asyncResult,
            Event @event,
            Func<T, bool> predicate) {

            Action<T> setResult = @obj => {
                if (predicate == null || predicate(obj)) {
                    asyncResult.SetResult(obj);
                }
            };
            @event.AddHandler(setResult);
            while (!asyncResult.IsResultAvailable) {
                yield return WaitCommand.WaitForNextFrame;
            }
            @event.RemoveHandler(setResult);
        }

        public class EventResult {
            private readonly AsyncResult<T> _asyncResult;
            public readonly WaitCommand WaitUntilReady;

            public EventResult(AsyncResult<T> asyncResult, WaitCommand waitUntilReady) {
                _asyncResult = asyncResult;
                WaitUntilReady = waitUntilReady;
            }

            public T Result {
                get { return _asyncResult.Result; }
            }
        }
    }

    public class Routine : IResetable, IAwaitable {

        private readonly Func<IEnumerator<WaitCommand>, Routine> _createSubroutine;
        private readonly Action<Routine> _disposeRoutine;

        private IEnumerator<WaitCommand> _fibre;

        private readonly IList<Routine> _activeSubroutines;
        private WaitCommand _activeWaitCommand;
        private bool _isDone;

        // Debugging aid only - identifies what this routine was running, for error logging.
        // Deliberately NOT cleared by Reset(): if something goes wrong with a routine that's
        // already been recycled (the null-fibre/stale-handle class of bug), the name of
        // whatever it was running *last* is still far more useful than nothing.
        private string _debugName;

        // Bumped every Initialize() - lets a RoutineHandle (see below) tell whether the
        // pooled instance it was handed out for is still the same logical coroutine, or has
        // since been recycled and reused for something else entirely.
        private int _generation;

        public Routine(Func<IEnumerator<WaitCommand>, Routine> createSubroutine, Action<Routine> disposeRoutine) {
            _createSubroutine = createSubroutine;
            _disposeRoutine = disposeRoutine;
            _activeSubroutines = new List<Routine>();
        }

        public int Generation {
            get { return _generation; }
        }

        public void Initialize(IEnumerator<WaitCommand> fibre, string callerMember = null, string callerFile = null, int callerLine = 0) {
            _fibre = fibre;
            _activeSubroutines.Clear();
            _activeWaitCommand = WaitCommand.DontWait;
            _isDone = false;
            _generation++;
            _debugName = FormatDebugName(fibre, callerMember, callerFile, callerLine);
            Update(new Duration(seconds: 0f, frameCount: 0));
        }

        private static string FormatDebugName(IEnumerator<WaitCommand> fibre, string callerMember, string callerFile, int callerLine) {
            var fibreName = fibre.GetType().FullName;
            if (string.IsNullOrEmpty(callerFile)) {
                // Spawned as a subroutine of another coroutine's yield, not a direct Run()
                // call - no meaningful call site of its own.
                return fibreName;
            }
            var shortFile = System.IO.Path.GetFileName(callerFile);
            return fibreName + " (Run() called from " + shortFile + ":" + callerLine + " in " + callerMember + ")";
        }

        public Duration Update(Duration timePassed) {
            // Find a new instruction and make it the current one
            if (IsRunningInstructionFinished) {
                FetchNextInstruction();
            }

            Duration leftOverTime = timePassed;
            // Update the current instruction
            if (!_isDone) {
                if (_activeSubroutines.Count > 0) {
                    for (int i = _activeSubroutines.Count - 1; i >= 0; i--) {
                        var subroutine = _activeSubroutines[i];
                        var subroutineTimeLeft = subroutine.Update(timePassed);
                        if (subroutine.IsDone) {
                            _activeSubroutines.RemoveAt(i);
                            subroutine.RecycleTree();
                        }
                        leftOverTime = Duration.Min(leftOverTime, subroutineTimeLeft);
                    }
                } else {
                    leftOverTime = timePassed - _activeWaitCommand.Duration.Value;
                    _activeWaitCommand = _activeWaitCommand - timePassed;
                }

                // TODO Check if we're stuck on a subroutine, or a wait command
                //      if not continue
                if (_activeWaitCommand.IsFinished && _activeSubroutines.Count == 0) {
                    leftOverTime = Update(leftOverTime);
                }
            }
            return leftOverTime;
        }

        private void FetchNextInstruction() {
            // Push/Pop (sub-)coroutines until we get another instruction or we run out of instructions.
            while(!_isDone && IsRunningInstructionFinished) {
                bool hasNext;
                try {
                    hasNext = _fibre.MoveNext();
                } catch (Exception e) {
                    UnityEngine.Debug.LogError("[CoroutineScheduler] Exception while advancing routine '" +
                        _debugName + "' (fibre " + (_fibre != null ? "present" : "null") + "): " + e);
                    // Mark this routine done so it gets recycled normally instead of being
                    // retried (and re-erroring) every frame - rethrowing here would abort the
                    // scheduler's per-frame update pass mid-loop, which can leave sibling/parent
                    // routines stuck without ever being cleaned up.
                    _isDone = true;
                    return;
                }
                if (hasNext) {
                    var newInstruction = _fibre.Current;
                    if (newInstruction.IsRoutine) {
                        for (int i = 0; i < newInstruction.Routines.Length; i++) {
                            var subroutine = newInstruction.Routines[i];
                            var startedSubroutine = _createSubroutine(subroutine);
                            _activeSubroutines.Add(startedSubroutine);
                        }

                        _activeWaitCommand = WaitCommand.DontWait;
                    } else {
                        _activeWaitCommand = newInstruction;
                    }
                } else {
                    _isDone = true;
                }
            }
        }

        private bool IsRunningInstructionFinished {
            get { return _activeSubroutines.Count == 0 && _activeWaitCommand.IsFinished; }
        }

        public bool IsDone { get { return _isDone; } }

        public void Reset() {
            _fibre = null;
            // Bump here, not just in Initialize(): Reset() clears _isDone back to false, which
            // would otherwise *un-signal* completion for anyone still holding a handle to the
            // finished coroutine. The scheduler recycles a routine one Update() pass after it
            // finishes, so a waiter polling on a *different* scheduler instance can easily
            // miss the brief window where _isDone was true and then wait forever. Bumping the
            // generation at recycle time makes "already recycled" permanently indistinguishable
            // from "done" to any older handle, which is exactly what a waiter needs.
            _generation++;
            _isDone = false;
            for (int i = 0; i < _activeSubroutines.Count; i++) {
                _activeSubroutines[i].RecycleTree();
            }
            _activeSubroutines.Clear();
            _activeWaitCommand = WaitCommand.DontWait;
        }

        // Recursively tears this routine and all its descendants down and returns them to
        // the pool. Only safe to call once nothing outside this subtree references the
        // routine being recycled any more - i.e. from whichever container (a parent's
        // _activeSubroutines list, or the scheduler's own top-level pass via Reset()) just
        // removed its own reference to it.
        private void RecycleTree() {
            for (int i = 0; i < _activeSubroutines.Count; i++) {
                _activeSubroutines[i].RecycleTree();
            }
            _activeSubroutines.Clear();
            _fibre = null;
            _disposeRoutine(this);
        }

        // IDisposable - the public cancellation contract, safe to call from anywhere,
        // including external game code holding an IAwaitable handle from
        // CoroutineScheduler.Run(), at any time (even after the routine has already
        // finished and its handle is stale). Only marks the routine finished; does NOT
        // touch the pool. Whatever container is tracking this instance (top-level
        // _routines, or a parent's _activeSubroutines) will notice IsDone on its next
        // Update() pass and actually recycle it via RecycleTree()/Reset() - actually
        // freeing it here instead would let the pool hand the same instance out again
        // while that container still held a reference to it, silently corrupting
        // whatever new coroutine ends up reusing it.
        public void Dispose() {
            _isDone = true;
        }
    }

    public interface IAwaitable : IDisposable {
        bool IsDone { get; }
    }

    // Generation-stamped wrapper handed out by CoroutineScheduler.Run() instead of the
    // pooled Routine directly - see the comment in RunInternal(). Once the Routine's
    // generation has moved past the one this handle was created for, the original coroutine
    // is unambiguously finished (Initialize() only bumps the generation once a routine has
    // already been fully recycled), so IsDone reports true rather than reflecting whatever
    // unrelated coroutine now occupies the same pooled instance.
    internal class RoutineHandle : IAwaitable {
        private readonly Routine _routine;
        private readonly int _generation;

        public RoutineHandle(Routine routine, int generation) {
            _routine = routine;
            _generation = generation;
        }

        public bool IsDone {
            get { return _routine.Generation != _generation || _routine.IsDone; }
        }

        public void Dispose() {
            if (_routine.Generation == _generation) {
                _routine.Dispose();
            }
            // Else: already recycled and reused for something else - disposing would cancel
            // the new occupant's coroutine, not the one this handle was originally for.
        }
    }
}
