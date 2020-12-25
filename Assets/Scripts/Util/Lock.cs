using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RamjetAnvil.Volo.Util {

    public interface ILock {
        void AcquireLock();

        void ReleaseLock();
    }

    public static class LockExtensions {

        public static Action RunWithLock(this ILock @lock, Action action) {
            return () => {
                try {
                    @lock.AcquireLock();
                    action();
                } finally {
                    @lock.ReleaseLock();
                }
            };
        }

        public static Action<T> RunWithLock<T>(this ILock @lock, Action<T> action) {
            return value => {
                try {
                    @lock.AcquireLock();
                    action(value);
                } finally {
                    @lock.ReleaseLock();
                }
            };
        }
    }
    
}
