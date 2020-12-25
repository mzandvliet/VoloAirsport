using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;
using RamjetAnvil.Unity.Utility;
using StringLeakTest;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RamjetAnvil.Volo.Ui {

    public class GuiComponentPrefabs : MonoBehaviour {
        [SerializeField] private GameObject _titlePrefab;

        [SerializeField] private GameObject _rangeValuePrefab;
        [SerializeField] private GameObject _checkBoxPrefab;
        [SerializeField] private GameObject _enumListPrefab;
        [SerializeField] private GameObject _textInputPrefab;

        public IComponentRenderer CreateGuiComponent(
            GuiComponentDescriptor componentDescriptor, 
            SettingsView parent) {

            IComponentRenderer renderer;
            GameObject renderable;
            if (componentDescriptor is GuiComponentDescriptor.Range) {
                renderable = Instantiate(_rangeValuePrefab);
                var range = renderable.GetComponent<Range>();
                renderer = new RangeRenderer(
                    range,
                    componentDescriptor as GuiComponentDescriptor.Range);
            } else if (componentDescriptor is GuiComponentDescriptor.List) {
                renderable = Instantiate(_enumListPrefab);
                var enumList = renderable.GetComponentInChildren<EnumList>();
                renderer = new EnumListRenderer(
                    enumList,
                    componentDescriptor as GuiComponentDescriptor.List);
            } else if (componentDescriptor is GuiComponentDescriptor.Boolean) {
                renderable = Instantiate(_checkBoxPrefab);
                var checkBox = renderable.GetComponentInChildren<Checkbox>();
                renderer = new CheckboxRenderer(
                    checkBox,
                    componentDescriptor as GuiComponentDescriptor.Boolean);
            } else if (componentDescriptor is GuiComponentDescriptor.TextInput) {
                renderable = Instantiate(_textInputPrefab);
                var textInput = renderable.GetComponent<InputField>();
                renderer = new TextInputRenderer(
                    textInput,
                    componentDescriptor as GuiComponentDescriptor.TextInput);
            } else {
                throw new ArgumentException("No renderer for type " + componentDescriptor.GetType());
            }

            var title = Instantiate(_titlePrefab);
            parent.AddWidget(title, renderable);

            return new EntryRenderer(componentDescriptor, title.GetComponent<EntryTitle>(), renderer, renderable);
        }
    }

    public class EntryRenderer : IComponentRenderer {

        private readonly EntryTitle _titleRenderer;
        private readonly GuiComponentDescriptor _descriptor;
        private readonly GameObject _valueGameObject;
        private readonly IComponentRenderer _valueRenderer;

        public EntryRenderer(GuiComponentDescriptor descriptor, EntryTitle titleRenderer, IComponentRenderer valueRenderer, GameObject valueGameObject) {
            _titleRenderer = titleRenderer;
            _valueRenderer = valueRenderer;
            _valueGameObject = valueGameObject;
            _descriptor = descriptor;
        }

        public void Update(Func<string, string> l) {
            _valueRenderer.Update(l);

            _titleRenderer.SetText(l(_descriptor.Title));
            _titleRenderer.gameObject.SetActive(_descriptor.IsVisible);
            _valueGameObject.SetActive(_descriptor.IsVisible);

            var navigation = _valueRenderer.NavigationElement.navigation;
            navigation.mode = _descriptor.IsEnabled && _descriptor.IsVisible ? Navigation.Mode.Explicit : Navigation.Mode.None;
            _valueRenderer.NavigationElement.navigation = navigation;
        }

        public Selectable NavigationElement {
            get { return _valueRenderer.NavigationElement; }
        }
    }

    

    // TODO Find a way to update the model from these renderers

    public interface IComponentRenderer {
        Selectable NavigationElement { get; }
        void Update(Func<string, string> l);
    }

    public class RangeRenderer : IComponentRenderer {

        private readonly Range _range;
        private readonly GuiComponentDescriptor.Range _descriptor;

        public RangeRenderer(Range range, GuiComponentDescriptor.Range descriptor) {
            _range = range;
            _descriptor = descriptor;
            _range.Init(CurrentProps);

            _range.OnValueChanged += rawValue => {
                //Debug.Log("updating slider value " + value);
                // Prevent feedback loop due to slider value update being triggered
                // when updated with the same value
                // Adhere to the next value
                if (Math.Abs(_descriptor.CurrentValue - rawValue) > float.Epsilon) {
                    var newValue = Mathf.Clamp(rawValue, _descriptor.MinValue, _descriptor.MaxValue);
                    _descriptor.UpdateValue(newValue);    
                }
            };
        }

        public void Update(Func<string, string> l) {
            _descriptor.UpdateDisplay();

            _range.SetEnabled(_descriptor.IsEnabled);

            _range.SetSliderValues(CurrentProps);
            _range.SetDisplayValue(_descriptor.DisplayValue);
        }

        private Range.SliderProps CurrentProps {
            get {
                return new Range.SliderProps {
                    Min = _descriptor.MinValue,
                    Max = _descriptor.MaxValue,
                    Value = _descriptor.CurrentValue,
                    StepSize = _descriptor.StepSize
                };
            }
        }

        public Selectable NavigationElement {
            get { return _range.NavigationElement; }
        }
    }

    public class CheckboxRenderer : IComponentRenderer {

        private readonly Checkbox _checkBox;
        private readonly GuiComponentDescriptor.Boolean _descriptor;

        public CheckboxRenderer(Checkbox checkBox, GuiComponentDescriptor.Boolean descriptor) {
            _checkBox = checkBox;
            _descriptor = descriptor;
            
            _checkBox.OnValueChanged += () => _descriptor.UpdateValue(!descriptor.IsChecked);
        }

        public void Update(Func<string, string> l) {
            _descriptor.UpdateDisplay();

            _checkBox.gameObject.SetActive(_descriptor.IsVisible);
            _checkBox.SetEnabled(_descriptor.IsEnabled);

            if (_descriptor.IsChecked) {
                _checkBox.Check();
            } else {
                _checkBox.Uncheck();
            }
        }

        public Selectable NavigationElement {
            get { return _checkBox.NavigationElement; }
        }
    }

    public class EnumListRenderer : IComponentRenderer {

        private readonly EnumList _enumList;
        private readonly GuiComponentDescriptor.List _descriptor;

        public EnumListRenderer(EnumList enumList, GuiComponentDescriptor.List descriptor) {
            _enumList = enumList;
            _descriptor = descriptor;

            _enumList.OnNext += _descriptor.SelectNext;
            _enumList.OnPrev += _descriptor.SelectPrev;
        }

        public void Update(Func<string, string> l) {
            _descriptor.UpdateDisplay();

            _enumList.SetEnabled(_descriptor.IsEnabled);

            _enumList.SetText(l(_descriptor.CurrentValue));

            if (_descriptor.IsOnNextEnabled) {
                _enumList.EnableOnNext();
            } else {
                _enumList.DisableOnNext();
            }

            if (_descriptor.IsOnPrevEnabled) {
                _enumList.EnableOnPrev();
            } else {
                _enumList.DisableOnPrev();
            }
        }

        public Selectable NavigationElement { get { return _enumList; } }
    }

    public class TextInputRenderer : IComponentRenderer {

        private readonly InputField _textInput;
        private readonly GuiComponentDescriptor.TextInput _descriptor;

        public TextInputRenderer(InputField textInput, 
            GuiComponentDescriptor.TextInput descriptor) {

            _textInput = textInput;
            _descriptor = descriptor;

            _textInput.onValueChanged.AddListener(value => {
                if (_descriptor.CurrentValue != value) {
                    _descriptor.UpdateValue(value);    
                }
            });
        }

        public Selectable NavigationElement {
            get { return _textInput; }
        }

        public void Update(Func<string, string> l) {
            _descriptor.UpdateDisplay();
            Debug.Log("updating text to " + _descriptor.CurrentValue);
            _textInput.text = _descriptor.CurrentValue;
        }
    }
}
