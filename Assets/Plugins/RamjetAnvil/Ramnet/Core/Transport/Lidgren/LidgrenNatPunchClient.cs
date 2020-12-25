using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;
using Guid = System.Guid;

namespace RamjetAnvil.RamNet {

    public class LidgrenNatPunchClient : INatPunchClient, IDisposable {

        private static readonly OnNatPunchSuccess EmptyOnSuccess = (punchId, endpoint) => { };
        private static readonly OnNatPunchFailure EmptyOnFailure = endpoint => { };

        private readonly IObjectPool<PunchAttempt> _punchAttemptPool;
        private readonly IEnumerator<NatPunchId> _natPunchIds; 

        private readonly TimeSpan _punchAttemptTimeout = TimeSpan.FromSeconds(30f);
        private readonly LidgrenNatFacilitatorConnection _facilitatorConnection;
        private readonly IDictionary<NatPunchId, IPooledObject<PunchAttempt>> _natPunchAttempts;
        private readonly IList<IPooledObject<PunchAttempt>> _natPunchRegistrations;

        private readonly IDisposable _cleanupRoutine;

        public LidgrenNatPunchClient(ICoroutineScheduler coroutineScheduler, LidgrenNatFacilitatorConnection facilitatorConnection) {

            _facilitatorConnection = facilitatorConnection;

            _punchAttemptPool = new ObjectPool<PunchAttempt>(() => new PunchAttempt());

            {
                const int natpunchIdCount = 256;
                var punchIds = new NatPunchId[natpunchIdCount];
                for (int i = 0; i < natpunchIdCount; i++) {
                    punchIds[i] = new NatPunchId(Guid.NewGuid().ToString());
                }
                _natPunchIds = CollectionUtil.LoopingEnumerator(punchIds);
            }

            _natPunchAttempts = new Dictionary<NatPunchId, IPooledObject<PunchAttempt>>();
            _natPunchRegistrations = new List<IPooledObject<PunchAttempt>>();
            _facilitatorConnection.OnNatPunchSuccess += OnNatPunchSuccess;
            _facilitatorConnection.OnNatPunchFailure += OnNatPunchFailure;
            _cleanupRoutine = coroutineScheduler.Run(ConnectionTimeoutCleanup());
        }

        public NatPunchId Punch(IPEndPoint remoteEndpoint, OnNatPunchSuccess onSuccess = null, OnNatPunchFailure onFailure = null) {
            onSuccess = onSuccess ?? EmptyOnSuccess;
            onFailure = onFailure ?? EmptyOnFailure;

            var attempt = _punchAttemptPool.Take();
            attempt.Instance.Timestamp = DateTime.Now;
            attempt.Instance.EndPoint = remoteEndpoint;
            _natPunchIds.MoveNext();
            attempt.Instance.PunchId = _natPunchIds.Current;
            attempt.Instance.OnSuccess += onSuccess;
            attempt.Instance.OnFailure += onFailure;
            AddNatPunchAttempt(attempt);

            _facilitatorConnection.SendIntroduction(remoteEndpoint, attempt.Instance.PunchId);

            return attempt.Instance.PunchId;
        }

        public void Dispose() {
            _facilitatorConnection.OnNatPunchSuccess -= OnNatPunchSuccess;
            _facilitatorConnection.OnNatPunchFailure -= OnNatPunchFailure;
            _cleanupRoutine.Dispose();
        }

        private void OnNatPunchSuccess(NatPunchId punchId, IPEndPoint actualEndpoint) {
            IPooledObject<PunchAttempt> attempt;
            
            if (_natPunchAttempts.TryGetValue(punchId, out attempt)) {
                Debug.Log("Received NAT punch success to endpoint: " + actualEndpoint);

                attempt.Instance.OnSuccess(punchId, actualEndpoint);
                RemoveNatPunchAttempt(attempt);
            }
        }
        
        private void OnNatPunchFailure(NatPunchId punchId, IPEndPoint actualEndpoint) {
            IPooledObject<PunchAttempt> attempt;
            if (_natPunchAttempts.TryGetValue(punchId, out attempt)) {
                attempt.Instance.OnFailure(punchId);
                RemoveNatPunchAttempt(attempt);
            }
        }
        
        private void AddNatPunchAttempt(IPooledObject<PunchAttempt> attempt) {
            _natPunchAttempts[attempt.Instance.PunchId] = attempt;
            _natPunchRegistrations.Add(attempt);
        }

        private void RemoveNatPunchAttempt(IPooledObject<PunchAttempt> attempt) {
            _natPunchAttempts.Remove(attempt.Instance.PunchId);
            _natPunchRegistrations.Remove(attempt);
            attempt.Dispose();
        }

        private readonly IList<IPooledObject<PunchAttempt>> _removeableAttempts = new List<IPooledObject<PunchAttempt>>(); 
        private IEnumerator<WaitCommand> ConnectionTimeoutCleanup() {
            while (true) {
                var now = DateTime.Now;
                _removeableAttempts.Clear();
                for (int i = 0; i < _natPunchRegistrations.Count; i++) {
                    var attempt = _natPunchRegistrations[i];
                    if (attempt.Instance.Timestamp + _punchAttemptTimeout < now) {
                        attempt.Instance.OnFailure(attempt.Instance.PunchId);
                        _removeableAttempts.Add(attempt);
                    }
                }
                for (int i = 0; i < _removeableAttempts.Count; i++) {
                    RemoveNatPunchAttempt(_removeableAttempts[i]);
                }
                yield return WaitCommand.WaitSeconds((float) _punchAttemptTimeout.TotalSeconds);
            }
        }

        private class PunchAttempt {
            public DateTime Timestamp;
            public IPEndPoint EndPoint;
            public NatPunchId PunchId;
            public OnNatPunchSuccess OnSuccess;
            public OnNatPunchFailure OnFailure;
        }
    }
}
