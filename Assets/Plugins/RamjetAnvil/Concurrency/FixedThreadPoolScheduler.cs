using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RamjetAnvil.Threading {

    public class FixedThreadPoolScheduler : TaskScheduler, IDisposable {

        private readonly Thread[] _threads;
        private readonly BlockingCollection<Task> _tasks;

        /// <summary>
        /// Create a new <see cref="FixedThreadPoolScheduler"/> with a provided number of threads. 
        /// </summary>
        /// <param name="numberOfThreads">Total number of threads in the pool.</param>
        /// <param name="isBackground">Whether the threads will run on the background</param>
        public FixedThreadPoolScheduler(int numberOfThreads, bool isBackground = true) {
            if (numberOfThreads < 1)
                throw new ArgumentOutOfRangeException("numberOfThreads");

            _threads = new Thread[numberOfThreads];
            _tasks = new BlockingCollection<Task>();

            for (int i = 0; i < numberOfThreads; i++) {
                var thread = new Thread(ThreadStart) { IsBackground = isBackground };
                _threads[i] = thread;
                thread.Start();
            }
        }

        private void ThreadStart() {
            foreach (var t in _tasks.GetConsumingEnumerable()) {
                TryExecuteTask(t);
            }
        }

        /// <summary>
        /// Queues a <see cref="T:System.Threading.Tasks.Task"/> to the scheduler.
        /// </summary>
        /// <param name="task">The <see cref="T:System.Threading.Tasks.Task"/> to be queued.</param><exception cref="T:System.ArgumentNullException">The <paramref name="task"/> argument is null.</exception>
        protected override void QueueTask(Task task) {
            _tasks.Add(task);
        }

        /// <summary>
        /// Determines whether the provided <see cref="T:System.Threading.Tasks.Task"/> can be executed synchronously in this call, and if it can, executes it.
        /// </summary>
        /// <returns>
        /// A Boolean value indicating whether the task was executed inline.
        /// </returns>
        /// <param name="task">The <see cref="T:System.Threading.Tasks.Task"/> to be executed.</param><param name="taskWasPreviouslyQueued">A Boolean denoting whether or not task has previously been queued. If this parameter is True, then the task may have been previously queued (scheduled); if False, then the task is known not to have been queued, and this call is being made in order to execute the task inline without queuing it.</param><exception cref="T:System.ArgumentNullException">The <paramref name="task"/> argument is null.</exception><exception cref="T:System.InvalidOperationException">The <paramref name="task"/> was already executed.</exception>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
            return false;
        }

        /// <summary>
        /// Generates an enumerable of <see cref="T:System.Threading.Tasks.Task"/> instances currently queued to the scheduler waiting to be executed.
        /// </summary>
        /// <returns>
        /// An enumerable that allows traversal of tasks currently queued to this scheduler.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">This scheduler is unable to generate a list of queued tasks at this time.</exception>
        protected override IEnumerable<Task> GetScheduledTasks() {
            return _tasks.ToArray();
        }

        /// <summary>
        /// Indicates the maximum concurrency level this <see cref="T:System.Threading.Tasks.TaskScheduler"/> is able to support.
        /// </summary>
        /// <returns>
        /// Returns an integer that represents the maximum concurrency level.
        /// </returns>
        public override int MaximumConcurrencyLevel {
            get { return _threads.Length; }
        }

        public void Dispose() {
            if (_tasks != null) {
                _tasks.CompleteAdding();
                for (int i = 0; i < _threads.Length; i++) {
                    _threads[i].Abort();
                }
                _tasks.Dispose();
            }
        }
    }
}
