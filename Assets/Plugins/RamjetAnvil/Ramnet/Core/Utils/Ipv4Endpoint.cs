using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace RamjetAnvil.Util {

    [Serializable]
    public struct Ipv4Endpoint : IEquatable<Ipv4Endpoint> {
        public string Address;
        public ushort Port;

        public Ipv4Endpoint(string address, ushort port) {
            Address = address;
            Port = port;
        }

        public Ipv4Endpoint(IPEndPoint dotnetEndpoint) {
            Address = dotnetEndpoint.Address.ToString();
            Port = (ushort) dotnetEndpoint.Port;
        }

        public bool Equals(Ipv4Endpoint other) {
            return string.Equals(Address, other.Address) && Port == other.Port;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Ipv4Endpoint && Equals((Ipv4Endpoint) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((Address != null ? Address.GetHashCode() : 0) * 397) ^ Port.GetHashCode();
            }
        }

        public static bool operator ==(Ipv4Endpoint left, Ipv4Endpoint right) {
            return left.Equals(right);
        }

        public static bool operator !=(Ipv4Endpoint left, Ipv4Endpoint right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return Address + ":" + Port;
        }

        public IPEndPoint ToIpEndPoint() {
            return new IPEndPoint(IPAddress.Parse(Address), Port);
        }

        public static Ipv4Endpoint Parse(string endpoint) {
            var ipAndPort = endpoint.Trim().Split(':');
            var ip = ipAndPort[0];
            var port = Convert.ToUInt16(ipAndPort[1]);
            return new Ipv4Endpoint(ip, port);
        }
    }
}
