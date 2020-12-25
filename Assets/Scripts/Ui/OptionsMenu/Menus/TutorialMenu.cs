using System;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class TutorialMenu : AbstractMenu {

        [SerializeField] private Text _title;
        [SerializeField] private ClickableUrl _link;
        [SerializeField] private CustomizableButton _back;

        public override void Initialize(OptionsMenuModel model) {
            _back.OnSubmit.AddListener(model.PopMenu);
        }

        public override void SetState(OptionsMenuModel model, Func<string, string> l) {
            _title.text = l("tutorial");

            //https://www.youtube.com/watch?v=jen7PgR_Ris&hl=en&cc_lang_pref=en&cc_load_policy=1
            var languageCode = l("language_code");
            var isSubtitleRequired = languageCode != "en";
            var url = "https://www.youtube.com/watch?v=jen7PgR_Ris";
            url += "&hl=" + languageCode;
            url += "&cc_lang_pref=" + languageCode;
            url += isSubtitleRequired ? "&cc_load_policy=1" : "";
            _link.Url = url;
            
            _back.text = l("back");
        }

        public override Menu Id {
            get { return Menu.Tutorial; }
        }
    }
}
