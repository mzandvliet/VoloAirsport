using System;
using System.Reflection;
using System.Text;
using StringLeakTest;

namespace RamjetAnvil.Unity.Utility {
    public class MutableString { 
        private const string InvisibleChar = "\u200B"; //ZERO WIDTH SPACE, http://www.fileformat.info/info/unicode/char/200b/browsertest.htm
        
        private readonly StringBuilder _builder;

        private string _string;

        public MutableString(int capacity) {
            _builder = new StringBuilder(capacity);
            _string = GarbageFreeString(_builder);
            Clear();
        }

        public MutableString Append(int i) {
            var prevCapacity = _builder.Capacity;
            _builder.Concat(i);
            ExpandIfNecessary(prevCapacity);
            return this;
        }

        public MutableString Append(float f, uint decimalPlaces) {
            var prevCapacity = _builder.Capacity;
            _builder.Concat(f, decimalPlaces);
            ExpandIfNecessary(prevCapacity);
            return this;
        }

        public MutableString Append(MutableString s) {
            return Append(s.ToString(), s._builder.Length);
        }

        public MutableString Append(string s) {
            return Append(s, s.Length);
        }

        private MutableString Append(string s, int length) {
            var currentCapacity = _builder.Capacity;

            _builder.Append(s, 0, length);

            // If builder capacity is expanded make sure to 
            // store the newly allocated string from the builder
            ExpandIfNecessary(currentCapacity);

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
            return _string;
        }

        public static string GarbageFreeString(StringBuilder sb) {
            return (string) 
                sb.GetType()
                    .GetField("_str", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(sb);
        }

        private void ExpandIfNecessary(int prevCapacity) {
            if (prevCapacity != _builder.Capacity) {
                var currentLength = _builder.Length;
                for (int i = _builder.Length; i < _builder.Capacity; i++) {
                    _builder.Append(InvisibleChar);
                }
                _builder.Length = currentLength;

                _string = GarbageFreeString(_builder);
            }
        }
    }
}