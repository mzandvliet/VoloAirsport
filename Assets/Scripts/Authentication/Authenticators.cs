using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Padrone.Client;
using RamjetAnvil.Volo.Networking;
using Steamworks;
using UnityEngine;

namespace RamjetAnvil.Volo {

    public delegate Maybe<AuthToken> ProvideAuthToken();

    public static class Authenticators {
        public static readonly Func<AuthToken> All = CombineProviders(new List<ProvideAuthToken> {
            ItchApiKeyAuthTokenProvider(),
            SteamAuthTokenProvider(),
            AdminAuthTokenProvider(),
            ItchDownloadKeyAuthTokenProvider()
        }.ToImmutableList());
        public static readonly AuthToken EmptyAuthToken = new AuthToken("empty", "empty");

        public static Func<AuthToken> CombineProviders(IReadOnlyList<ProvideAuthToken> providers) {
            return () => {
                for (int i = 0; i < providers.Count; i++) {
                    var provider = providers[i];
                    var authToken = provider();
                    if (authToken.IsJust) {
                        return authToken.Value;
                    }
                }
                return EmptyAuthToken;
            };
        }

        public static ProvideAuthToken ItchApiKeyAuthTokenProvider() {
            Maybe<AuthToken> cachedAuthToken = Maybe.Nothing<AuthToken>();
            string cachedExpiryDate = null;

            return () => {
                var apiKey = Environment.GetEnvironmentVariable("ITCHIO_API_KEY");
                if (apiKey != null) {
                    var expiryDateStr = Environment.GetEnvironmentVariable("ITCHIO_API_KEY_EXPIRES_AT");
                    var isCacheExpired = expiryDateStr != cachedExpiryDate;
                    if (isCacheExpired) {
                        cachedExpiryDate = expiryDateStr;
                        Debug.Log("itch.io API key: '" + apiKey + "'");
                        // Example: 2016-07-20 15:56:04
//                        var expiryDate = DateTime.ParseExact(expiryDateStr, "yyyy-MM-dd HH:mm:ss", 
//                            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                        cachedAuthToken = Maybe.Just(Authentication.ItchApiKeyAuthToken(apiKey));
                    }
                } else {
                    cachedAuthToken = Maybe.Nothing<AuthToken>();
                }

                return cachedAuthToken;
            };
        }

        public static ProvideAuthToken ItchDownloadKeyAuthTokenProvider() {
            Maybe<AuthToken> authToken = Maybe.Nothing<AuthToken>();
            var downloadKeyPath = Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "ItchDownloadKey.txt");

            return () => {
                if (authToken.IsNothing) {
                    if (File.Exists(downloadKeyPath)) {
                        var downloadKey = File.ReadAllText(downloadKeyPath, Encoding.UTF8);
                        authToken = Maybe.Just(Authentication.ItchDownloadKeyAuthToken(downloadKey));
                    }   
                }
                return authToken;
            };
        }

        public static ProvideAuthToken SteamAuthTokenProvider() {
            const int maxTicketLength = 256;
            var cacheTimeOut = TimeSpan.FromMinutes(30);

            Maybe<AuthToken> cachedAuthToken = Maybe.Nothing<AuthToken>();
            DateTime? cacheTime = null;

            return () => {
                if (SteamAPI.IsSteamRunning() && SteamManager.Initialized) {
                    if (!cacheTime.HasValue || (DateTime.Now - cacheTime) > cacheTimeOut) {
                        uint ticketLength;
                        var ticket = new byte[maxTicketLength];
                        SteamUser.GetAuthSessionTicket(ticket, maxTicketLength, out ticketLength);
                        cachedAuthToken = Maybe.Just(Authentication.SteamAuthToken(ticket, ticketLength));
                        cacheTime = DateTime.Now;
                    }
                } else {
                    cachedAuthToken = Maybe.Nothing<AuthToken>();
                }

                return cachedAuthToken;
            };
        }

        public static ProvideAuthToken AdminAuthTokenProvider() {
            Maybe<AuthToken> authToken;
            var devSettings = DevSettingsSerialization.Deserialize();
            if (devSettings.IsJust) {
                authToken = Maybe.Just(Authentication.AdminAuthToken(devSettings.Value.AdminUsername, devSettings.Value.AdminPassword));
            } else {
                authToken = Maybe.Nothing<AuthToken>();
            }

            return () => authToken;
        }

        public static bool IsEmpty(this AuthToken authToken) {
            return authToken.Method == EmptyAuthToken.Method;
        }
    }
}
