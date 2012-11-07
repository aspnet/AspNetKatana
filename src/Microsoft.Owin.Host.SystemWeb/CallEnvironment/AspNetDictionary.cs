// <copyright file="AspNetDictionary.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Microsoft.Owin.Host.SystemWeb.CallHeaders;

namespace Microsoft.Owin.Host.SystemWeb.CallEnvironment
{
    internal sealed partial class AspNetDictionary : IDictionary<string, object>
    {
        private static readonly IDictionary<string, object> WeakNilEnvironment = new NilDictionary();

        private readonly OwinCallContext _callContext;
        private readonly RequestContext _requestContext;
        private readonly HttpContextBase _httpContext;
        private readonly HttpRequestBase _httpRequest;
        private HttpResponseBase _httpResponse;
        private IDictionary<string, object> _extra = WeakNilEnvironment;

        internal AspNetDictionary(OwinCallContext callContext, RequestContext requestContext)
        {
            _callContext = callContext;
            _requestContext = requestContext;
            _httpContext = requestContext.HttpContext;
            _httpRequest = _httpContext.Request;
            _httpResponse = _httpContext.Response;
        }

        internal IDictionary<string, object> Extra
        {
            get { return _extra; }
        }

        private IDictionary<string, object> StrongExtra
        {
            get
            {
                if (_extra == WeakNilEnvironment)
                {
                    Interlocked.CompareExchange(ref _extra, new Dictionary<string, object>(), WeakNilEnvironment);
                }
                return _extra;
            }
        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                object value;
                return PropertiesTryGetValue(key, out value) ? value : Extra[key];
            }
            set
            {
                if (!PropertiesTrySetValue(key, value))
                {
                    StrongExtra[key] = value;
                }
            }
        }

        void IDictionary<string, object>.Add(string key, object value)
        {
            if (!PropertiesTrySetValue(key, value))
            {
                StrongExtra.Add(key, value);
            }
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return PropertiesContainsKey(key) || Extra.ContainsKey(key);
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return PropertiesKeys().Concat(Extra.Keys).ToArray(); }
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            // Although this is a mutating operation, Extra is used instead of StrongExtra,
            // because if a real dictionary has not been allocated the default behavior of the
            // nil dictionary is perfectly fine.
            return PropertiesTryRemove(key) || Extra.Remove(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            return PropertiesTryGetValue(key, out value) || Extra.TryGetValue(key, out value);
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { return PropertiesValues().Concat(Extra.Values).ToArray(); }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>)this).Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            foreach (var key in PropertiesKeys())
            {
                PropertiesTryRemove(key);
            }
            Extra.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            object value;
            return ((IDictionary<string, object>)this).TryGetValue(item.Key, out value) && Object.Equals(value, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            PropertiesEnumerable().Concat(Extra).ToArray().CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get { return PropertiesKeys().Count() + Extra.Count; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)this).Contains(item) &&
                ((IDictionary<string, object>)this).Remove(item.Key);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return PropertiesEnumerable().Concat(Extra).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, object>)this).GetEnumerator();
        }

        string InitOwinVersion()
        {
            return "1.0";
        }
        CancellationToken InitCallCancelled()
        {
            return _callContext.BindDisconnectNotification();
        }
        string InitRequestProtocol()
        {
            return _httpRequest.ServerVariables["SERVER_PROTOCOL"];
        }
        string InitRequestMethod()
        {
            return _httpRequest.HttpMethod;
        }
        string InitRequestScheme()
        {
            return _httpRequest.IsSecureConnection ? "https" : "http";
        }
        string InitRequestPathBase()
        {
            return null;
        }
        string InitRequestPath()
        {
            return null;
        }
        string InitRequestQueryString()
        {
            return _callContext.GetQuery();
        }
        IDictionary<string, string[]> InitRequestHeaders()
        {
            return new AspNetRequestHeaders(_httpRequest.Headers);
        }
        Stream InitRequestBody()
        {
            return _httpRequest.InputStream;
        }
        IDictionary<string, string[]> InitResponseHeaders()
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }
        TextWriter InitHostTraceOutput()
        {
            return TraceTextWriter.Instance;
        }
        string InitHostAppMode()
        {
            return _callContext.GetAppMode();
        }
        System.Security.Principal.IPrincipal InitServerUser()
        {
            return _httpContext.User;
        }
        string InitServerRemoteIpAddress()
        {
            return _httpRequest.ServerVariables["REMOTE_ADDR"];
        }
        string InitServerRemotePort()
        {
            return _httpRequest.ServerVariables["REMOTE_PORT"];
        }
        string InitServerLocalIpAddress()
        {
            return _httpRequest.ServerVariables["LOCAL_ADDR"];
        }
        string InitServerLocalPort()
        {
            return _httpRequest.ServerVariables["SERVER_PORT"];
        }
        bool InitServerIsLocal()
        {
            return _httpRequest.IsLocal;
        }
        X509Certificate InitClientCert()
        {
            return _callContext.LoadClientCert();
        }
        Func<Task> InitLoadClientCert()
        {
            return _callContext.GetLoadClientCert();
        }
        object InitWebSocketAccept()
        {
            return _callContext.BindWebSocketAccept();
        }
        RequestContext InitRequestContext()
        {
            return _requestContext;
        }
        HttpContextBase InitHttpContextBase()
        {
            return _httpContext;
        }
    }
}
