using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.RamNet {
    public class TransporterClosedException : Exception {
        public TransporterClosedException(string message) : base(message) {}
    }
}
