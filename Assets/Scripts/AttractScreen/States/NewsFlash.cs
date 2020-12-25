using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.InputModule;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Volo.Ui;
using UnityEngine;

namespace RamjetAnvil.Volo.States {

    public class NewsFlash : State {
        [Serializable]
        public class Data {
            public NewsFeedView NewsFeedView;
            public CameraManager CameraManager;
            public CameraMount CameraMount;
            public RamjetInputModule InputModule;
        }

        private readonly Data _data;
        private readonly IStateMachine _machine;

        public NewsFlash(IStateMachine machine, Data data) : base(machine) {
            _machine = machine;
            _data = data;
            _data.NewsFeedView.gameObject.SetActive(false);
        }

        void OnEnter() {
            _data.NewsFeedView.gameObject.SetActive(true);
            _data.CameraManager.SwitchMount(_data.CameraMount);
            _data.InputModule.ReContextualize(_data.NewsFeedView.FirstObject);
            _data.NewsFeedView.OnContinue.AddListener(Continue);
        }

        private void Continue() {
            _data.NewsFeedView.gameObject.SetActive(false);
            _machine.Transition(VoloStateMachine.States.TitleScreen);
            _data.NewsFeedView.OnContinue.RemoveListener(Continue);
        }
    }
}
