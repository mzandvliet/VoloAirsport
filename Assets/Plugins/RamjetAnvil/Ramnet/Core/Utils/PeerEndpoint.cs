using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RamjetAnvil.Util;

namespace RamjetAnvil.RamNet {

    public struct PeerEndpoint : IEquatable<PeerEndpoint> {
        private readonly Ipv4Endpoint? _internal;
        private readonly Ipv4Endpoint _external;

        public PeerEndpoint(Ipv4Endpoint @internal, Ipv4Endpoint external) {
            _internal = @internal;
            _external = external;
        }

        public PeerEndpoint(Ipv4Endpoint external) {
            _external = external;
            _internal = null;
        }

        public Ipv4Endpoint? Internal {
            get { return _internal; }
        }

        public Ipv4Endpoint External {
            get { return _external; }
        }

        public bool Contains(Ipv4Endpoint endpoint) {
            return endpoint == _internal || endpoint == _external;
        }

        public bool Equals(PeerEndpoint other) {
            return _internal.Equals(other._internal) && _external.Equals(other._external);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PeerEndpoint && Equals((PeerEndpoint) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (_internal.GetHashCode() * 397) ^ _external.GetHashCode();
            }
        }

        public static bool operator ==(PeerEndpoint left, PeerEndpoint right) {
            return left.Equals(right);
        }

        public static bool operator !=(PeerEndpoint left, PeerEndpoint right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return string.Format("Internal: {0}, External: {1}", _internal, _external);
        }
    }
}
