using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace RxUnity.Schedulers {
    public class NodeList<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : IComparable<TKey> {
        private readonly Node start = new Node(default(TKey), default(TValue));

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            for (Node n = start.GetNext(); n != null; n = n.GetNext()) {
                yield return new KeyValuePair<TKey, TValue>(n.key, n.value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public Node Add(TKey key, TValue value) {
            Node parent;
            Node node;
            var newNode = new Node(key, value);
            do {
                node = FindNode(key, out parent);
            } while (!parent.InsertChild(newNode, node));
            return newNode;
        }

        private Node FindNode(TKey key, out Node parent) {
            Node n = start;
            Node node = n.GetNext();
            while ((node != null) && node.key.CompareTo(key) <= 0) {
                n = node;
                node = n.GetNext();
            }
            parent = n;
            return node;
        }

        public Node FirstNode() {
            return start.GetNext();
        }

        public class Node : IDisposable {
            internal readonly TKey key;
            internal readonly TValue value;
            private NodeState state;

            public Node(TKey key, TValue value) {
                this.key = key;
                this.value = value;
                state = new NodeState(false, null);
            }

            public TKey Key {
                get { return key; }
            }

            public TValue Value {
                get { return value; }
            }

            public void Dispose() {
                FlagAsDeleted();
            }

            public override string ToString() {
                return key + ", " + value;
            }

            internal bool InsertChild(Node newNode, Node successor) {
                NodeState oldState = state;

                if ((!oldState.isDeleted) && (oldState.next == successor)) {
                    var newState = new NodeState(false, newNode);
                    newNode.state = new NodeState(false, oldState.next);
                    return CasState(oldState, newState);
                }
                return false;
            }

            public Node GetNext() {
                Node node = state.next;
                // remove everything that is flagged as deleted, until a node is reached that is not deleted (garbage collection at each tick)
                while ((node != null) && (node.state.isDeleted)) {
                    TryDeleteNext(node);
                    node = state.next;
                }
                return node;
            }

            private void TryDeleteNext(Node next) {
                NodeState oldState = state;
                if (oldState.next == next) {
                    var newState = new NodeState(oldState.isDeleted, next.state.next);
                    CasState(oldState, newState);
                }
            }

            public void FlagAsDeleted() {
                NodeState newState;
                NodeState oldState;
                do {
                    oldState = state;
                    newState = new NodeState(true, oldState.next);
                } while (!CasState(oldState, newState));
            }

            private bool CasState(NodeState oldState, NodeState newState) {
                return oldState == Interlocked.CompareExchange(ref state, newState, oldState);
            }
        }

        private class NodeState {
            internal readonly bool isDeleted;
            internal readonly Node next;

            public NodeState(bool isDeleted, Node next) {
                this.isDeleted = isDeleted;
                this.next = next;
            }
        }
    }
}