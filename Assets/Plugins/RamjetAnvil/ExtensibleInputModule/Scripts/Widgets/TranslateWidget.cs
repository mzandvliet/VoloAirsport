using System;
using UnityEngine;

namespace RamjetAnvil.InputModule {
    public class TranslateWidget : MonoBehaviour {
        [SerializeField] private SingleAxisTranslateWidget _xWidget;
        [SerializeField] private SingleAxisTranslateWidget _yWidget;
        [SerializeField] private SingleAxisTranslateWidget _zWidget;

        private InputRange _inputRange;

        void Awake() {
            _xWidget.OnPositionChanged += newValue => OnUpdateAxis(newValue, Axis.X);
            _yWidget.OnPositionChanged += newValue => OnUpdateAxis(newValue, Axis.Y);
            _zWidget.OnPositionChanged += newValue => OnUpdateAxis(newValue, Axis.Z);
        }

        public void UpdateState() {
            SetPosition(_inputRange.Value);
        }

        public InputRange InputRange {
            get { return _inputRange; }
            set {
                _inputRange = value;

                if ((_inputRange.ActiveAxes & Axis.X) != 0) {
                    _xWidget.DragSpeed = RangeToSpeed(_inputRange.XAxisDescription);
                    _xWidget.gameObject.SetActive(true);
                } else {
                    _xWidget.gameObject.SetActive(false);
                }

                if ((_inputRange.ActiveAxes & Axis.Y) != 0) {
                    _yWidget.DragSpeed = RangeToSpeed(_inputRange.YAxisDescription);
                    _yWidget.gameObject.SetActive(true);
                } else {
                    _yWidget.gameObject.SetActive(false);
                }

                if ((_inputRange.ActiveAxes & Axis.Z) != 0) {
                    _zWidget.DragSpeed = RangeToSpeed(_inputRange.ZAxisDescription);
                    _zWidget.gameObject.SetActive(true);
                } else {
                    _zWidget.gameObject.SetActive(false);
                }

                SetPosition(_inputRange.Value);
            }
        }

        private float RangeToSpeed(Range range) {
            var valueRange = range.Max - range.Min;
            return valueRange / 10;
        }

        private void SetPosition(Vector3 newPosition) {
            _xWidget.SetPosition(newPosition);
            _yWidget.SetPosition(newPosition);
            _zWidget.SetPosition(newPosition);
        }

        private void OnUpdateAxis(Vector3 newValue, Axis axis) {
            if (_inputRange != null) {
                var newPosition = _inputRange.Value;
                switch (axis) {
                    case Axis.X:
                        newPosition.x = newValue.x;
                        break;
                    case Axis.Y:
                        newPosition.y = newValue.y;
                        break;
                    case Axis.Z:
                        newPosition.z = newValue.z;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("axis", axis, null);
                }

                _inputRange.SetValue(newPosition);
            }
        }
    }
}
