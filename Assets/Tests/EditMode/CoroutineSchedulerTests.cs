using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RamjetAnvil.Coroutine;
using UnityEngine;
using UnityEngine.TestTools;

namespace RamjetAnvil.Coroutine.Tests {

    // Tier 1: behaviour documented by the CoroutineScheduler README/public API
    // (Dependencies/CoroutineScheduler/README.md) and exercised by every game state that
    // uses WaitCommand combinators.
    [TestFixture]
    public class CoroutineSchedulerDocumentedBehaviourTests {

        [Test]
        public void Run_WithEmptyRoutine_CompletesSynchronouslyWithoutAnUpdateCall() {
            // CoroutineScheduler.CreateRoutine() calls Routine.Initialize(), which eagerly
            // runs the fibre until it hits a real wait command - a routine with nothing to
            // wait on finishes inside Run() itself.
            var scheduler = new CoroutineScheduler();
            var awaitable = scheduler.Run(EmptyRoutine());
            Assert.IsTrue(awaitable.IsDone);
        }

        [Test]
        public void WaitFrames_CompletesAfterExactNumberOfUpdateTicks() {
            var scheduler = new CoroutineScheduler();
            var awaitable = scheduler.Run(WaitNFrames(3));
            Assert.IsFalse(awaitable.IsDone);

            scheduler.Update(0, 0.0);
            Assert.IsFalse(awaitable.IsDone, "should still be waiting after 1 of 3 frames");
            scheduler.Update(1, 0.0);
            Assert.IsFalse(awaitable.IsDone, "should still be waiting after 2 of 3 frames");
            scheduler.Update(2, 0.0);
            Assert.IsTrue(awaitable.IsDone, "should be done after exactly 3 frames");
        }

        [Test]
        public void WaitSeconds_CompletesOnceElapsedTimeReachesDuration() {
            var scheduler = new CoroutineScheduler();
            var awaitable = scheduler.Run(WaitHalfASecond());

            scheduler.Update(0, 0.0);
            scheduler.Update(1, 0.3);
            Assert.IsFalse(awaitable.IsDone, "0.3s elapsed, should still be waiting for 0.5s");
            scheduler.Update(2, 0.5);
            Assert.IsTrue(awaitable.IsDone, "0.5s elapsed, should be done");
        }

        [Test]
        public void WaitRoutine_RunsNestedRoutineToCompletionBeforeContinuing() {
            var scheduler = new CoroutineScheduler();
            var order = new List<string>();
            var awaitable = scheduler.Run(OuterRoutine(order));

            scheduler.Update(0, 0.0);

            Assert.IsTrue(awaitable.IsDone);
            CollectionAssert.AreEqual(new[] { "before", "inner", "after" }, order);
        }

        [Test]
        public void Interleave_RunsSubroutinesConcurrentlyAndWaitsForAll() {
            var scheduler = new CoroutineScheduler();
            var completedFast = false;
            var completedSlow = false;
            var awaitable = scheduler.Run(WaitCommand.Interleave(
                RecordCompletion(1, () => completedFast = true),
                RecordCompletion(2, () => completedSlow = true)).AsRoutine);

            scheduler.Update(0, 0.0);
            Assert.IsTrue(completedFast, "1-frame subroutine should be done");
            Assert.IsFalse(completedSlow, "2-frame subroutine should still be running");
            Assert.IsFalse(awaitable.IsDone, "parent should wait for both subroutines");

            scheduler.Update(1, 0.0);
            Assert.IsTrue(completedSlow);
            Assert.IsTrue(awaitable.IsDone);
        }

        [Test]
        public void Dispose_CancelsRoutineBeforeItWouldNaturallyComplete() {
            var scheduler = new CoroutineScheduler();
            var ranToCompletion = false;
            var awaitable = scheduler.Run(LongRunningRoutine(() => ranToCompletion = true));
            Assert.IsFalse(awaitable.IsDone);

            awaitable.Dispose();
            Assert.IsTrue(awaitable.IsDone, "Dispose() should mark the routine done immediately");

            scheduler.Update(0, 0.0);
            Assert.IsFalse(ranToCompletion, "a disposed routine must not run its remaining body");
        }

        [Test]
        public void WaitUntilDone_CompletesOnceTheAwaitedRoutineFinishes() {
            var scheduler = new CoroutineScheduler();
            var innerHandle = scheduler.Run(WaitNFrames(2));
            var waiterDone = false;
            scheduler.Run(WaitThenFlag(innerHandle, () => waiterDone = true));

            for (int frame = 0; frame < 10 && !waiterDone; frame++) {
                scheduler.Update(frame, 0.0);
            }

            Assert.IsTrue(innerHandle.IsDone);
            Assert.IsTrue(waiterDone, "WaitUntilDone() should eventually observe the awaited routine finishing");
        }

        [Test]
        public void AndThen_RunsBothWaitCommandsInSequence() {
            var scheduler = new CoroutineScheduler();
            var order = new List<string>();
            var awaitable = scheduler.Run(First(order).AndThen(Second(order)));

            for (int frame = 0; frame < 10 && !awaitable.IsDone; frame++) {
                scheduler.Update(frame, 0.0);
            }

            CollectionAssert.AreEqual(new[] { "first", "second" }, order);
        }

        private static IEnumerator<WaitCommand> EmptyRoutine() {
            yield break;
        }

        private static IEnumerator<WaitCommand> WaitNFrames(int frameCount) {
            yield return WaitCommand.WaitFrames(frameCount);
        }

        private static IEnumerator<WaitCommand> WaitHalfASecond() {
            yield return WaitCommand.WaitSeconds(0.5f);
        }

        private static IEnumerator<WaitCommand> OuterRoutine(List<string> order) {
            order.Add("before");
            yield return WaitCommand.WaitRoutine(InnerRoutine(order));
            order.Add("after");
        }

        private static IEnumerator<WaitCommand> InnerRoutine(List<string> order) {
            yield return WaitCommand.WaitFrames(1);
            order.Add("inner");
        }

        private static IEnumerator<WaitCommand> RecordCompletion(int frameCount, Action onComplete) {
            yield return WaitCommand.WaitFrames(frameCount);
            onComplete();
        }

        private static IEnumerator<WaitCommand> LongRunningRoutine(Action onComplete) {
            yield return WaitCommand.WaitFrames(1000);
            onComplete();
        }

        private static IEnumerator<WaitCommand> WaitThenFlag(IAwaitable awaitable, Action onDone) {
            yield return awaitable.WaitUntilDone();
            onDone();
        }

        private static IEnumerator<WaitCommand> First(List<string> order) {
            yield return WaitCommand.WaitFrames(1);
            order.Add("first");
        }

        private static IEnumerator<WaitCommand> Second(List<string> order) {
            yield return WaitCommand.WaitFrames(1);
            order.Add("second");
        }
    }

    // Tier 3: regression tests for bugs actually found and fixed while porting this project
    // to Unity 6 (see Documentation/unity6-port-roadmap.md, "runtime debugging part 3" and
    // "three precompiled dependencies become source" entries).
    [TestFixture]
    public class CoroutineSchedulerRegressionTests {

        [Test]
        public void RoutineHandle_KeepsReportingTrueCompletion_AfterPooledSlotIsReusedByAnotherRoutine() {
            // Regression test for the "waiter never notices completion" hang: CoroutineScheduler
            // used to hand out the pooled Routine object itself as the IAwaitable. Once that
            // Routine was recycled and handed to an unrelated new coroutine, an old handle's
            // IsDone would start reflecting the NEW coroutine's state instead of the one it was
            // actually issued for - silently un-signalling completion to anything still waiting
            // on it (WaitUntilDone() would then loop forever). Fixed via generation-stamped
            // RoutineHandle, bumped on both Initialize() and Reset().
            //
            // growthStep: 1 forces the pool down to exactly one Routine instance, guaranteeing
            // routine B below reuses the exact same pooled object routine A just vacated.
            var scheduler = new CoroutineScheduler(initialCapacity: 1, growthStep: 1);

            var handleA = scheduler.Run(EmptyRoutine());
            Assert.IsTrue(handleA.IsDone, "precondition: A completes synchronously on Run()");

            scheduler.Update(0, 0.0); // lets the scheduler actually recycle A's Routine to the pool

            var handleB = scheduler.Run(WaitNFrames(1000));
            Assert.IsFalse(handleB.IsDone, "precondition: B is still running");

            Assert.IsTrue(handleA.IsDone,
                "old handle A must still report done, not silently flip to false because its " +
                "pooled slot now belongs to B");
        }

        [Test]
        public void SchedulerRemainsConsistent_WhenARoutineSchedulesAnotherRoutineDuringUpdate() {
            // The historical crash (see roadmap: "UnityCoroutineScheduler.Run() pumps Update()
            // as its first step") lived specifically in the UnityCoroutineScheduler/
            // FixedUnityCoroutineScheduler MonoBehaviour wrappers, which eagerly call their own
            // Update() from inside Run() - reentering Update() while a previous call is still
            // iterating the routine list. That exact reentrant call shape can't be exercised
            // here without a live Update loop and dependency injection (see dont-forget-me.md).
            // What IS exercisable at this level: a routine body scheduling brand-new work on the
            // same scheduler instance while that scheduler is mid-Update() must not corrupt or
            // throw - this documents/guards that narrower, always-true safety property.
            var scheduler = new CoroutineScheduler();
            var spawnedRoutineRan = false;

            var awaitable = scheduler.Run(SpawningRoutine(scheduler, () => spawnedRoutineRan = true));

            Assert.DoesNotThrow(() => scheduler.Update(0, 0.0));
            Assert.DoesNotThrow(() => scheduler.Update(1, 0.0));

            Assert.IsTrue(spawnedRoutineRan);
            Assert.IsTrue(awaitable.IsDone);
        }

        [Test]
        public void FaultingRoutine_IsMarkedDoneInsteadOfRetriedForever() {
            // Regression test for Routine.FetchNextInstruction()'s exception handling: an
            // exception thrown while advancing a routine used to abort the scheduler's entire
            // per-frame update pass (rethrown mid-loop), which could leave sibling/parent
            // routines torn down incorrectly and caused the same faulting routine to be
            // re-entered and re-throw every frame forever. Fixed to log, mark the routine done,
            // and let it recycle normally instead.
            LogAssert.Expect(LogType.Error, new Regex("\\[CoroutineScheduler\\].*boom", RegexOptions.Singleline));

            var scheduler = new CoroutineScheduler();
            var awaitable = scheduler.Run(ThrowingRoutine());

            Assert.IsTrue(awaitable.IsDone, "a faulting routine should be marked done, not retried");
            Assert.DoesNotThrow(() => scheduler.Update(0, 0.0));
        }

        private static IEnumerator<WaitCommand> EmptyRoutine() {
            yield break;
        }

        private static IEnumerator<WaitCommand> WaitNFrames(int frameCount) {
            yield return WaitCommand.WaitFrames(frameCount);
        }

        private static IEnumerator<WaitCommand> SpawningRoutine(ICoroutineScheduler scheduler, Action onSpawnedRan) {
            yield return WaitCommand.WaitFrames(1);
            scheduler.Run(SpawnedRoutine(onSpawnedRan));
            yield return WaitCommand.WaitFrames(1);
        }

        private static IEnumerator<WaitCommand> SpawnedRoutine(Action onRan) {
            onRan();
            yield break;
        }

        private static IEnumerator<WaitCommand> ThrowingRoutine() {
            throw new InvalidOperationException("boom");
#pragma warning disable 162
            yield break;
#pragma warning restore 162
        }
    }
}
