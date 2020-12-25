using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {

    public class ChallengeIntroductionTester : MonoBehaviour {

        [SerializeField] private UnityCoroutineScheduler _coroutineScheduler;
        [SerializeField] private ChallengeAnnouncerUi _ui;

        void Update() {
            if (UnityEngine.Input.GetKeyDown(KeyCode.H)) {
                _coroutineScheduler.Run(_ui.Introduce("Spawnpoint discovered:", Color.grey, "Garden of Lift", "5/10 spawnpoints discovered"));
            }
        }

    }
}
