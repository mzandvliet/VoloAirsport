using System;
using System.Linq;
using System.Net;
using Lidgren.Network;
using RamjetAnvil.RamNet;
using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;

namespace RamjetAnvil.RamNet {

    public struct TransportGroupId {
        public readonly int Value;

        public TransportGroupId(int value) {
            Value = value;
        }

        public bool Equals(TransportGroupId other) {
            return Value == other.Value;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TransportGroupId && Equals((TransportGroupId) obj);
        }

        public override int GetHashCode() {
            return Value;
        }

        public override string ToString() {
            return string.Format("TransportGroupId({0})", Value);
        }

        public static explicit operator int(TransportGroupId groupId) {
            return groupId.Value;
        }
        public static explicit operator TransportGroupId(int value) {
            return new TransportGroupId(value);
        }
    }

    public class TransportRouterConfig {

        public readonly IDictionary<TransportGroupId, TransportGroupConfig> Groups;
        public readonly TransportGroupId DefaultGroup;

        public TransportRouterConfig(IDictionary<TransportGroupId, TransportGroupConfig> groups, TransportGroupId defaultGroup) {
            Groups = groups;
            DefaultGroup = defaultGroup;
        }

        public TransportRouterConfig(TransportGroupId defaultGroup, params KeyValuePair<TransportGroupId, TransportGroupConfig>[] groups) {
            DefaultGroup = defaultGroup;
            Groups = ArrayDictionary.FromValues(groups);
        }

        public int MaxConnections {
            get {
                return Groups.Values.Aggregate(0, (total, config) => total + config.MaxConnections);    
            }
        }
    }

    public struct TransportGroupConfig {
        public readonly int MaxConnections;

        public TransportGroupConfig(int maxConnections) {
            MaxConnections = maxConnections;
        }
    }

    /* Todo: 
     * Group identification should be twofold: This thing knows which connection is in which group, that's one. Communication also includes
     * a byte id indicating intended group, that's two. This way cheaters can't bypass auth module, and any inadvertent communication mismatches
     * can be caught as well. I.e. when client is trying to talk to game module but hasn't actually been authenticated yet.
     */
     // TODO Group handlers should handle group joined/group left events instead of onConnected/onDisconnected
    public class TransportGroupRouter : IDisposable {
        private readonly IConnectionTransporter _transporter;
        private readonly TransportRouterConfig _config;

        private readonly IDictionary<TransportGroupId, ConnectionGroup> _groups; 
        private readonly IDictionary<TransportGroupId, ITransportDataHandler> _dataHandlers;
        
        public TransportGroupRouter(IConnectionTransporter transporter, TransportRouterConfig config) {
            _transporter = transporter;
            _config = config;

            var groupIds = config.Groups.Keys.ToList();
            _groups = new ArrayDictionary<TransportGroupId, ConnectionGroup>(config.Groups.Count);
            for (int i = 0; i < groupIds.Count; i++) {
                var groupId = groupIds[i];
                var groupConfig = config.Groups[groupId];
                _groups[groupId] = new ConnectionGroup(groupId, maxConnections: groupConfig.MaxConnections);
            }
            _dataHandlers =  new ArrayDictionary<TransportGroupId, ITransportDataHandler>(config.Groups.Count);

            transporter.OnConnectionOpened += OnConnectionEstablished;
            transporter.OnConnectionClosed += OnDisconnected;
            transporter.OnDataReceived += OnDataReceived;
        }

        public void SetDataHandler(TransportGroupId group, ITransportDataHandler handler) {
            _dataHandlers[group] = handler;
        }

        public IConnectionGroup GetConnectionGroup(TransportGroupId group) {
            return _groups[group];
        }

        private void OnConnectionEstablished(ConnectionId connectionId, IPEndPoint endPoint) {
            var assignedGroupId = GetAssignedGroupId(connectionId);
            if (!assignedGroupId.HasValue) {
                AssignToGroup(_config.DefaultGroup, connectionId);    
            }
        }

        private void OnDisconnected(ConnectionId connectionId) {
            var groupId = GetGroupId(connectionId);
            //Debug.Log("removing " + connectionId + " from group " + group);
            _groups[groupId].RemoveConnection(connectionId);
        }

        public void OnDataReceived(ConnectionId connectionId, IPEndPoint endpoint, NetBuffer reader) {
            // Todo: read first byte to ensure data is meant for the group we're about to send it to
            // Todo: if we do the above, would it be good to also write that ID from within this class?

            var group = GetGroupId(connectionId);
            if (_dataHandlers[group] != null) {
                //Debug.Log("message for group " + group + " on " + connectionId);
                _dataHandlers[group].OnDataReceived(connectionId, endpoint, reader);
            } else {
                Debug.LogWarning("message for group " + group + " but there is no handler");
            }
        }

        public TransportGroupId GetGroupId(ConnectionId connectionId) {
            var group = GetAssignedGroupId(connectionId);
            if (group.HasValue) {
                return group.Value;
            } else {
                throw new ArgumentException("Connection with ID " + connectionId + " is not in any group. This shouldn't happen.");    
            }
        }

        public IList<ConnectionId> GetActiveConnections(TransportGroupId groupId) {
            return _groups[groupId].ActiveConnections;
        }

        public TransportGroupId? GetAssignedGroupId(ConnectionId connectionId) {
            if (connectionId == ConnectionId.Self) {
                return _config.DefaultGroup;
            }

            var groups = _groups.Values.ToListOptimized();
            for (int i = 0; i < groups.Count; i++) {
                var group = groups[i];
                if (group.ActiveConnections.Contains(connectionId)) {
                    return group.GroupId;
                }
            }
            return null;
        }

        public bool AssignToGroup(TransportGroupId newGroupId, ConnectionId connectionId) {
            //Debug.Log("assigning " + connectionId + " to " + newGroup);

            var newGroup = _groups[newGroupId];
            if (newGroup.ActiveConnections.Count < newGroup.MaxConnections) {
                var existingGroupId = GetAssignedGroupId(connectionId);
                if (existingGroupId.HasValue) {
                    var existingGroup = _groups[existingGroupId.Value];
                    existingGroup.RemoveConnection(connectionId);
                }
                newGroup.AddConnection(connectionId);
                return true;
            }

            Debug.LogWarning("No more room in " + newGroup);

            return false;
        }

        public void Dispose() {
            _transporter.OnConnectionOpened -= OnConnectionEstablished;
            _transporter.OnConnectionClosed -= OnDisconnected;
            _transporter.OnDataReceived -= OnDataReceived;            
        }

        public void ClearDataHandlers() {
            _dataHandlers.Clear();
        }

        private class ConnectionGroup : IConnectionGroup {

            public readonly int MaxConnections;
            public readonly TransportGroupId GroupId;
            private readonly IList<ConnectionId> _connections;

            private Action<ConnectionId> _peerJoined;
            public event Action<ConnectionId> PeerJoined {
                add {
                    for (int i = 0; i < _connections.Count; i++) {
                        var connectionId = _connections[i];
                        value(connectionId);
                    }
                    _peerJoined += value;
                }
                remove { _peerJoined += value; }
            }
            public event Action<ConnectionId> PeerLeft;

            public ConnectionGroup(TransportGroupId groupId, int maxConnections) {
                GroupId = groupId;
                MaxConnections = maxConnections;
                _connections = new List<ConnectionId>(maxConnections);
            }

            public IList<ConnectionId> ActiveConnections {
                get { return _connections; }
            }

            public void AddConnection(ConnectionId connectionId) {
                _connections.Add(connectionId);
                if (_peerJoined != null) {
                    _peerJoined(connectionId);
                }
            }

            public void RemoveConnection(ConnectionId connectionId) {
                _connections.Remove(connectionId);
                if (PeerLeft != null) {
                    PeerLeft(connectionId);
                }
            }
        }
    }


}
