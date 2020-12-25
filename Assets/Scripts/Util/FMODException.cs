using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Volo.Util {
    public class FMODException : Exception {

        private readonly FMOD.RESULT _resultCode;

        public FMODException(string message, FMOD.RESULT resultCode) : base(message) {
            _resultCode = resultCode;
        }

        public override string ToString() {
            return base.ToString() + " (Result code '" + _resultCode + "')";
        }
    }
}
