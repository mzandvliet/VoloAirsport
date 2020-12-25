using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility {

    public struct SerializeTask<T> {
        public string FilePath;
        public T SerializableValue;
    }
}
