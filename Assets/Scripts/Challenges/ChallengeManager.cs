using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Ui;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public class ChallengeManager : MonoBehaviour {
        [SerializeField, Dependency] private UnityCoroutineScheduler _coroutineScheduler;
        [SerializeField, Dependency] private PilotActionMapProvider _actionMapProvider;
        [SerializeField] private ChallengeAnnouncerUi _challengeAnnouncerUi;
        [SerializeField] private Challenge[] _challenges;
        [SerializeField] private Gate _gatePrefab;

        private bool _isChallengeStarting;
        private Gate[] _gates;
        private Maybe<Challenge> _activeChallenge;

        public void Initialize() {
            _challenges = GameObject.FindObjectsOfType<Challenge>();

            _activeChallenge = Maybe<Challenge>.Nothing;
            _gates = new Gate[_challenges.Length];
            for (int i = 0; i < _challenges.Length; i++) {
                var challengeIndex = i;
                var challenge = _challenges[i];

                var gate = GameObject.Instantiate(_gatePrefab);
                gate.gameObject.transform.position = challenge.StartTransform.Position;
                gate.gameObject.transform.rotation = challenge.StartTransform.Rotation;
                gate.name = "Gate";
                gate.transform.SetParent(challenge.transform);
                _gates[challengeIndex] = gate;
                gate.OnGateTriggered += player => OnChallengeTriggered(challengeIndex, player);
                challenge.OnFinished += OnChallengeFinished;
            }
        }

        void Update() {
            if (_actionMapProvider == null) {
                return;
            }

            var actionMap = _actionMapProvider.ActionMap;
            if (actionMap.PollButtonEvent(WingsuitAction.Respawn) == ButtonEvent.Down) {
                
            }
        }

        private void OnChallengeTriggered(int challengeIndex, FlightStatistics player) {
            // Disable all gates
            for (int i = 0; i < _gates.Length; i++) {
                _gates[i].gameObject.SetActive(false);
            }

            var challenge = _challenges[challengeIndex];
            
            _activeChallenge = Maybe.Just(challenge);
            _coroutineScheduler.Run(BeginChallenge(challenge, player));
        }

        private void OnChallengeFinished(FlightStatistics player, FinishCondition finishCondition) {
            Debug.Log(_activeChallenge.Value.Name + " challenge finished");
            _activeChallenge = Maybe.Nothing<Challenge>();
            // Enable all gates
            for (int i = 0; i < _gates.Length; i++) {
                _gates[i].gameObject.SetActive(true);
            }

            // TODO Allow players to restart a challenge
            // Restart
//            else {
//                _coroutineScheduler.Run(BeginChallenge(_activeChallenge.Value, player));
//            }
        }

        private IEnumerator<WaitCommand> BeginChallenge(IChallenge challenge, FlightStatistics player) {
            if (_isChallengeStarting) {
                Debug.LogError("Challenge '" + challenge.Name + "' start is triggered while another challenge '" + 
                    _activeChallenge.Value.Name + "' is already starting");
                yield break;
            }
            _isChallengeStarting = true;
            Debug.Log("Starting challenge: " + challenge.Name);
            yield return WaitCommand.Interleave(
                challenge.Begin(player),
                _challengeAnnouncerUi.Introduce(challenge.ChallengeType.PrettyString(), Color.white, challenge.Name));
            _isChallengeStarting = false;
        }
    }
}
