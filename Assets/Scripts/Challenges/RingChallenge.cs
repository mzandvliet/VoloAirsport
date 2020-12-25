using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Coroutine.Time;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityExecutionOrder;
using Fmod = FMODUnity.RuntimeManager;

namespace RamjetAnvil.Volo {

    [Run.After(typeof(Ring))]
    public class RingChallenge : Challenge {

        public override event FinishChallenge OnFinished;

        [SerializeField, Dependency("gameClock")] private AbstractUnityClock _clock;
        [SerializeField, Dependency] private NotificationList _notificationList;
        [SerializeField, Dependency] private ActiveLanguage _activeLanguage;
        [SerializeField] private string _name;
        [SerializeField] private string _id = System.Guid.NewGuid().ToString();
        [SerializeField] private Transform _startTransform;
        [SerializeField] private List<Turret> _turrets;
        [SerializeField] private List<Ring> _rings;
        [SerializeField] private float _notificationTimeout = 5f;

        private FlightStatistics _player;
        private int _nextRing;
        private HashSet<int> _ringsPassed; 
        private double _startTime;

        void Awake() {
            _ringsPassed = new HashSet<int>();
            for (int i = 0; i < _rings.Count; i++) {
                var ringIndex = i;
                var ring = _rings[ringIndex];

                var isLastRing = ringIndex == _rings.Count - 1;
                if (isLastRing) {
                    ring.OnPlayerContact += player => OnCourseFinished();
                } else {
                    ring.OnPlayerContact += player => OnRingPassed(ringIndex);
                }
                ring.SetEnabled(false);
            }
            for (int i = 0; i < _turrets.Count; i++) {
                //_turrets[i].Initialize();
                _turrets[i].gameObject.SetActive(false);
            }
        }

        public override IEnumerator<WaitCommand> Begin(FlightStatistics player) {
            _startTime = _clock.CurrentTime;
            _nextRing = 0;
            _ringsPassed.Clear();
            _player = player;

            for (int i = 0; i < _rings.Count; i++) {
                var ring = _rings[i];
                ring.SetEnabled(true);
            }
            for (int i = 0; i < _turrets.Count; i++) {
                //_turrets[i].SetTarget(player);
                _turrets[i].gameObject.SetActive(true);
            }

            _notificationList.AddTimedNotification(_activeLanguage.Table["course"] + ": " + Name, _notificationTimeout.Seconds());

            Fmod.PlayOneShot("event:/Objects/ring_pass_start");

            yield return WaitCommand.DontWait;
        }

        private void OnRingPassed(int ringIndex) {
            _ringsPassed.Add(ringIndex);
            _nextRing = ringIndex + 1;
            Fmod.PlayOneShot("event:/Objects/ring_pass_norm");
        }

        private void OnCourseFinished() {
            _ringsPassed.Add(_rings.Count - 1);

            var endTime = _clock.CurrentTime - _startTime;
            var totalRingsPassed = _ringsPassed.Count + 1;
            var ringsSkipped = _rings.Count - totalRingsPassed;
            string ringsSkippedStr = "";
            if (ringsSkipped <= 0) {
                ringsSkippedStr = "";
            } else if (ringsSkipped == 1) {
                ringsSkippedStr = " (" + _activeLanguage.Table["ring_skipped"] + ")";
            } else {
                ringsSkippedStr = " (" + _activeLanguage.Table["rings_skipped"].Replace("$n", ringsSkipped.ToString()) + ")";
            }
            _notificationList.AddTimedNotification(
                _activeLanguage.Table["time"] + ": " +
                MathUtils.RoundToDecimals(endTime, 3).ToString("N3") +
                " " + _activeLanguage.Table["seconds"] + 
                ringsSkippedStr, _notificationTimeout.Seconds());


            Fmod.PlayOneShot("event:/Objects/ring_pass_start");

            for (int i = 0; i < _rings.Count; i++) {
                var ring = _rings[i];
                ring.SetEnabled(false);
            }
            for (int i = 0; i < _turrets.Count; i++) {
                _turrets[i].gameObject.SetActive(false);
            }

            if (OnFinished != null) {
                OnFinished(_player, FinishCondition.Win);
            }
        }

        public override ChallengeType ChallengeType {
            get { return ChallengeType.Rings; }
        }

        public override string Id {
            get { return _id; }
        }

        public override string Name { get { return _name; } }

        public override ImmutableTransform StartTransform {
            get { return _startTransform.MakeImmutable(); }
        }
    }
}
