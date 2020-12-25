using System;
using System.Collections.Generic;
using System.Diagnostics;
using Disruptor;
using Disruptor.Dsl;
using Priority_Queue;
using RamjetAnvil.Unity.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

/*
 - Todo: Maybe have separate classes for unity and async jobs
 */

namespace RamjetAnvil.Threading {

    public class JobManager : MonoBehaviour {
        [SerializeField] private float _frameCompletionTimeInMs = 0.5f;
        [SerializeField] private int _jobPoolSize = 2048;

        private FixedThreadPoolScheduler _asyncThreadPoolScheduler;
        private FixedThreadPoolScheduler _asyncIoThreadPoolScheduler;
        private Disruptor<DisruptorTask> _asyncDisruptor;
        private Disruptor<DisruptorTask> _asyncIoDisruptor;
        private IPriorityQueue<JobTask> _unityTasks;

        private Queue<JobTask> _taskPool;
        private Queue<Job> _jobPool;

        private IList<Job> _queuedJobs;

        private long _frameStartTime;

        void Awake() {
            _jobPoolSize = Mathf.ClosestPowerOfTwo(_jobPoolSize);
            _jobPool = new Queue<Job>(_jobPoolSize);
            var taskPoolSize = _jobPoolSize * 3;
            _taskPool = new Queue<JobTask>(taskPoolSize);

            Action<Job> taskCompleted = OnTaskCompleted;
            for (int i = 0; i < _jobPoolSize; i++) {
                _jobPool.Enqueue(new Job(_taskPool, taskCompleted));
            }
            for (int i = 0; i < taskPoolSize; i++) {
                _taskPool.Enqueue(new JobTask());
            }

            _unityTasks = new HeapPriorityQueue<JobTask>(_jobPoolSize);
            _queuedJobs = new List<Job>(_jobPoolSize);

            var numAsyncThreads = 1;
            //var numAsyncThreads = Mathf.Clamp(SystemInfo.processorCount / 2, 1, 2);
            bool singleDisruptor = numAsyncThreads == 1;

            if (singleDisruptor) {
                _asyncThreadPoolScheduler = new FixedThreadPoolScheduler(numAsyncThreads, isBackground: false);
                _asyncDisruptor = new Disruptor<DisruptorTask>(
                    () => new DisruptorTask(),
                    new MultiThreadedClaimStrategy(_jobPoolSize),
                    new BlockingWaitStrategy(),
                    _asyncThreadPoolScheduler);
                _asyncDisruptor.HandleEventsWith(new DisruptorJobHandler());
                _asyncDisruptor.Start();

                _asyncIoDisruptor = _asyncDisruptor;

                Debug.Log("JobManager || Using single disruptor, with " + numAsyncThreads + " async threads, " + 0 + " io threads");
            }
            else {
                _asyncThreadPoolScheduler = new FixedThreadPoolScheduler(numAsyncThreads, isBackground: false);
                _asyncDisruptor = new Disruptor<DisruptorTask>(
                    () => new DisruptorTask(),
                    new MultiThreadedClaimStrategy(_jobPoolSize),
                    new BlockingWaitStrategy(),
                    _asyncThreadPoolScheduler);
                _asyncDisruptor.HandleEventsWith(new DisruptorJobHandler());
                _asyncDisruptor.Start();

                var numIoThreads = 1;
                _asyncIoThreadPoolScheduler = new FixedThreadPoolScheduler(numIoThreads, isBackground: false);
                _asyncIoDisruptor = new Disruptor<DisruptorTask>(
                    () => new DisruptorTask(),
                    new MultiThreadedClaimStrategy(_jobPoolSize),
                    new BlockingWaitStrategy(),
                    _asyncThreadPoolScheduler);
                _asyncIoDisruptor.HandleEventsWith(new DisruptorJobHandler());
                _asyncIoDisruptor.Start();

                Debug.Log("JobManager || Using multiple disruptors, with " + numAsyncThreads + " async threads, " + numIoThreads + " io threads");
            }
        }

        public Job CreateJob(object input = null) {

            UnityEngine.Profiling.Profiler.BeginSample("Create Job");

            var job = _jobPool.Dequeue();
            job.InitialInput = input;

            UnityEngine.Profiling.Profiler.EndSample();

            return job;
        }

        public IDisposable StartJob(Job job) {
            if (job.CurrentTask == null) {
                throw new ArgumentException("Cannot start a job that doesn't have any tasks");
            }

            UnityEngine.Profiling.Profiler.BeginSample("Start Job");

            _queuedJobs.Add(job);
            job.CurrentTask.Input = job.InitialInput;
            job.IsBeingProcessed = true;

            EnqueueTask(job.CurrentTask);

            UnityEngine.Profiling.Profiler.EndSample();

            return job.CancelToken;
        }

        void Update() {
            _frameStartTime = CurrentTimeMillis();
            ProcessJobs();
        }

        void LateUpdate() {
            ProcessJobs();
        }

        private void ProcessJobs() {

            UnityEngine.Profiling.Profiler.BeginSample("Process Jobs");

            for (int i = _queuedJobs.Count - 1; i >= 0; i--) {
                var job = _queuedJobs[i];

                if (!job.IsBeingProcessed) {
                    if (job.IsCanceled) {
                        job.OnCanceled(job);
                    }

                    job.Reset();
                    _queuedJobs.RemoveAt(i);
                    _jobPool.Enqueue(job);
                } 
                // This is the one special case where we need to enqueue a new task from the unity thread.
                // The async job schedulers are not allowed to enqueue a task directly on the unity thread
                // because it would involve locking or using a slow data structure like the ConcurrentQueue.
                // This alternative frees up precious CPU time for the Unity thread
                else if (job.CurrentTask.NeedsQueueing) {
                    EnqueueTask(job.CurrentTask);
                }
            }

            while (_unityTasks.Count > 0 && CurrentTimeMillis() - _frameStartTime < _frameCompletionTimeInMs) {
                var task = _unityTasks.Dequeue();
                task.Run();
            }

            UnityEngine.Profiling.Profiler.EndSample();

        }

        private static long CurrentTimeMillis() {
            return (long)TimeSpan.FromTicks(Stopwatch.GetTimestamp()).TotalMilliseconds;
        }

        // Enqueue from unity thread
        private void EnqueueTask(JobTask task) {

            task.NeedsQueueing = false;
            switch (task.Type) {
                case JobTaskType.UnityThread:
                    UnityEngine.Profiling.Profiler.BeginSample("EnqueueTaskUnity_Unity");
                    _unityTasks.Enqueue(task, JobTask.HighPriority);
                    UnityEngine.Profiling.Profiler.EndSample();
                    break;
                case JobTaskType.Async:
                    UnityEngine.Profiling.Profiler.BeginSample("EnqueueTaskUnity_Async");
                    QueueDisruptorTask(_asyncDisruptor, task);
                    UnityEngine.Profiling.Profiler.EndSample();
                    break;
                case JobTaskType.AsyncIo:
                    UnityEngine.Profiling.Profiler.BeginSample("EnqueueTaskUnity_AsyncIo");
                    QueueDisruptorTask(_asyncIoDisruptor, task);
                    UnityEngine.Profiling.Profiler.EndSample();
                    break;
            }
        }

        private void EnqueueTaskAsync(JobTask task) {
            switch (task.Type) {
                case JobTaskType.Async:
                    QueueDisruptorTask(_asyncDisruptor, task);
                    break;
                case JobTaskType.AsyncIo:
                    QueueDisruptorTask(_asyncIoDisruptor, task);
                    break;
                case JobTaskType.UnityThread:
                    // Wait for the unity thread to schedule
                    // we can't access the task queue from another thread
                    task.NeedsQueueing = true;
                    break;
            }
        }

        private void OnTaskCompleted(Job job) {
            var completedTask = job.CurrentTask;

            if (completedTask.NextTask != null && !job.IsCanceled) {
                var nextTask = completedTask.NextTask;
                nextTask.Input = completedTask.Output;
                // Set the current task for all threads to see
                job.CurrentTask = nextTask;

                // Three different schedule cases:
                // Unity -> Any
                // Async -> Async
                // Async -> Unity
                // two of which are handled here
                // for the other one we have to wait on the Unity thread

                // If this task was run on the unity thread we have to enqueue
                // a bit differently 
                if (completedTask.Type == JobTaskType.UnityThread) {
                    EnqueueTask(nextTask);
                }
                // If it isn't we immediately schedule a new task if it is async
                // If it isn't async then we have to wait for the Unity thread
                // to enqueue the next task
                else {
                    EnqueueTaskAsync(nextTask);
                }

            }
            else {
                // When done or canceled, flag this so the Unity thread picks it up
                job.IsBeingProcessed = false;
            }
        }

        private static void QueueDisruptorTask(Disruptor<DisruptorTask> disruptor, JobTask task) {
            task.NeedsQueueing = false;
            var buffer = disruptor.RingBuffer;
            var disruptorJobId = buffer.Next();
            var disruptorJob = buffer[disruptorJobId];
            disruptorJob.Task = task;

            buffer.Publish(disruptorJobId);
        }

        void OnDestroy() {
            _asyncDisruptor.Shutdown();
            _asyncDisruptor = null;
            _asyncThreadPoolScheduler.Dispose();
            _asyncThreadPoolScheduler = null;

            if (_asyncIoDisruptor != null) {
                _asyncIoDisruptor.Shutdown();
            }
            if (_asyncIoThreadPoolScheduler != null) {
                _asyncIoThreadPoolScheduler.Dispose();
            }
        }

        private class DisruptorTask {
            public JobTask Task;
        }

        private class DisruptorJobHandler : IEventHandler<DisruptorTask> {

            public void OnNext(DisruptorTask disruptorTask, long sequence, bool endOfBatch) {
                disruptorTask.Task.Run();
            }
        }

//        private class DisruptorTaskDispatcher : IEventHandler<DisruptorTask> {
//
//            private readonly Disruptor<DisruptorTask> _asyncDisruptor; 
//            private readonly Disruptor<DisruptorTask> _asyncIoDisruptor;
//
//            public DisruptorTaskDispatcher(Disruptor<DisruptorTask> asyncDisruptor, Disruptor<DisruptorTask> asyncIoDisruptor) {
//                _asyncDisruptor = asyncDisruptor;
//                _asyncIoDisruptor = asyncIoDisruptor;
//            }
//
//            public void OnNext(DisruptorTask disruptorTask, long sequence, bool endOfBatch) {
//                switch (disruptorTask.Task.Type) {
//                    case JobTaskType.Async:
//                        QueueDisruptorTask(_asyncDisruptor, disruptorTask.Task);
//                        break;
//                    case JobTaskType.AsyncIo:
//                        QueueDisruptorTask(_asyncIoDisruptor, disruptorTask.Task);
//                        break;
//                }
//            }
//        }
    }

    public class Job {

        private static readonly Action<Job> EmptyOnCancelled = job => { };

        public readonly CancelToken CancelToken;

        public object InitialInput;
        public JobTask CurrentTask;
        public Action<Job> OnCanceled;

        public bool IsBeingProcessed;

        private readonly Queue<JobTask> _taskPool; 
        private readonly IList<JobTask> _allTasks; 
        private readonly Action _onTaskDone;

        public Job(Queue<JobTask> taskPool, Action<Job> onTaskDone) {
            _taskPool = taskPool;
            _onTaskDone = () => onTaskDone(this);
            CancelToken = new CancelToken();
            _allTasks = new List<JobTask>(3);
        }

        public bool IsCanceled {
            get { return CancelToken.IsCanceled; }
        }

        public Job AddTask(Func<object, ICancelToken, object> work, JobTaskType type) {
            var task = _taskPool.Dequeue();
            task.NeedsQueueing = false;
            IsBeingProcessed = false;
            task.NextTask = null;
            task.Work = work;
            task.Type = type;
            task.CancelToken = CancelToken;
            task.OnDone = _onTaskDone;
            
            if (CurrentTask != null) {
                var prevTask = CurrentTask;
                while (prevTask.NextTask != null) {
                    prevTask = prevTask.NextTask;
                }
                prevTask.NextTask = task;
            } else {
                CurrentTask = task;
            }
            _allTasks.Add(task);

            return this;
        }

        public Job OnCancel(Action<Job> onCanceled) {
            OnCanceled = onCanceled;
            return this;
        }

        public void Reset() {
            for (int i = 0; i < _allTasks.Count; i++) {
                _taskPool.Enqueue(_allTasks[i]);
            }
            _allTasks.Clear();
            CurrentTask = null;
            CancelToken.Reset();
            OnCanceled = EmptyOnCancelled;
            InitialInput = null;
        }
    }

    public interface ICancelToken {
        bool IsCanceled { get; }
    }

    public class CancelToken : ICancelToken, IDisposable {

        private bool _isCanceled;

        public CancelToken() {
            _isCanceled = false;
        }

        public bool IsCanceled { get { return _isCanceled; } }

        public void Dispose() {
            _isCanceled = true;
        }

        public void Reset() {
            _isCanceled = false;
        }
    }

    public enum JobTaskType {
        UnityThread, Async, AsyncIo
    }

    public class JobTask : PriorityQueueNode {
        public static readonly double LowPriority = 100d;
        public static readonly double HighPriority = 0d;

        public JobTaskType Type;
        public CancelToken CancelToken;

        public bool NeedsQueueing;
        public object Input;
        public object Output;
        public Func<object, ICancelToken, object> Work;
        public Action OnDone;
        public JobTask NextTask;

        public readonly Action Run;

        public JobTask() {
            Run = () => {
                // Todo: How does try/catch affect job performance?
                try {
                    Output = Work(Input, CancelToken);
                } catch (Exception e) {
                    Debug.LogError("Job Exception: " + e);
                    CancelToken.Dispose();
                }
                OnDone();
            };
        }
    }

}
 