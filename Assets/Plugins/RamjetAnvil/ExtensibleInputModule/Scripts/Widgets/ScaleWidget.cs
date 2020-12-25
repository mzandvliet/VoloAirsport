using System;
using UnityEngine;

namespace RamjetAnvil.InputModule {
    public class ScaleWidget : MonoBehaviour {
        [SerializeField] private SingleAxisScaleWidget _xWidget;
        [SerializeField] private SingleAxisScaleWidget _yWidget;
        [SerializeField] private SingleAxisScaleWidget _zWidget;

        private InputRange _inputRange;

        void Awake() {
            _xWidget.OnSizeChanged += newValue => OnUpdateAxis(newValue, Axis.X);
            _yWidget.OnSizeChanged += newValue => OnUpdateAxis(newValue, Axis.Y);
            _zWidget.OnSizeChanged += newValue => OnUpdateAxis(newValue, Axis.Z);
        }

        public void UpdateState() {
            SetSize(_inputRange.UnitValue);
        }

        public InputRange InputRange {
            get { return _inputRange; }
            set {
                _inputRange = value;

                _xWidget.gameObject.SetActive((_inputRange.ActiveAxes & Axis.X) != 0);
                _yWidget.gameObject.SetActive((_inputRange.ActiveAxes & Axis.Y) != 0);
                _zWidget.gameObject.SetActive((_inputRange.ActiveAxes & Axis.Z) != 0);
                SetSize(_inputRange.UnitValue);
            }
        }

        private void SetSize(Vector3 newSize) {
            _xWidget.SetSize(newSize.x);
            _yWidget.SetSize(newSize.y);
            _zWidget.SetSize(newSize.z);
        }

        private void OnUpdateAxis(float newValue, Axis axis) {
            //Debug.Log("update axis " + newValue + ", axis " + axis);
            if (_inputRange != null) {
                var newSize = _inputRange.UnitValue;
                switch (axis) {
                    case Axis.X:
                        newSize.x = newValue;
                        break;
                    case Axis.Y:
                        newSize.y = newValue;
                        break;
                    case Axis.Z:
                        newSize.z = newValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("axis", axis, null);
                }

                _inputRange.SetUnitValue(newSize);
            }
        }
    }
}
