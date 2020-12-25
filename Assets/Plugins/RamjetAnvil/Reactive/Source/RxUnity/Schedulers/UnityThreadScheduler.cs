using System;
using System.Collections;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using UnityEngine;

namespace RxUnity.Schedulers
{
    public static class UnityThreadScheduler
    {
        static readonly Lazy<IScheduler> mainThread = new Lazy<IScheduler>(() => new UnityScheduler());

        public static IScheduler MainThread
        {
            get { return mainThread.Value; }
        }

        class UnityScheduler : IScheduler
        {
            IEnumerator DelayAction(TimeSpan dueTime, Action action)
            {
                yield return new WaitForSeconds((float)dueTime.TotalSeconds);
                action();
            }

            public DateTimeOffset Now
            {
                get { return DateTimeOffset.Now; }
            }

            public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
            {
                var d = new SingleAssignmentDisposable();
                UnityThreadDispatcher.Instance.Post(() =>
                {
                    if (!d.IsDisposed)
                    {
                        d.Disposable = action(this, state);
                    }
                });
                return d;
            }

            public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
            {
                return Schedule(state, dueTime - Now, action);
            }

            public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
            {
                var d = new SingleAssignmentDisposable();
                UnityThreadDispatcher.Instance.QueueCoroutine(DelayAction(dueTime, () =>
                {
                    if (!d.IsDisposed)
                    {
                        d.Disposable = action(this, state);
                    }
                }));

                return d;
            }
        }
    }
}