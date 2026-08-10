using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Padrone.Client {
    public enum HttpMethod { Get, Post }

    public static class Util {

        public static string ToHexString(this byte[] bytes, int startIndex, int endIndex) {
            var length = endIndex - startIndex;
            var result = new StringBuilder(length * 2);
            for (int i = 0; i < length; i++) {
                var b = bytes[i + startIndex];
                const string hexAlphabet = "0123456789ABCDEF";
                result.Append(hexAlphabet[b >> 4]);
                result.Append(hexAlphabet[b & 0xF]);
            }
            return result.ToString();
        }

        public class WWWPool {
            public static readonly byte[] EmptyPostData = new byte[0];

            private readonly Queue<WWW> _getRequestPool;
            private readonly Queue<WWW> _postRequestPool;

            private readonly IDictionary<WWW, HttpMethod> _unpooledRequests;

            public WWWPool(int size) {
                _getRequestPool = new Queue<WWW>(size);
                _postRequestPool = new Queue<WWW>(size);
                _unpooledRequests = new Dictionary<WWW, HttpMethod>();

                for (int i = 0; i < size; i++) {
                    _getRequestPool.Enqueue(new WWW(""));
                    _postRequestPool.Enqueue(new WWW("", EmptyPostData));
                }
            }

            public WWW Take(HttpMethod method) {
                WWW request;
                if (method == HttpMethod.Get) {
                    if (_getRequestPool.Count == 0) {
                        throw new Exception("Pool is empty");
                    }
                    request = _getRequestPool.Dequeue();
                } else {
                    if (_postRequestPool.Count == 0) {
                        throw new Exception("Pool is empty");
                    }
                    request = _postRequestPool.Dequeue();
                }

                _unpooledRequests[request] = method;

                return request;
            }

            public void Return(WWW request) {
                var method = _unpooledRequests[request];
                if (method == HttpMethod.Get) {
                    _getRequestPool.Enqueue(request);
                } else {
                    _postRequestPool.Enqueue(request);
                }
            }

            public int Count {
                get { return _getRequestPool.Count; }
            }
        }
    }
}
