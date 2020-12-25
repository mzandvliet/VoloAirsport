using System;
using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    public class ChallengeAnnouncerUi : MonoBehaviour {
        [SerializeField, Dependency("gameClock")] private AbstractUnityClock _clock;

        [SerializeField] private TextAnimationConfig _challengeType;
        [SerializeField] private TextAnimationConfig _challengeName;
        [SerializeField] private TextAnimationConfig _challengeHighScore;
        [SerializeField] private AnimationCurve _animationCurve;
        [SerializeField] private float _animationDuration;

        private bool _isAnimationInProgress;

        void Awake() {
            DisableUi();
        }

        void OnEnable() {
            if (_isAnimationInProgress) {
                EnableUi();
            }
        }

        void OnDisable() {
            DisableUi();
        }

        public IEnumerator<WaitCommand> Introduce(string challengeType, Color typeColor, string challengeName, string scoringInfo = null) {
            _challengeType.Text.text = challengeType;
            _challengeType.Text.color = typeColor;
            _challengeName.Text.text = challengeName;
            _challengeHighScore.Text.text = scoringInfo ?? "";
            var animationTime = 0f;

            while (_isAnimationInProgress) {
                yield return WaitCommand.WaitForNextFrame;
            }

            _isAnimationInProgress = true;
            EnableUi();
            while (animationTime <= 1f) {
                animationTime += _clock.DeltaTime / _animationDuration;   
                var position = _animationCurve.Evaluate(animationTime);
                var opacity = position;

                _challengeType.Wrapper.alpha = 0f;
                _challengeName.Wrapper.alpha = 0f;
                _challengeHighScore.Wrapper.alpha = 0f;

                _challengeType.Wrapper.alpha = opacity;
                _challengeType.Wrapper.transform.localPosition = Vector2.Lerp(_challengeType.StartPosition, _challengeType.EndPosition, position);

                _challengeName.Wrapper.alpha = opacity;
                _challengeName.Wrapper.transform.localPosition = Vector2.Lerp(_challengeName.StartPosition, _challengeName.EndPosition, position); 

                _challengeHighScore.Wrapper.alpha = opacity;
                _challengeHighScore.Wrapper.transform.localPosition = Vector2.Lerp(_challengeHighScore.StartPosition, _challengeHighScore.EndPosition, position); 

                yield return WaitCommand.WaitForNextFrame;
            }
            DisableUi();
            _isAnimationInProgress = false;
        }

        void DisableUi() {
            _challengeType.Wrapper.gameObject.SetActive(false);
            _challengeName.Wrapper.gameObject.SetActive(false);
            _challengeHighScore.Wrapper.gameObject.SetActive(false);
        }

        void EnableUi() {
            _challengeType.Wrapper.gameObject.SetActive(true);
            _challengeName.Wrapper.gameObject.SetActive(true);
            _challengeHighScore.Wrapper.gameObject.SetActive(true);
        }

        [Serializable]
        public struct TextAnimationConfig {
            public Vector2 StartPosition;
            public Vector2 EndPosition;
            public CanvasGroup Wrapper;
            public Text Text;
        }
    }
}
