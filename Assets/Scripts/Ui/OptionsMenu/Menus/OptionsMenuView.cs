using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class OptionsMenuView : AbstractMenu {

        [SerializeField] private Text _title;
        [SerializeField] private CustomizableButton _timeAndWeather;
        [SerializeField] private CustomizableButton _gameplay;
        [SerializeField] private CustomizableButton _input;
        [SerializeField] private CustomizableButton _graphics;
        [SerializeField] private CustomizableButton _audio;
        [SerializeField] private CustomizableButton _other;
        [SerializeField] private CustomizableButton _back;

        private IList<Selectable> _selectables; 

        public override void Initialize(OptionsMenuModel model) {
            _graphics.OnSubmit.AddListener(() => model.PushMenu(Menu.Graphics));
            _input.OnSubmit.AddListener(() => model.PushMenu(Menu.Input));
            _gameplay.OnSubmit.AddListener(() => model.PushMenu(Menu.Gameplay));
            _audio.OnSubmit.AddListener(() => model.PushMenu(Menu.Audio));
            _timeAndWeather.OnSubmit.AddListener(() => model.PushMenu(Menu.TimeAndWeather));
            _other.OnSubmit.AddListener(() => model.PushMenu(Menu.Other));
            _back.OnSubmit.AddListener(model.PopMenu);

            _selectables = GetComponentsInChildren<Selectable>(includeInactive: true)
                .ToList();
        }

        public override void SetState(OptionsMenuModel model, Func<string, string> l) {
            _title.text = l("options");
            _graphics.text = l("graphics");
            _input.text = l("input");
            _gameplay.text = l("gameplay");
            _audio.text = l("audio");
            _timeAndWeather.text = l("time_and_weather");
            _other.text = l("other");
            _back.text = l("back");

            UiNavigation.ResolveExplicitNavigation(_selectables);
        }

        public override Menu Id { get {return Menu.Options;} }
    }
}
