using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Volo.States;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    public class MainOptionsMenu : AbstractMenu {
        [SerializeField] private CustomizableButton _resume;
        [SerializeField] private CustomizableButton _restart;
        [SerializeField] private CustomizableButton _changeParachute;
        [SerializeField] private CustomizableButton _startSelection;
        [SerializeField] private CustomizableButton _options;
        [SerializeField] private CustomizableButton _tutorial;
        [SerializeField] private CustomizableButton _mainMenu;
        [SerializeField] private CustomizableButton _credits;
        [SerializeField] private CustomizableButton _followUs;
        [SerializeField] private CustomizableButton _quit;

        private IList<Selectable> _selectables; 

        public override void Initialize(OptionsMenuModel model) {
            _resume.OnSubmit.AddListener(() => model.CloseMenu(MenuActionId.Resume));
            _restart.OnSubmit.AddListener(() => model.CloseMenu(MenuActionId.Restart));
            _changeParachute.OnSubmit.AddListener(() => model.CloseMenu(MenuActionId.ChangeParachute));
            _startSelection.OnSubmit.AddListener(() => model.CloseMenu(MenuActionId.StartSelection));
            _mainMenu.OnSubmit.AddListener(() => model.CloseMenu(MenuActionId.MainMenu));
            _quit.OnSubmit.AddListener(() => model.CloseMenu(MenuActionId.Quit));

            _options.OnSubmit.AddListener(() => model.PushMenu(Menu.Options));
            _followUs.OnSubmit.AddListener(() => model.PushMenu(Menu.FollowUs));
            _tutorial.OnSubmit.AddListener(() => model.PushMenu(Menu.Tutorial));
            _credits.OnSubmit.AddListener(() => model.PushMenu(Menu.Credits));

            _selectables = GetComponentsInChildren<Selectable>(includeInactive: true)
                .ToList();
        }

        public override void SetState(OptionsMenuModel model, Func<string, string> l) {
            _resume.text = l("resume");

            _restart.text = l("restart");
            _restart.gameObject.SetActive(model.MenuState == MenuId.Playing);

            _changeParachute.gameObject.SetActive(model.MenuState == MenuId.Playing);
            _changeParachute.text = l("change_parachute");

            _startSelection.text = l("start_selection");
            _startSelection.gameObject.SetActive(model.MenuState == MenuId.Playing);

            _options.text = l("options");

            _tutorial.text = l("tutorial");

            _mainMenu.text = l("main_menu");
            _mainMenu.gameObject.SetActive(model.MenuState == MenuId.StartSelection);

            _credits.gameObject.SetActive(model.MenuState != MenuId.Playing);
            _credits.text = l("credits");

            _followUs.gameObject.SetActive(model.MenuState != MenuId.Playing);
            _followUs.text = l("follow_us");

            _quit.text = l("quit");

            UiNavigation.ResolveExplicitNavigation(_selectables);
        }

        public override Menu Id { get {return Menu.Main;} }
    }
}

