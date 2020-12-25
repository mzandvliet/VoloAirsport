using System;
using UnityEngine;

namespace RamjetAnvil.InputModule {

    public class Range {
        public readonly float Min;
        public readonly float Max;
        private readonly Func<float> _getValue;
        private readonly Action<float> _updateValue;
        public readonly float StepSize;

        public Range(float min, float max, Func<float> getValue, Action<float> updateValue, int numberOfSteps) {
            Debug.Assert(min < max);
            Min = min;
            Max = max;
            _getValue = getValue;
            _updateValue = updateValue;
            StepSize = (Max - Min) / numberOfSteps;
        }

        public float Value {
            get { return Mathf.Clamp(_getValue(), Min, Max); }
        }

        public float UnitValue {
            get { return Mathf.InverseLerp(Min, Max, Value); }
        }

        public void Update(float value) {
            _updateValue(Mathf.Clamp(value, Min, Max));
        }

        public void UpdateFromUnitValue(float normalizedValue) {
            _updateValue(Mathf.Lerp(Min, Max, Mathf.Clamp(normalizedValue, 0, 1)));
        }
    }

    public class InputRange {
        public readonly Range XAxisDescription;
        public readonly Range YAxisDescription;
        public readonly Range ZAxisDescription;

        public InputRange(Range xAxisDescription = null, Range yAxisDescription = null, Range zAxisDescription = null) {
            XAxisDescription = xAxisDescription;
            YAxisDescription = yAxisDescription;
            ZAxisDescription = zAxisDescription;
        }

        public Vector3 UnitValue {
            get {
                return new Vector3(
                    x: XAxisDescription != null ? XAxisDescription.UnitValue : 0f,
                    y: YAxisDescription != null ? YAxisDescription.UnitValue : 0f,
                    z: ZAxisDescription != null ? ZAxisDescription.UnitValue : 0f);
            }
        }

        public Vector3 Value {
            get {
                return new Vector3(
                    x: XAxisDescription != null ? XAxisDescription.Value : 0,
                    y: YAxisDescription != null ? YAxisDescription.Value : 0,
                    z: ZAxisDescription != null ? ZAxisDescription.Value : 0);
            }
        }

        public void SetUnitValue(Vector3 unitValue) {
            if (XAxisDescription != null) {
                XAxisDescription.UpdateFromUnitValue(unitValue.x);
            }
            if (YAxisDescription != null) {
                YAxisDescription.UpdateFromUnitValue(unitValue.y);
            }
            if (ZAxisDescription != null) {
                ZAxisDescription.UpdateFromUnitValue(unitValue.z);
            }
        }

        public void SetValue(Vector3 value) {
            if (XAxisDescription != null) {
                XAxisDescription.Update(value.x);
            }
            if (YAxisDescription != null) {
                YAxisDescription.Update(value.y);
            }
            if (ZAxisDescription != null) {
                ZAxisDescription.Update(value.z);
            }
        }

        public bool ContainsAxes(Axis axes) {
            return (ActiveAxes & axes) == ActiveAxes;
        }

        public Axis ActiveAxes {
            get {
                return (XAxisDescription != null ? Axis.X : 0) |
                       (YAxisDescription != null ? Axis.Y : 0) |
                       (ZAxisDescription != null ? Axis.Z : 0);
            }
        }
    }
}
