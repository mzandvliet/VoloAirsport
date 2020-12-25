using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.RamNet {
    public class ConnectionIdPool {

        private readonly int _maxConnectionIds;
        private readonly Queue<ConnectionId> _pool;

        public ConnectionIdPool(int maxConnectionIds) {
            _maxConnectionIds = maxConnectionIds;
            _pool = new Queue<ConnectionId>(maxConnectionIds);
            for (int i = 0; i < maxConnectionIds; i++) {
                _pool.Enqueue(new ConnectionId(i));
            }
        }

        public ConnectionId Take() {
            if (_pool.Count == 0) {
                throw new Exception("No more connection ids available");
            }
            
            var connectionId = _pool.Dequeue();
//            Debug.Log(connectionId + " taken from pool");
            return connectionId;
        }

        public void Put(ConnectionId connectionId) {
//            Debug.Log(connectionId + " put back in pool");

            if (!_pool.Contains(connectionId)) {
                _pool.Enqueue(connectionId);    
            }
        }

        public int MaxConnectionIds {
            get { return _maxConnectionIds; }
        }
    }
}
