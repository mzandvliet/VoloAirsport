using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class FollowUsMenu : AbstractMenu {

        [SerializeField] private Text _title;
        [SerializeField] private Text _visitHomepage;
        [SerializeField] private Text _talkAboutVolo;
        [SerializeField] private Text _stayUpToDate;
        [SerializeField] private CustomizableButton _back;

        public override void Initialize(OptionsMenuModel model) {
            _back.OnSubmit.AddListener(model.PopMenu);
        }

        public override void SetState(OptionsMenuModel model, Func<string, string> l) {
            _title.text = l("follow_us");
            _visitHomepage.text = l("visit_the_homepage");
            _talkAboutVolo.text = l("talk_about_volo");
            _stayUpToDate.text = l("stay_up_to_date");
            _back.text = l("back");
        }

        public override Menu Id {
            get { return Menu.FollowUs; }
        }
    }
}
