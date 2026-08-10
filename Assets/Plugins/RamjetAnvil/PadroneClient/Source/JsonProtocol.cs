using System;
using System.Globalization;
using System.Net;
using Newtonsoft.Json;

namespace RamjetAnvil.Padrone.Client {
    public static class JsonProtocol {
        public class ClientSecretConverter : JsonConverter {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                var sessionId = (ClientSecret) value;
                serializer.Serialize(writer, sessionId.Value);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer) {
                var sessionId = serializer.Deserialize<string>(reader);
                return new ClientSecret(sessionId);
            }

            public override bool CanConvert(Type objectType) {
                return objectType == typeof (ClientSecret);
            }
        }

        public class ClientSessionIdConverter : JsonConverter {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                var sessionId = (ClientSessionId) value;
                serializer.Serialize(writer, sessionId.Value);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer) {
                var sessionId = serializer.Deserialize<string>(reader);
                return new ClientSessionId(sessionId);
            }

            public override bool CanConvert(Type objectType) {
                return objectType == typeof (ClientSessionId);
            }
        }

        public class IPEndpointConverter : JsonConverter {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                var ipEndpoint = (IPEndPoint) value;
                serializer.Serialize(writer, ipEndpoint.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer) {
                var ipEndpointStr = serializer.Deserialize<string>(reader);
                return ParseIpEndPoint(ipEndpointStr);
            }

            public override bool CanConvert(Type objectType) {
                return objectType == typeof (IPEndPoint);
            }

            private static IPEndPoint ParseIpEndPoint(string endPoint) {
                string[] ep = endPoint.Split(':');
                if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
                IPAddress ip;
                if (ep.Length > 2) {
                    if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip)) {
                        throw new FormatException("Invalid ip-adress");
                    }
                } else {
                    if (!IPAddress.TryParse(ep[0], out ip)) {
                        throw new FormatException("Invalid ip-adress");
                    }
                }
                int port;
                if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port)) {
                    throw new FormatException("Invalid port");
                }
                return new IPEndPoint(ip, port);
            }
        }
    }
}
