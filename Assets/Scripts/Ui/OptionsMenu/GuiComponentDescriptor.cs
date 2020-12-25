using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Util;
using StringLeakTest;
using UnityEngine;

namespace Assets.Scripts.OptionsMenu {

    public abstract class GuiComponentDescriptor {

        public readonly string Title;
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }

        private GuiComponentDescriptor(string title) {
            Title = title;
            IsEnabled = true;
            IsVisible = true;
        }

        public abstract void UpdateDisplay();

        public sealed class Range : GuiComponentDescriptor {
            public float MinValue;
            public float MaxValue;
            public float StepSize;
            
            public Action<float> UpdateValue;

            private readonly Func<float> _currentValue;
            private readonly MutableString _displayValue;
            private readonly Action<Range, MutableString> _updateDisplayValue;

            public Range(string title, 
                float minValue, 
                float maxValue, 
                Action<float> updateValue,
                Func<float> currentValue,
                Action<Range, MutableString> updateDisplayValue,
                float stepSize = 1f,
                int mutableStringSize = 16) : base(title) {

                MinValue = minValue;
                MaxValue = maxValue;
                UpdateValue = updateValue;
                StepSize = stepSize;
                _currentValue = currentValue;
                _updateDisplayValue = updateDisplayValue;
                _displayValue = new MutableString(mutableStringSize);
            }

            public override void UpdateDisplay() {
                _displayValue.Clear();
                _updateDisplayValue(this, _displayValue);
            }

            public string DisplayValue {
                get { return _displayValue.ToString(); }
            }

            public float CurrentValue {
                get { return _currentValue(); }
            }
        }

        public sealed class List : GuiComponentDescriptor {
            private bool _isOnNextEnabled;
            private bool _isOnPrevEnabled;
            private string _displayValue;
            public readonly Action SelectNext;
            public readonly Action SelectPrev;

            private readonly Action _updateDisplayValue;

            public List(string title, string[] values, Action<int> updateIndex, Func<int> currentIndex) : base(title) {
                _updateDisplayValue = () => {
                    _isOnNextEnabled = currentIndex() < values.Length - 1 && values.Length > 1;
                    _isOnPrevEnabled = currentIndex() > 0 && values.Length > 1;
                    _displayValue = values[currentIndex()];
                };
                SelectNext = () => {
                    if (IsOnNextEnabled) {
                        var value = MathInt.Clamp(currentIndex() + 1, 0, values.Length - 1);
                        updateIndex(value);   
                    }
                };
                SelectPrev = () => {
                    if (_isOnPrevEnabled) {
                        var value = MathInt.Clamp(currentIndex() - 1, 0, values.Length - 1);
                        updateIndex(value);   
                    }
                };
            }

            public override void UpdateDisplay() {
                _updateDisplayValue();
            }

            public string CurrentValue {
                get { return _displayValue; }
            }

            public bool IsOnNextEnabled {
                get { return _isOnNextEnabled; }
            }

            public bool IsOnPrevEnabled {
                get { return _isOnPrevEnabled; }
            }
        }

        public sealed class Boolean : GuiComponentDescriptor {
            private readonly Func<bool> _isChecked;
            public readonly Action<bool> UpdateValue;

            private readonly Action<Boolean, bool> _updateDisplayValue;

            public Boolean(string title, Action<bool> updateValue, Func<bool> isChecked, Action<Boolean, bool> updateDisplay = null) : base(title) {
                _isChecked = isChecked;
                UpdateValue = updateValue;
                _updateDisplayValue = updateDisplay ?? ((descriptor, value) => { });
            }

            public override void UpdateDisplay() {
                _updateDisplayValue(this, IsChecked);
            }

            public bool IsChecked {
                get { return _isChecked(); }
            }
        }

        public sealed class TextInput : GuiComponentDescriptor {

            private readonly int _maxLength;
            private readonly Func<string> _currentValue;
            private readonly Action<string> _updateValue;

            public TextInput(string title, Func<string> value, Action<string> updateValue, 
                int maxLength) : base(title) {

                _currentValue = value;
                _maxLength = maxLength;
                _updateValue = updateValue;
            }

            public void UpdateValue(string newValue) {
                newValue = newValue.Limit(_maxLength, trailingChars: "");
                _updateValue(newValue);
            }

            public override void UpdateDisplay() {
            }

            public string CurrentValue {
                get { return _currentValue(); }
            }
        }

        public static void DisplayNumber(float value, MutableString displayValue, uint decimalPlaces = 0, string postFix = "") {
            //var numberFormat = MutableString.NumberFormat(decimalPlaces);
            //if (decimalPlaces > 0) {
                displayValue.Append(value, decimalPlaces);
            //} else {
              //  displayValue.Builder.Concat(Mathf.RoundToInt(value));
            //}
            displayValue.Append(postFix);
        }

        public static GuiComponentDescriptor[] FindDescriptors(object @object) {
            var fields = @object.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            return fields
                .Select(field => field.GetValue(@object) as GuiComponentDescriptor)
                .Where(fieldValue => fieldValue != null)
                .ToArray();
        }

        public static string[] GetEnumStrings<TEnum>(string prefix = null) where TEnum : struct, IComparable, IConvertible, IFormattable {
            return EnumUtils.GetValues<TEnum>().Select(e => GetEnumString(e, prefix)).ToArray();
        }

        public static string GetEnumString<TEnum>(TEnum e, string prefix = null) {
            var s = e.ToString().ToUnderscoreCase();
            if (prefix != null) {
                s = prefix + "_" + s;
            }
            return s;
        }

        public static void DisplayPercentage(float value, MutableString str) {
            str.Append(Mathf.RoundToInt(value * 100f));
            str.Append("%");
        }
    }
}
