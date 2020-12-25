using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace RamjetAnvil.Volo.UI
{
    public interface IDataCursor {
        TData Get<TData>();
        void Update(object data);
        IDataCursor Refine(string key);
        IObservable<object> OnUpdate { get; }
    }

    public static class DataCursor {

        public static IDataCursor Create(object data) {
            return new Cursor(
                getData: () => data,
                updateData: newData => {
                    data = newData;
                });
        }

        public static TData Get<TData>(this IDataCursor cursor, string key) {
            return cursor.Refine(key).Get<TData>();
        }

        public static TData Get<TData>(this IDataCursor cursor, params string[] path) {
            return cursor.Refine(path).Get<TData>();
        }

        public static IDataCursor Refine(this IDataCursor initialCursor, params string[] path) {
            var currentCursor = initialCursor;
            for (int i = 0; i < path.Length; i++) {
                currentCursor = currentCursor.Refine(path[i]);
            }
            return currentCursor;
        }

        public static void Update(this IDataCursor cursor, string key, object data) {
            cursor.Refine(key).Update(data);
        }

        public static void Update(this IDataCursor cursor, string[] path, object data) {
            cursor.Refine(path).Update(data);
        }

        private class Cursor : IDataCursor {

            private readonly ISubject<object> _onUpdate;
            private readonly Func<object> _getData;
            private readonly Action<object> _updateData;
            private IDictionary<string, IDataCursor> _subCursors;

            public Cursor(Func<object> getData, Action<object> updateData) {
                _getData = getData;
                _updateData = updateData;
                _onUpdate = new ReplaySubject<object>(1);
                _onUpdate.OnNext(_getData());
            }

            public TData Get<TData>() {
                return (TData) _getData();
            }

            public void Update(object data) {
                var currentData = _getData();
                if (currentData is IDictionary<string, object>) {
                    throw new Exception("Not allowed to update complex type use Update(string key, object value) instead");
                }
                _updateData(data);
                _onUpdate.OnNext(data);
            }

            public IDataCursor Refine(string key) {
                if (_subCursors == null) {
                    _subCursors = new Dictionary<string, IDataCursor>();
                }
                IDataCursor subCursor;
                if (!_subCursors.TryGetValue(key, out subCursor)) {
                    subCursor = new Cursor(
                        getData: () => ((IDictionary<string, object>) _getData())[key],
                        updateData: newValue => {
                            var data = ((IDictionary<string, object>) _getData());
                            data[key] = newValue;
                            _updateData(data);
                            _onUpdate.OnNext(data);
                        });
                    _subCursors[key] = subCursor;
                }
                return subCursor;
            }

            public IObservable<object> OnUpdate {
                get {
                    return _onUpdate;
                }
            }
        }
    }

}
