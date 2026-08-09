using System.Text;
using StringLeakTest;

namespace RamjetAnvil.Unity.Utility {
    public class MutableString {
        private const string InvisibleChar = "\u200B"; //ZERO WIDTH SPACE, http://www.fileformat.info/info/unicode/char/200b/browsertest.htm

        private readonly StringBuilder _builder;

        public MutableString(int capacity) {
            _builder = new StringBuilder(capacity);
            Clear();
        }

        public MutableString Append(int i) {
            _builder.Concat(i);
            return this;
        }

        public MutableString Append(float f, uint decimalPlaces) {
            _builder.Concat(f, decimalPlaces);
            return this;
        }

        public MutableString Append(MutableString s) {
            return Append(s.ToString(), s._builder.Length);
        }

        public MutableString Append(string s) {
            return Append(s, s.Length);
        }

        private MutableString Append(string s, int length) {
            _builder.Append(s, 0, length);
            return this;
        }

        public MutableString Clear() {
            _builder.Length = 0;
            for (int i = _builder.Length; i < _builder.Capacity; i++) {
                _builder.Append(InvisibleChar);
            }
            _builder.Length = 0;

            return this;
        }

        public override string ToString() {
            return _builder.ToString();
        }
    }
}
