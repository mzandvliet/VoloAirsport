using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RamjetAnvil.Reactive {
    public static class RxExtensions {
        // Todo: the Rx.NET version bundled with Unity (2.2.0.0) predates WithLatestFrom being
        // added to mainline System.Reactive.Linq - this is a minimal reimplementation.
        public static IObservable<TResult> WithLatestFrom<TSource, TOther, TResult>(
            this IObservable<TSource> source, IObservable<TOther> other, Func<TSource, TOther, TResult> resultSelector) {
            return Observable.Create<TResult>(observer => {
                var hasLatest = false;
                var latest = default(TOther);

                var otherSubscription = other.Subscribe(value => {
                    latest = value;
                    hasLatest = true;
                }, observer.OnError);

                var sourceSubscription = source.Subscribe(value => {
                    if (hasLatest) {
                        observer.OnNext(resultSelector(value, latest));
                    }
                }, observer.OnError, observer.OnCompleted);

                return new CompositeDisposable(otherSubscription, sourceSubscription);
            });
        }
    }
}
