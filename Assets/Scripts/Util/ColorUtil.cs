using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Util {

    public static class ColorUtil {

        public static Color FromRgb(int r, int g, int b) {
            return FromRgba(r, g, b, a: 255);
        }

        public static Color FromRgba(int r, int g, int b, int a) {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }
    }
}
