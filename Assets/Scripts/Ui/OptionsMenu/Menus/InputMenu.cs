using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RamjetAnvil.Volo.Ui {
    public class InputMenu : AbstractMenu {

        [SerializeField] private GameObject _inputBindingPrefab;

        [SerializeField] private Text _title;
        [SerializeField] private SettingsContainer _settingsContainer;
        [SerializeField] private GameObject _inputBindingsContainer;
        [SerializeField] private CustomizableButton _back;
        [SerializeField] private CustomizableButton _restoreDefaults;

        private bool _isInputBindingsInitialized;
        private IList<InputBindingView> _inputBindingViews;

        private IList<Selectable> _selectables; 

        public override void Initialize(OptionsMenuModel model) {
            _settingsContainer.Initialize(GuiComponentDescriptor.FindDescriptors(model.InputSettings));
            _restoreDefaults.OnSubmit.AddListener(model.RestoreInputDefaults);
            _back.OnSubmit.AddListener(model.PopMenu);
        }

        public override void SetState(OptionsMenuModel model, Func<string, string> l) {
            _title.text = l("input");

            if (model.InputBindings != null) {
                if (!_isInputBindingsInitialized) {
                    _inputBindingViews = new InputBindingView[model.InputBindings.Length];
                    for (int i = 0; i < model.InputBindings.Length; i++) {
                        var inputBinding = model.InputBindings[i];
                        var inputBindingView = Instantiate(_inputBindingPrefab).GetComponent<InputBindingView>();
                        inputBindingView.transform.SetParent(_inputBindingsContainer.transform, worldPositionStays: false);
                        inputBindingView.transform.localScale = Vector3.one;
                        inputBindingView.OnSubmit += () => model.StartRebind(inputBinding);
                        _inputBindingViews[i] = inputBindingView;
                    }
                    var bindingViews = _inputBindingViews.Select(v => v as Selectable);
                    var settingViews = _settingsContainer.GuiComponents
                        .Select(g => g.NavigationElement);
                    var buttons = new[] {
                        _restoreDefaults.Button,
                        _back.Button
                    };
                    _selectables = bindingViews.Concat(settingViews).Concat(buttons).ToList();
                    _isInputBindingsInitialized = true;
                }

                var rebindingText = l("press_to_bind") + " (" + l("press_escape_to_cancel") + ")";
                for (int i = 0; i < model.InputBindings.Length; i++) {
                    var inputBinding = model.InputBindings[i];
                    var inputBindingView = _inputBindingViews[i];
                    var isRebinding = model.Rebinding.HasValue && model.Rebinding.Value.Id == inputBinding.Id;
                    inputBindingView.SetState(inputBinding, isRebinding, rebindingText);
                }
            }

            _settingsContainer.SetState(l);

            _back.text = l("back");
            _restoreDefaults.text = l("restore_defaults");

            UiNavigation.ResolveExplicitNavigation(_selectables);
        }

        public override Menu Id {
            get { return Menu.Input; }
        }

    }
}
