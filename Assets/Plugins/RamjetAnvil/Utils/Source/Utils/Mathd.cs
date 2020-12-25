using System;

namespace RamjetAnvil.Unity.Utility {
    public static class Mathd {
        public static double Clamp(double value, double min, double max) {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        public static double Clamp01(double value) {
            if (value < 0d)
                return 0d;
            if (value > 1d)
                return 1d;
        
            return value;
        }

        public static double Min(double a, double b) {
            return a < b ? a : b;
        }

        public static double Max(double a, double b) {
            return a > b ? a : b;
        }

        public static double ToMillis(double seconds) {
            return Math.Round(seconds * 1000d, 2);
        }

        public static double Lerp(double a, double b, double t) {
            return a + (b - a) * Clamp01(t);
        }

        public static double InverseLerp(double a, double b, double value) {
            if (a != b)
                return Clamp01((value - a) / (b - a));
            return 0d;
        }

        public static double Repeat(double t, double length) {
            return t - Math.Floor(t / length) * length;
        }

        /// <summary>
        /// 
        /// <para>
        /// PingPongs the value t, so that it is never larger than length and never smaller than 0.
        /// </para>
        /// 
        /// </summary>
        /// <param name="t"/><param name="length"/>
        public static double PingPong(double t, double length) {
            t = Repeat(t, length * 2d);
            return length - Math.Abs(t - length);
        }
    }
}
