using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class AudioMenu : AbstractMenu {

        [SerializeField] private Text _title;
        [SerializeField] private SettingsContainer _settingsContainer;
        [SerializeField] private CustomizableButton _restoreDefaults;
        [SerializeField] private CustomizableButton _back;

        private IList<Selectable> _selectables;
             
        public override void Initialize(OptionsMenuModel model) {
            _settingsContainer.Initialize(GuiComponentDescriptor.FindDescriptors(model.AudioSettings));
            _restoreDefaults.OnSubmit.AddListener(model.RestoreAudioDefaults);
            _back.OnSubmit.AddListener(model.PopMenu);

            _selectables = _settingsContainer.GuiComponents
                .Select(g => g.NavigationElement)
                .Concat(new[] { _restoreDefaults.Button, _back.Button })
                .ToList();
        }

        public override void SetState(OptionsMenuModel model, Func<string, string> l) {
            _title.text = l("audio");
            _settingsContainer.SetState(l);
            _restoreDefaults.text = l("restore_defaults");
            _back.text = l("back");

            UiNavigation.ResolveExplicitNavigation(_selectables);
        }

        public override Menu Id {
            get { return Menu.Audio; }
        }
    }
}
