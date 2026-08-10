using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RamjetAnvil.Coroutine;
using UnityEngine;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace RamjetAnvil.Padrone.Client {

    public interface IPadroneClient {
        void RegisterHost(HostRegistrationRequest registration, Action<HttpStatusCode> responseHandler);
        void UnregisterHost(IPEndPoint externalEndpoint, Action<HttpStatusCode> responseHandler);
        void ListHosts(bool hideFull, int limit, Action<HttpStatusCode, IList<RemoteHost>> responseHandler);
        void Ping(IPEndPoint hostEndpoint, IList<ClientSessionId> connectedClients, Action<HttpStatusCode> responseHandler);
        void HealthCheck(Action<HttpStatusCode> responseHandler);
        void Me(Action<HttpStatusCode, PlayerInfo> responseHandler);

        void Join(IPEndPoint hostEndpoint, string password, Action<HttpStatusCode, JoinResponse> responseHandler);
        void Leave(Action<HttpStatusCode> responseHandler);
        void ReportLeave(IPEndPoint hostEndpoint, ClientSessionId sessionId, Action<HttpStatusCode> responseHandler);

        void GetPlayerInfo(IPEndPoint hostEndpoint, ClientSessionId sessionId,
            Action<HttpStatusCode, PlayerSessionInfo> responseHandler);
    }

    public delegate void ResponseHandler(HttpStatusCode statusCode, string data);

    public class PadroneClient : IPadroneClient {

        private readonly string _masterServerUrl;
        private readonly string _appVersion;
        private readonly JsonSerializer _jsonSerializer;
        private readonly TimeSpan _requestTimeout;
        private readonly ICoroutineScheduler _coroutineScheduler;
        private readonly string[] _customHeaders;
        private readonly Func<AuthToken> _authTokenProvider;

        public PadroneClient(string masterServerUrl, string appVersion, Func<AuthToken> authTokenProvider,
            ICoroutineScheduler coroutineScheduler, TimeSpan requestTimeout) {

            _customHeaders = new string[16]; // Place for 8 key-value pairs
            _authTokenProvider = authTokenProvider;
            _masterServerUrl = masterServerUrl.TrimEnd('/') + "/app";
            _requestTimeout = requestTimeout;
            _coroutineScheduler = coroutineScheduler;
            _jsonSerializer = new JsonSerializer {
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _jsonSerializer.Converters.Add(new JsonProtocol.IPEndpointConverter());
            _jsonSerializer.Converters.Add(new JsonProtocol.ClientSessionIdConverter());
            _jsonSerializer.Converters.Add(new JsonProtocol.ClientSecretConverter());
            _appVersion = appVersion;
        }

        public void RegisterHost(HostRegistrationRequest registration, Action<HttpStatusCode> responseHandler) {
            Post("register-host", registration, (statusCode, data) => responseHandler(statusCode));
        }

        public void UnregisterHost(IPEndPoint externalEndpoint, Action<HttpStatusCode> responseHandler) {
            Post("unregister-host", new UnregisterHostRequest(externalEndpoint), (statusCode, data) => responseHandler(statusCode));
        }

        public void ListHosts(bool hideFull, int limit, Action<HttpStatusCode, IList<RemoteHost>> responseHandler) {
            var version = WWW.EscapeURL(_appVersion);
            Get("list-hosts?version=" + version + "&hideFull=" + hideFull.ToString().ToLower() + "&limit=" + limit, (statusCode, data) => {
                IList<RemoteHost> hostList = null;
                if (statusCode == HttpStatusCode.OK) {
                    using(var stringReader = new StringReader(data))
                    using(var jsonReader = new JsonTextReader(stringReader)) {
                        hostList = _jsonSerializer.Deserialize<IList<RemoteHost>>(jsonReader);
                    }
                }
                responseHandler(statusCode, hostList);
            });
        }

        public void GetPlayerInfo(IPEndPoint hostEndpoint, ClientSessionId sessionId, Action<HttpStatusCode, PlayerSessionInfo> responseHandler) {
            var hostEndpointParam = WWW.EscapeURL(hostEndpoint.ToString());
            var sessionIdParam = WWW.EscapeURL(sessionId.Value);
            Get("player-info?hostEndpoint=" + hostEndpointParam + "&sessionId=" + sessionIdParam, (statusCode, data) => {
                PlayerSessionInfo playerInfo = null;
                if (statusCode == HttpStatusCode.OK) {
                    using(var stringReader = new StringReader(data))
                    using(var jsonReader = new JsonTextReader(stringReader)) {
                        playerInfo = _jsonSerializer.Deserialize<PlayerSessionInfo>(jsonReader);
                    }
                }
                responseHandler(statusCode, playerInfo);
            });
        }

        public void Ping(IPEndPoint hostEndpoint, IList<ClientSessionId> connectedClients, Action<HttpStatusCode> responseHandler) {
            Post("ping", new PingRequest(hostEndpoint, connectedClients), (statusCode, data) => responseHandler(statusCode));
        }

        public void HealthCheck(Action<HttpStatusCode> responseHandler) {
            Get("health-check", (statusCode, data) => responseHandler(statusCode));
        }

        public void Me(Action<HttpStatusCode, PlayerInfo> responseHandler) {
            Get("me", (statusCode, data) => {
                PlayerInfo playerInfo = null;
                if (statusCode == HttpStatusCode.OK) {
                    using(var stringReader = new StringReader(data))
                    using(var jsonReader = new JsonTextReader(stringReader)) {
                        playerInfo = _jsonSerializer.Deserialize<PlayerInfo>(jsonReader);
                    }
                }
                responseHandler(statusCode, playerInfo);
            });
        }

        public void Join(IPEndPoint hostEndpoint, string password, Action<HttpStatusCode, JoinResponse> responseHandler) {
            Post("join", new JoinRequest(hostEndpoint, password), (statusCode, data) => {
                JoinResponse joinResponse = null;
                if (statusCode == HttpStatusCode.OK) {
                    using (var stringReader = new StringReader(data))
                    using (var jsonReader = new JsonTextReader(stringReader)) {
                        joinResponse = _jsonSerializer.Deserialize<JoinResponse>(jsonReader);
                    }
                }
                responseHandler(statusCode, joinResponse);
            });
        }

        public void Leave(Action<HttpStatusCode> responseHandler) {
            Post<byte[]>("leave", null, (statusCode, data) => responseHandler(statusCode));
        }

        public void ReportLeave(IPEndPoint hostEndpoint, ClientSessionId sessionId, Action<HttpStatusCode> responseHandler) {
            Post("report-leave", new ReportLeaveRequest(hostEndpoint, sessionId), (statusCode, data) => {
                responseHandler(statusCode);
            });
        }

        private void Post<T>(string uri, T payload, ResponseHandler onComplete) {
            byte[] data;
            string contentType;
            if (payload == null) {
                contentType = null;
                data = new byte[0];
            } else {
                using (var stringWriter = new StringWriter())
                using (var jsonWriter = new JsonTextWriter(stringWriter)) {
                    _jsonSerializer.Serialize(jsonWriter, payload);
                    data = Encoding.UTF8.GetBytes(stringWriter.ToString());
                }
                contentType = "application/json";
            }

            Send(uri, HttpMethod.Post, data, contentType, onComplete);
        }

        private void Get(string uri, ResponseHandler onComplete) {
            Send(uri, HttpMethod.Get, null, null, onComplete);
        }

        private void Send(string uri, HttpMethod method, byte[] data, string contentType, ResponseHandler onComplete) {
            Array.Clear(_customHeaders, 0, _customHeaders.Length);
            if (contentType != null) {
                 AddHeader("Content-Type", contentType);
            }
            AddHeader("Accept", "application/json, text/plain");
            var authToken = _authTokenProvider();
            AddHeader("X-Padrone-Auth", authToken.AsBase64());
            uri = _masterServerUrl + "/" + uri;
            _coroutineScheduler.Run(ExecuteWebRequest(uri, method, data, _customHeaders, onComplete));
        }

        private void AddHeader(string name, string value) {
            for (int i = 0; i < _customHeaders.Length; i++) {
                if (_customHeaders[i] == null) {
                    _customHeaders[i] = name;
                    _customHeaders[i + 1] = value;
                    break;
                }
            }
        }

        // Todo: this used to pool WWW instances and reuse them via WWW.InitWWW - that method was
        // removed entirely from modern Unity's (legacy, pre-UnityWebRequest) WWW class, only its
        // constructors remain. The master server itself is unreachable regardless (see
        // MainMenu.cs/UnityMasterServerClient.cs), so rather than rewriting this transport onto
        // UnityWebRequest for a service nothing can reach, just report failure without making a
        // real request.
        private IEnumerator<WaitCommand> ExecuteWebRequest(string uri, HttpMethod method, byte[] data, string[] headers,
            ResponseHandler onComplete) {
            Debug.Log("request " + uri + ", method " + method);

            yield return WaitCommand.WaitForNextFrame;

            onComplete(HttpStatusCode.ServiceUnavailable, "");
        }
    }
}
