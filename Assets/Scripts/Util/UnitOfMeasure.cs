using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.Util.UnitOfMeasure {

    public struct Measure<T> {
        public readonly T Value;
        public readonly string Unit;

        public Measure(T value, string unit) {
            Value = value;
            Unit = unit;
        }

        public string Format(int precision) {
            if (precision < 1) {
                return string.Format("{0:0} {1}", Value, Unit);
            } 
            return string.Format("{0:0." + "#".Repeat(precision) + "} {1}", Value, Unit);
        }

        public override string ToString() {
            return Format(precision: 0);
        }
    }

    public static class MeasureExtensions {
        public static MutableString FormatTo(this Measure<float> measure, MutableString s, int precision) {
            return s.Append(measure.Value, (uint) precision)
                .Append(" ")
                .Append(measure.Unit);
        }

        public static MutableString FormatTo(this Measure<int> measure, MutableString s) {
            return s.Append(measure.Value)
                .Append(" ")
                .Append(measure.Unit);
        }
    }

    /*
     * Algebraic data type: UnitSystem = Metric | Imperial
     */
    public class MeasureSystem {
        private MeasureSystem() {}
        public sealed class Metric : MeasureSystem {}
        public sealed class Imperial : MeasureSystem {}

        public static Vector3 MetersToFeet(Vector3 meters) {
            return new Vector3(MetersToFeet(meters.x), MetersToFeet(meters.y), MetersToFeet(meters.z));
        }

        public static Vector3 KmhToMph(Vector3 kmh) {
            return new Vector3(KilometersToMiles(kmh.x), KilometersToMiles(kmh.y), KilometersToMiles(kmh.z));
        }

        public static float KilometersToMiles(float kilometers) {
            return kilometers * 0.621f;
        }

        public static float MetersToFeet(float meters) {
            return meters * 3.281f;
        }

        public static int MetersToFeet(int meters) {
            return Mathf.RoundToInt(meters * 3.281f);
        }

        public static float KilogramsToOunces(float kilograms) {
            return kilograms * 35.274f;
        }

        public static float KilogramsToPounds(float kilograms) {
            return kilograms * 2.205f;
        }
    }

}
