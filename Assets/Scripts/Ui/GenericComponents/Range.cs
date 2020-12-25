using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class Range : MonoBehaviour {

        public event Action<float> OnValueChanged;

        [SerializeField] private ImprovedSlider _slider;
        [SerializeField] private Text _displayValue;

        void Awake() {

        }
        
        public void SetEnabled(bool isEnabled) {
            _slider.interactable = isEnabled;
        }

        public void Init(SliderProps props) {
            SetSliderValues(props);
            _slider.onValueChanged.AddListener(value => {
//                Debug.Log("value changed " + value);
                if (OnValueChanged != null) {   
                    OnValueChanged(value);
                }
            });
        }

        public void SetSliderValues(SliderProps sliderProps) {
            _slider.minValue = sliderProps.Min;
            _slider.maxValue = sliderProps.Max;
            _slider.value = sliderProps.Value;    
            //_slider.wholeNumbers = sliderProps.StepSize % 1f < float.Epsilon;
            _slider.stepSize = sliderProps.StepSize;
        }

        public void SetDisplayValue(string text) {
            _displayValue.SetMutableString(text);
        }

        public Selectable NavigationElement {
            get { return _slider; }
        }

        public struct SliderProps {
            public float Min;
            public float Max;
            public float Value;
            public float StepSize;
        }
    }
}
