using System;
using System.Collections.Generic;
using NUnit.Framework;
using RamjetAnvil.Coroutine;
using RamjetAnvil.StateMachine;

namespace RamjetAnvil.StateMachine.Tests {

    // Tier 1/2: behaviour documented by StateMachine's public API (State/StateInstance
    // lifecycle methods, Permit/PermitChild, [StateEvent] routing) and exercised by real
    // usage in Assets/Scripts/AttractScreen (VoloStateMachine, Playing.cs).
    [TestFixture]
    public class StateMachineDocumentedBehaviourTests {

        [Test]
        public void Transition_ToRootState_InvokesOnEnter() {
            var scheduler = new CoroutineScheduler();
            var owner = new TestOwner();
            var machine = new StateMachine<TestOwner>(owner, scheduler);
            var log = new List<string>();
            var aId = new StateId("A");
            machine.AddState(aId, new RecordingState(machine, "A", log));

            var awaitable = machine.Transition(aId);

            Assert.IsTrue(awaitable.IsDone);
            CollectionAssert.AreEqual(new[] { "A.OnEnter" }, log);
        }

        [Test]
        public void Transition_BetweenSiblings_InvokesExitThenEnterInOrder() {
            var scheduler = new CoroutineScheduler();
            var owner = new TestOwner();
            var machine = new StateMachine<TestOwner>(owner, scheduler);
            var log = new List<string>();
            var aId = new StateId("A");
            var bId = new StateId("B");
            var a = machine.AddState(aId, new RecordingState(machine, "A", log));
            machine.AddState(bId, new RecordingState(machine, "B", log));
            a.Permit(bId);

            machine.Transition(aId);
            log.Clear();
            machine.Transition(bId);

            CollectionAssert.AreEqual(new[] { "A.OnExit", "B.OnEnter" }, log);
        }

        [Test]
        public void PermitChild_TransitionToChild_SuspendsParentAndEntersChildWithoutExitingParent() {
            var scheduler = new CoroutineScheduler();
            var owner = new TestOwner();
            var machine = new StateMachine<TestOwner>(owner, scheduler);
            var log = new List<string>();
            var parentId = new StateId("Parent");
            var childId = new StateId("Child");
            var parent = machine.AddState(parentId, new RecordingState(machine, "Parent", log));
            machine.AddState(childId, new RecordingState(machine, "Child", log));
            parent.PermitChild(childId);

            machine.Transition(parentId);
            log.Clear();
            machine.Transition(childId);

            CollectionAssert.AreEqual(new[] { "Parent.OnSuspend", "Child.OnEnter" }, log);
            Assert.IsFalse(machine.IsTransitioning);
        }

        [Test]
        public void Transition_ToStateNotPermittedFromCurrent_Throws() {
            var scheduler = new CoroutineScheduler();
            var owner = new TestOwner();
            var machine = new StateMachine<TestOwner>(owner, scheduler);
            var aId = new StateId("A");
            var bId = new StateId("B");
            machine.AddState(aId, new State(machine)); // no Permit() - B is unreachable from A
            machine.AddState(bId, new State(machine));

            machine.Transition(aId);

            Assert.Throws<Exception>(() => machine.Transition(bId));
        }

        [Test]
        public void TransitionToParent_AtTopLevelState_ThrowsInvalidOperationException() {
            var scheduler = new CoroutineScheduler();
            var owner = new TestOwner();
            var machine = new StateMachine<TestOwner>(owner, scheduler);
            var aId = new StateId("A");
            machine.AddState(aId, new State(machine));

            machine.Transition(aId);

            Assert.Throws<InvalidOperationException>(() => machine.TransitionToParent());
        }

        [Test]
        public void Transition_ThrowsWhileAnotherTransitionIsStillInProgress() {
            var scheduler = new CoroutineScheduler();
            var owner = new TestOwner();
            var machine = new StateMachine<TestOwner>(owner, scheduler);
            var aId = new StateId("A");
            var bId = new StateId("B");
            // OnEnter yields a real WaitCommand, so the transition genuinely spans multiple
            // scheduler ticks instead of completing synchronously inside Transition() - that's
            // what makes IsTransitioning observable from outside.
            machine.AddState(aId, new SlowEnterState(machine, frameCount: 5));
            machine.AddState(bId, new State(machine));

            machine.Transition(aId);

            Assert.IsTrue(machine.IsTransitioning);
            Assert.Throws<Exception>(() => machine.Transition(bId));
        }

        [Test]
        public void StateEventAttribute_RoutesOwnerEventToActiveStatesImplementation() {
            // Mirrors the real pattern used by VoloStateMachine/Playing.cs: [StateEvent("Update")]
            // on the owner routes to a private "Update" method on whichever state is active.
            var scheduler = new CoroutineScheduler();
            var owner = new TickOwner();
            var machine = new StateMachine<TickOwner>(owner, scheduler);
            var aId = new StateId("A");
            var state = new TickCountingState(machine);
            machine.AddState(aId, state);

            machine.Transition(aId);
            owner.Tick();
            owner.Tick();

            Assert.AreEqual(2, state.TickCount);
        }

        private class TestOwner { }

        private class RecordingState : State {
            private readonly string _name;
            private readonly List<string> _log;

            public RecordingState(IStateMachine machine, string name, List<string> log) : base(machine) {
                _name = name;
                _log = log;
            }

            private void OnEnter() { _log.Add(_name + ".OnEnter"); }
            private void OnExit() { _log.Add(_name + ".OnExit"); }
            private void OnSuspend() { _log.Add(_name + ".OnSuspend"); }
            private void OnResume() { _log.Add(_name + ".OnResume"); }
        }

        private class SlowEnterState : State {
            private readonly int _frameCount;

            public SlowEnterState(IStateMachine machine, int frameCount) : base(machine) {
                _frameCount = frameCount;
            }

            private IEnumerator<WaitCommand> OnEnter() {
                yield return WaitCommand.WaitFrames(_frameCount);
            }
        }

        private class TickOwner {
            [StateEvent("Tick")]
            public event Action OnTick;
            public void Tick() { if (OnTick != null) OnTick(); }
        }

        private class TickCountingState : State {
            public int TickCount;
            public TickCountingState(IStateMachine machine) : base(machine) { }
            private void Tick() { TickCount++; }
        }
    }

    // Tier 3: regression test for a bug actually found and fixed while porting this project to
    // Unity 6 (see Documentation/unity6-port-roadmap.md, "runtime debugging part 3" entry, and
    // the inline comment on StateMachine<T>.TransitionToParent()).
    [TestFixture]
    public class StateMachineRegressionTests {

        [Test]
        public void TransitionToParent_DoesNotDoublePopStack_AcrossRepeatedSuspendResumeCycles() {
            // TransitionToParent() used to pop the stack itself AND delegate to a coroutine
            // that popped again. Every parent transition therefore removed two entries: after
            // the first pause -> resume, the stack was silently already empty instead of still
            // holding the parent. The *second* pause then took the "stack is empty, enter as a
            // fresh top-level state" branch instead of the child-transition branch - skipping
            // Parent.OnSuspend entirely - and closing the menu after that hit the
            // "_stack.Count <= 1" top-level guard and threw. This test performs the
            // suspend/resume cycle twice, which only passes if the stack depth is correct after
            // the first cycle.
            var scheduler = new CoroutineScheduler();
            var owner = new TestOwner();
            var machine = new StateMachine<TestOwner>(owner, scheduler);
            var log = new List<string>();
            var parentId = new StateId("Parent");
            var childId = new StateId("Child");
            var parent = machine.AddState(parentId, new RecordingState(machine, "Parent", log));
            machine.AddState(childId, new RecordingState(machine, "Child", log));
            parent.PermitChild(childId);

            machine.Transition(parentId); // enter Parent (root)
            log.Clear();

            // First suspend/resume cycle - e.g. opening and closing a pause menu once.
            machine.Transition(childId);
            machine.TransitionToParent();
            log.Clear();

            // Second cycle - this is exactly what broke before the double-pop fix.
            machine.Transition(childId);
            Assert.DoesNotThrow(() => machine.TransitionToParent());

            CollectionAssert.AreEqual(
                new[] { "Parent.OnSuspend", "Child.OnEnter", "Child.OnExit", "Parent.OnResume" },
                log);
        }

        private class TestOwner { }

        private class RecordingState : State {
            private readonly string _name;
            private readonly List<string> _log;

            public RecordingState(IStateMachine machine, string name, List<string> log) : base(machine) {
                _name = name;
                _log = log;
            }

            private void OnEnter() { _log.Add(_name + ".OnEnter"); }
            private void OnExit() { _log.Add(_name + ".OnExit"); }
            private void OnSuspend() { _log.Add(_name + ".OnSuspend"); }
            private void OnResume() { _log.Add(_name + ".OnResume"); }
        }
    }
}
