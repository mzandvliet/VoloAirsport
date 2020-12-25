using UnityEngine;
using System;

namespace RamjetAnvil.Unity.Utils {

    /// <summary>
    /// Used to explicitely manage update order between multiple dependent components.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUpdateEventSource<T> {
        event Action<T> OnPreUpdate;
        event Action<T> OnPostUpdate;
        event Action<T> OnPreFixedUpdate;
        event Action<T> OnPostFixedUpdate;
    }
}
