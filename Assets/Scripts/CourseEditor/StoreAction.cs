using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.CourseEditor
{
    public struct StoreAction<T> {
        private readonly string _id;
        private readonly T _arguments;

        public StoreAction(string id, T arguments) {
            _id = id;
            _arguments = arguments;
        }

        public string Id {
            get { return _id; }
        }

        public T Arguments {
            get { return _arguments; }
        }
    }

    public static class StoreAction {

        public static Func<T, StoreAction<object>> Create<T>(string actionId) {
            return arguments => new StoreAction<object>(actionId, arguments);
        }
    }
}
