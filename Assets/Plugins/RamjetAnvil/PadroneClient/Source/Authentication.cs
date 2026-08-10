using System;
using System.Text;

namespace RamjetAnvil.Padrone.Client {
    public struct AuthToken {
        public readonly string Method;
        public readonly string Credentials;
        private readonly string _base64String;

        public AuthToken(string method, string credentials) {
            Method = method;
            Credentials = credentials;
            _base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(Method + ":" + Credentials));
        }

        public string AsBase64() {
            return _base64String;
        }
    }

    public static class Authentication {
        public static AuthToken AdminAuthToken(string username, string password) {
            return new AuthToken("admin", username + "," + password);
        }

        public static AuthToken SteamAuthToken(byte[] authTicket, uint ticketLength) {
            return new AuthToken("steam", authTicket.ToHexString(0, (int) ticketLength));
        }

        public static AuthToken ItchDownloadKeyAuthToken(string downloadKey) {
            return new AuthToken("itch.downloadkey", downloadKey);
        }

        public static AuthToken ItchApiKeyAuthToken(string apiKey) {
            return new AuthToken("itch.apikey", apiKey);
        }

        public static AuthToken OculusAuthToken(string userId, string nonce) {
            return new AuthToken("oculus", userId + "," + nonce);
        }
    }
}
