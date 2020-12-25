using System;
using System.Net;

namespace RamjetAnvil.RamNet {

    public interface IConnectionManager : IDisposable {
        event RequestApproval RequestApproval;

        ConnectionId Connect(IPEndPoint hostEndpoint,
            ApprovalSecret approvalSecret,
            OnConnectionEstablished onConnectionEstablished = null,
            OnConnectionFailure onConnectionFailure = null,
            OnDisconnected onDisconnected = null);

        void Disconnect(ConnectionId connectionId);

        /// <summary>
        /// Cancels a connection that is not yet established
        /// No established/failure/disconnect handlers will be called,
        /// it is as if the connection attempt never happened.
        /// 
        /// If the connection is already established it does nothing.
        /// </summary>
        /// <param name="connectionId">the pending connection to cancel</param>
        void CancelPending(ConnectionId connectionId);
    }
}
