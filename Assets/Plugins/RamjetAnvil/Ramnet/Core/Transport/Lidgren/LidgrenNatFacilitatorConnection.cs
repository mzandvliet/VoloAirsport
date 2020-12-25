using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Coroutine;
using RamjetAnvil.RamNet;
using RamjetAnvil.Util;
using UnityEngine;

namespace RamjetAnvil.RamNet {

    public class LidgrenNatFacilitatorConnection : IDisposable {

        public event Action<NatPunchId, IPEndPoint> OnNatPunchSuccess;
        public event Action<NatPunchId, IPEndPoint> OnNatPunchFailure;
        
        private readonly LidgrenNetworkTransporter _transporter;
        private readonly IPEndPoint _natFacilitatorEndpoint;

        private readonly NetBuffer _outgoingMessage;
        private bool _isRegistrationRunning;

        private AsyncResult<IPEndPoint> _externalEndpoint;

        public LidgrenNatFacilitatorConnection(IPEndPoint natFacilitatorEndpoint, LidgrenNetworkTransporter transporter) {

            _outgoingMessage = new NetBuffer();
            _natFacilitatorEndpoint = natFacilitatorEndpoint;
            _transporter = transporter;
            _externalEndpoint = new AsyncResult<IPEndPoint>();

            _transporter.OnNatPunchSuccess += NatPunchSuccess;
            _transporter.OnUnconnectedDataReceived += OnUnconnectedDataReceived;
        }

        public void SendIntroduction(IPEndPoint remoteEndpoint, NatPunchId punchId) {
            _outgoingMessage.Reset();
            _outgoingMessage.Write((byte) NatFacilitatorRequestType.RequestIntroduction);
            _outgoingMessage.Write(_transporter.InternalEndpoint);
            _outgoingMessage.Write(remoteEndpoint);
            _outgoingMessage.Write(punchId.Value);
            _transporter.SendUnconnected(_natFacilitatorEndpoint, _outgoingMessage);
            _transporter.Flush();
        } 

        public IEnumerator<WaitCommand> Register() {
            // Add confirmation message

            _isRegistrationRunning = true;
            while (_isRegistrationRunning) {
                if (_transporter.Status == TransporterStatus.Open) {
                    _outgoingMessage.Reset();
                    _outgoingMessage.Write((byte) NatFacilitatorRequestType.RegisterPeer);
                    _outgoingMessage.Write(_transporter.InternalEndpoint);
                    _transporter.SendUnconnected(_natFacilitatorEndpoint, _outgoingMessage);
                    yield return WaitCommand.WaitSeconds(10);
                } else {
                    yield return WaitCommand.WaitForNextFrame;
                }
            }
        }

        public void Unregister() {
            // Unregister
            _isRegistrationRunning = false;
            _outgoingMessage.Reset();
            _outgoingMessage.Write((byte) NatFacilitatorRequestType.UnregisterPeer);
            _transporter.SendUnconnected(_natFacilitatorEndpoint, _outgoingMessage);
            _externalEndpoint = new AsyncResult<IPEndPoint>();
        }

        public AsyncResult<IPEndPoint> ExternalEndpoint {
            get { return _externalEndpoint; }
        }

        private void OnUnconnectedDataReceived(IPEndPoint endpoint, NetBuffer incomingData) {
            if (endpoint.Equals(_natFacilitatorEndpoint)) {
                var messageType = (NatFacilitatorMessageType) incomingData.ReadByte();
                switch (messageType) {
                    case NatFacilitatorMessageType.HostNotRegistered:
                        // Handle host not registered response
                        if (OnNatPunchFailure != null) {
                            var punchId = new NatPunchId(incomingData.ReadString());
                            OnNatPunchFailure(punchId, endpoint);
                        }
                        break;
                    case NatFacilitatorMessageType.PeerRegistrationSuccess:
                        if (_isRegistrationRunning) {
                            _externalEndpoint.SetResult(incomingData.ReadIPEndPoint());
                        }
                        break;
                    default:
                        // Skip this message
                        break;
                }
            }
        }

        private void NatPunchSuccess(string punchId, IPEndPoint endpoint) {
            //Debug.Log("NAT punch was successful to " + endpoint + " (punch id: " + punchId + ")");
            if (OnNatPunchSuccess != null) {
                OnNatPunchSuccess(new NatPunchId(punchId), endpoint);
            }
        }

        public void Dispose() {
            _isRegistrationRunning = false;
            _transporter.OnUnconnectedDataReceived -= OnUnconnectedDataReceived;
            _transporter.OnNatPunchSuccess -= NatPunchSuccess;
        }
    }
}
