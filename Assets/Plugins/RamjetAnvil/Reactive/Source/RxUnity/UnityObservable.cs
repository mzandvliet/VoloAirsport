using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Reactive {
    public static class UnityObservable {
        /// <summary>
        ///     <para>
        ///         Create an observable who's observer is called from the MonoBehaviour.Update method.
        ///         Can be used to create an Rx Stream from polling for data inside a MonoBehaviour.Update loop.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">The observable's type</typeparam>
        /// <param name="subscribe">The method that is called on each call to MonoBehaviour.Update</param>
        /// <returns>An observable who's events are produced inside the MonoBehaviour update loop</returns>
        public static IObservable<T> CreateUpdate<T>(Action<IObserver<T>> subscribe) {
            return Observable.Create<T>(observer => {
                IDisposable updateDisposable = AnonymousMonoBehaviours.Update(() => subscribe(observer),
                    observer.OnCompleted);
                return updateDisposable;
            }).Publish().RefCount();
        }

        public static IObservable<T> DistinctUntilChanged<T>(this IObservable<T> source, Func<T, T, bool> comparer) {
            return Observable.Create<T>(observer => {
                bool isFirst = true;
                T lastValue = default(T);
                return source.Subscribe(value => {
                    if (isFirst || !comparer(value, lastValue)) {
                        isFirst = false;
                        lastValue = value;
                        observer.OnNext(value);
                    }
                }, observer.OnCompleted);
            });
        }

        public static IObservable<TSource> FilterOn<TSource, TFilter>(this IObservable<TSource> source,
            IObservable<TFilter> filter, Func<TFilter, bool> predicate) {
            return Observable.Create<TSource>(observer => {
                bool isSourceOpen = false;

                IDisposable filterDisp = filter.Subscribe(value => { isSourceOpen = predicate(value); });

                IDisposable sourceDisp = source.Subscribe(value => {
                    if (isSourceOpen) {
                        observer.OnNext(value);
                    }
                });

                return new CompositeDisposable(filterDisp, sourceDisp);
            });
        }

        public static IObservable<T> Suspenable<T>(this IObservable<T> source, IObservable<bool> suspension) {
            return suspension.Select(isSuspended => {
                if (isSuspended) {
                    return Observable.Empty<T>();
                }
                return source;
            }).Switch();
        }
    }
}