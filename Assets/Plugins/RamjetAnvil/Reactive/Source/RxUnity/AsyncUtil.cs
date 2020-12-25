using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace RamjetAnvil.Reactive {
    public class AsyncUtil {
        public static void DoWorkAsync(Action work) {
            Task.Factory.StartNew(work);
        }

        public static IObservable<T> DoWorkAsync<T>(Func<T> work, IScheduler scheduler) {
            var asyncSubject = new AsyncSubject<T>();

            Task.Factory.StartNew(() => {
                try {
                    T result = work.Invoke();
                    asyncSubject.OnNext(result);
                    asyncSubject.OnCompleted();
                }
                catch (Exception e) {
                    asyncSubject.OnError(e);
                }
            });

            return asyncSubject.ObserveOn(scheduler)
                .SubscribeOn(scheduler);
        }
    }
}