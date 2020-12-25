using System.Net;
using Lidgren.Network;
using RamjetAnvil.RamNet;
using RamjetAnvil.Util;
using UnityEngine;
using UnityEngine.Networking;

public interface ITransportConnectionHandler {
    void OnConnectionEstablished(ConnectionId connectionId, IPEndPoint endpoint);
    void OnDisconnected(ConnectionId connectionId);
}

public interface IConnectionlessDataHandler {
    void OnDataReceived(IPEndPoint endpoint, NetBuffer buffer);
}

public interface ITransportDataHandler {
    void OnDataReceived(ConnectionId connectionId, IPEndPoint endpoint, NetBuffer buffer);
}