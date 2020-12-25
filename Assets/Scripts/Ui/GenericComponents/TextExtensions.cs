using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public static class TextExtensions {

        public static void SetMutableString(this Text t, MutableString s) {
            t.SetMutableString(s.ToString());
        }

        public static void SetMutableString(this Text t, string s) {
            t.text = " ";
            t.text = s;
            t.cachedTextGenerator.Invalidate();
        }
    }
}
