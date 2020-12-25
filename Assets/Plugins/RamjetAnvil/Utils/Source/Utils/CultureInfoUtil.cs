using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility {
    public static class CultureInfoUtil {

        public static readonly Func<string, CultureInfo> GetCulture = Memoization.Memoize<string, CultureInfo>(languageCode => {
            return new CultureInfo(languageCode);
        });
    }
}
