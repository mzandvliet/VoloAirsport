using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Volo.CourseEditing
{
    public struct Diff<T> {
        private readonly T _old;
        private readonly T _new;

        public Diff(T old, T @new) {
            _old = old;
            _new = @new;
        }

        public T Old {
            get { return _old; }
        }

        public T New {
            get { return _new; }
        }
    }
}
