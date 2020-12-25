using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RamjetAnvil.Volo.Util {
    public static class StringUtils {

        public static string ReplaceLast(this string source, string pattern, string replacement) {
            int place = source.LastIndexOf(pattern, System.StringComparison.Ordinal);

            if (place == -1) {
                return source;
            }
                
            return source.Remove(place, pattern.Length).Insert(place, replacement);
        }

        public static string Limit(this string s, int maxLength, string trailingChars = "...") {
            if (s.Length > maxLength) {
                return s.Remove(maxLength - 1, s.Length - maxLength) + trailingChars;
            } else {
                return s;
            }
        }

        public static string Capitalize(this string s) {
            return s.Substring(0, 1).ToUpper() + s.Substring(1, s.Length - 1).ToLower();
        }

        public static string ToUpperCamelCase(this string s) {
            return s.Substring(0, 1).ToUpper() + s.Substring(1, s.Length - 1);
        }

        public static string ToLowerCamelCase(this string s) {
            return s.Substring(0, 1).ToLower() + s.Substring(1, s.Length - 1);
        }

        public static string ToUnderscoreCase(this string s) {
            s = Regex.Replace(s, @"(?<=[a-z])([A-Z])", @"_$1");
            s = s.ToLowerInvariant();
            return s;
        }

        public static string Repeat(this string s, int times) {
            string result = "";
            for (int i = 0; i < times; i++) {
                result += s;
            }
            return result;
        }
    }
}
