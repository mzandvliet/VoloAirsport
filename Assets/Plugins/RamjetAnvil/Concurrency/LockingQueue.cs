using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RamjetAnvil.Threading {

    public class LockingQueue<T> : IProducerConsumerCollection<T> {

        private readonly object _syncRoot;
        private readonly Queue<T> _queue; 

        public LockingQueue(int capacity) {
            _syncRoot = new object();
            _queue = new Queue<T>(capacity);
        }

        public IEnumerator<T> GetEnumerator() {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public int Count {
            get {
                lock (_syncRoot) {
                    return _queue.Count;
                }
            }
        }

        public bool IsSynchronized {
            get { return true; }
        }

        public object SyncRoot {
            get { return _syncRoot; }
        }

        public void CopyTo(T[] array, int index) {
            throw new NotImplementedException();
        }

        public void BatchAdd(IList<T> items) {
            if (items.Count > 0) {
                lock (_syncRoot) {
                    for (int i = 0; i < items.Count; i++) {
                        _queue.Enqueue(items[i]);
                    }
                }
            }
        }

        public bool TryAdd(T item) {
            lock (_syncRoot) {
                _queue.Enqueue(item);
                return true;
            }
        }

        public bool TryTake(out T item) {
            lock (_syncRoot) {
                if (_queue.Count > 0) {
                    item = _queue.Dequeue();
                    return true;
                } else {
                    item = default(T);
                    return false;
                }
            }
        }

        public T[] ToArray() {
            throw new NotImplementedException();
        }
    }

}