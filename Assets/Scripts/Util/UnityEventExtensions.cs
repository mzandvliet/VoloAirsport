using System;
using System.Reactive.Disposables;
using UnityEngine.Events;

namespace RamjetAnvil.Volo.Util {
    public static class UnityEventExtensions {

        public static IDisposable OnChange(this UnityEvent @event, UnityAction a) {
            @event.AddListener(a);
            return Disposable.Create(() => @event.RemoveListener(a));
        }

        public static IDisposable OnChange<T>(this UnityEvent<T> @event, UnityAction<T> a) {
            @event.AddListener(a);
            return Disposable.Create(() => @event.RemoveListener(a));
        }
    }
}
