using System;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class CreditsMenu : AbstractMenu {

        [SerializeField] private Text _title;
        [SerializeField] private CustomizableButton _back;
        [SerializeField] private LocalizedText[] _localizedFields;

        public override void Initialize(OptionsMenuModel model) {
            _back.OnSubmit.AddListener(model.PopMenu);
        }

        public override void SetState(OptionsMenuModel model, Func<string, string> l) {
            _title.text = l("credits");
            for (int i = 0; i < _localizedFields.Length; i++) {
                var localizedField = _localizedFields[i];
                localizedField.UpdateLocalization(l);
            }
            _back.text = l("back");
        }

        public override Menu Id {
            get { return Menu.Credits; }
        }
    }
}
