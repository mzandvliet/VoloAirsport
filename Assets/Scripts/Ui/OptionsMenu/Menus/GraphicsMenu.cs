using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class GraphicsMenu : AbstractMenu {

        [SerializeField] private Text _title;
        [SerializeField] private SettingsContainer _settingsContainer;
        [SerializeField] private Text _pleaseRestart;
        [SerializeField] private CustomizableButton _back;
        [SerializeField] private CustomizableButton _apply;
        [SerializeField] private CustomizableButton _restoreDefaults;

        private IList<Selectable> _selectables; 

        public override void Initialize(OptionsMenuModel model) {
            _settingsContainer.Initialize(GuiComponentDescriptor.FindDescriptors(model.GraphicsSettings));
            _restoreDefaults.OnSubmit.AddListener(model.RestoreGraphicsDefaults);
            _back.OnSubmit.AddListener(model.PopMenu);
            _apply.OnSubmit.AddListener(model.ApplyGraphicsSettings);

            _selectables = _settingsContainer.GuiComponents
                .Select(g => g.NavigationElement)
                .Concat(new[] { _restoreDefaults.Button, _apply.Button, _back.Button })
                .ToList();
        }

        public override void SetState(OptionsMenuModel model, Func<string, string> l) {
            _title.text = l("graphics");
            _pleaseRestart.text = l("please_restart");
            if (model.NeedsRestart) {
                ShowText(_pleaseRestart);
            } else {
                HideText(_pleaseRestart);
            }
            _settingsContainer.SetState(l);
            _restoreDefaults.text = l("restore_defaults");
            _apply.text = l("apply");
            _apply.gameObject.SetActive(model.IsApplyGraphicsSettingsRequired);
            _back.text = l("back");

            UiNavigation.ResolveExplicitNavigation(_selectables);
        }

        public override Menu Id {
            get { return Menu.Graphics; }
        }

        private void HideText(Text text) {
            var color = text.color;
            color.a = 0;
            text.color = color;
        }

        private void ShowText(Text text) {
            var color = text.color;
            color.a = 255;
            text.color = color;
        }
    }
}
