// <copyright file="GoogleAuthenticationContext.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Owin.Security.Google.Infrastructure;
using Microsoft.Owin.Security.Infrastructure;
using Owin.Types;
using Owin.Types.Extensions;

namespace Microsoft.Owin.Security.Google
{
    internal class GoogleAuthenticationContext
    {
        private static readonly Action<object> ApplyResponseDelegate = obj => ((GoogleAuthenticationContext)obj).ApplyResponse();

        private readonly GoogleAuthenticationOptions _options;
        private readonly IDictionary<string, object> _description;
        private SecurityHelper _helper;
        private OwinRequest _request;
        private OwinResponse _response;
        private Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task> _chainAuthenticate;

        private Task<IIdentity> _getIdentity;
        private bool _getIdentityInitialized;
        private object _getIdentitySyncLock;

        private bool _applyChallenge;
        private bool _applyChallengeInitialized;
        private object _applyChallengeSyncLock;
        private string _requestPathBase;

        public GoogleAuthenticationContext(
            GoogleAuthenticationOptions options,
            IDictionary<string, object> description,
            IDictionary<string, object> env)
        {
            _options = options;
            _description = description;
            _request = new OwinRequest(env);
            _response = new OwinResponse(env);
            _helper = new SecurityHelper(env);
        }

        public async Task Initialize()
        {
            _chainAuthenticate = _request.AuthenticateDelegate;
            _request.AuthenticateDelegate = Authenticate;

            _requestPathBase = _request.PathBase;

            _request.OnSendingHeaders(ApplyResponseDelegate, this);

            if (_options.AuthenticationMode == AuthenticationMode.Active)
            {
                await ApplyIdentity();
            }
        }

        public void Teardown()
        {
            ApplyResponse();
        }

        private async Task ApplyIdentity()
        {
            IIdentity identity = await GetIdentity();
            if (identity != null)
            {
                _helper.AddUserIdentity(identity);
            }
        }

        private async Task Authenticate(
            string[] authenticationTypes,
            Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object> callback,
            object state)
        {
            if (authenticationTypes == null)
            {
                callback(null, null, _description, state);
            }
            else if (authenticationTypes.Contains(_options.AuthenticationType, StringComparer.Ordinal))
            {
                IIdentity identity = await GetIdentity();
                if (identity != null)
                {
                    callback(identity, null, _description, state);
                }
            }
            if (_chainAuthenticate != null)
            {
                await _chainAuthenticate(authenticationTypes, callback, state);
            }
        }

        private Task<IIdentity> GetIdentity()
        {
            return LazyInitializer.EnsureInitialized(
                ref _getIdentity,
                ref _getIdentityInitialized,
                ref _getIdentitySyncLock,
                GetIdentityOnce);
        }

        private async Task<IIdentity> GetIdentityOnce()
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                // TODO: trace
                return null;
            }
        }

        private bool ApplyResponse()
        {
            return LazyInitializer.EnsureInitialized(
                ref _applyChallenge,
                ref _applyChallengeInitialized,
                ref _applyChallengeSyncLock,
                ApplyResponseOnce);
        }

        private bool ApplyResponseOnce()
        {
            ApplyResponseGrant();
            ApplyResponseChallenge();
            return default(bool);
        }

        private void ApplyResponseGrant()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "MemoryStream.Dispose is idempotent")]
        private void ApplyResponseChallenge()
        {
            if (_response.StatusCode != 401)
            {
                return;
            }

            Tuple<string[], IDictionary<string, string>> challenge = _helper.LookupChallenge(_options.AuthenticationType, _options.AuthenticationMode);

            if (challenge != null)
            {
                string requestPrefix = _request.Scheme + "://" + _request.Host;

                IDictionary<string, string> extra = challenge.Item2 ?? new Dictionary<string, string>(StringComparer.Ordinal);

                if (!extra.ContainsKey("security.ReturnUri"))
                {
                    string currentQueryString = _request.QueryString;
                    string currentUri = string.IsNullOrEmpty(currentQueryString)
                        ? requestPrefix + _request.PathBase + _request.Path
                        : requestPrefix + _request.PathBase + _request.Path + "?" + currentQueryString;
                    extra["security.ReturnUri"] = currentUri;
                }

                byte[] protectedData;
                using (var memory = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(memory))
                    {
                        writer.Write(1);
                        writer.Write(extra.Count);
                        foreach (var kv in extra)
                        {
                            writer.Write(kv.Key);
                            writer.Write(kv.Value);
                        }
                        writer.Flush();
                        byte[] userData = memory.ToArray();
                        protectedData = _options.DataProtection.Protect(userData);
                    }
                }

                string redirectUri = requestPrefix + _requestPathBase + _options.ReturnPath + "?state=" + Uri.EscapeDataString(Convert.ToBase64String(protectedData));

                string authorizationEndpoint =
                    "https://www.google.com/accounts/o8/ud" +
                        "?openid.ns=" + Uri.EscapeDataString("http://specs.openid.net/auth/2.0") +
                        "&openid.ns.ax=" + Uri.EscapeDataString("http://openid.net/srv/ax/1.0") +
                        "&openid.mode=" + Uri.EscapeDataString("checkid_setup") +
                        "&openid.claimed_id=" + Uri.EscapeDataString("http://specs.openid.net/auth/2.0/identifier_select") +
                        "&openid.identity=" + Uri.EscapeDataString("http://specs.openid.net/auth/2.0/identifier_select") +
                        "&openid.return_to=" + Uri.EscapeDataString(redirectUri) +
                        "&openid.realm=" + Uri.EscapeDataString(requestPrefix) +
                        "&openid.ax.mode=" + Uri.EscapeDataString("fetch_request") +
                        "&openid.ax.type.email=" + Uri.EscapeDataString("http://axschema.org/contact/email") +
                        "&openid.ax.type.name=" + Uri.EscapeDataString("http://axschema.org/namePerson") +
                        "&openid.ax.type.first=" + Uri.EscapeDataString("http://axschema.org/namePerson/first") +
                        "&openid.ax.type.last=" + Uri.EscapeDataString("http://axschema.org/namePerson/last") +
                        "&openid.ax.required=" + Uri.EscapeDataString("email,name,first,last");

                _response.StatusCode = 302;
                _response.SetHeader("Location", authorizationEndpoint);
            }
        }

        public async Task<bool> Invoke()
        {
            return await InvokeReturnPath();
        }

        public async Task<bool> InvokeReturnPath()
        {
            if (_options.ReturnPath != null &&
                String.Equals(_options.ReturnPath, _request.Path, StringComparison.OrdinalIgnoreCase))
            {
                IDictionary<string, string[]> query = _request.GetQuery();

                Dictionary<string, string> extra = null;
                string[] values;
                if (query.TryGetValue("state", out values) && values.Length == 1)
                {
                    byte[] protectedData = Convert.FromBase64String(values[0]);
                    byte[] userData = _options.DataProtection.Unprotect(protectedData);
                    using (var memory = new MemoryStream(userData))
                    {
                        using (var reader = new BinaryReader(memory))
                        {
                            int version = reader.ReadInt32();
                            if (version != 1)
                            {
                                return false;
                            }
                            int count = reader.ReadInt32();
                            extra = new Dictionary<string, string>(count);
                            for (int index = 0; index != count; ++index)
                            {
                                string key = reader.ReadString();
                                string value = reader.ReadString();
                                extra.Add(key, value);
                            }
                        }
                    }
                }

                var message = new Message(query);
                bool messageValidated = false;

                Property mode;
                if (message.Properties.TryGetValue("mode.http://specs.openid.net/auth/2.0", out mode) &&
                    string.Equals("id_res", mode.Value, StringComparison.Ordinal))
                {
                    mode.Value = "check_authentication";

                    WebRequest verifyRequest = WebRequest.Create("https://www.google.com/accounts/o8/ud");
                    verifyRequest.Method = "POST";
                    verifyRequest.ContentType = "application/x-www-form-urlencoded";
                    using (var writer = new StreamWriter(await verifyRequest.GetRequestStreamAsync()))
                    {
                        string body = message.ToFormUrlEncoded();
                        await writer.WriteAsync(body);
                    }
                    WebResponse verifyResponse = await verifyRequest.GetResponseAsync();
                    using (var reader = new StreamReader(verifyResponse.GetResponseStream()))
                    {
                        var verifyBody = new Dictionary<string, string[]>();
                        string body = await reader.ReadToEndAsync();
                        foreach (var line in body.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            int delimiter = line.IndexOf(':');
                            if (delimiter != -1)
                            {
                                verifyBody.Add("openid." + line.Substring(0, delimiter), new[] { line.Substring(delimiter + 1) });
                            }
                        }
                        var verifyMessage = new Message(verifyBody);
                        Property isValid;
                        if (verifyMessage.Properties.TryGetValue("is_valid.http://specs.openid.net/auth/2.0", out isValid))
                        {
                            if (string.Equals("true", isValid.Value, StringComparison.Ordinal))
                            {
                                messageValidated = true;
                            }
                        }
                    }
                }

                // TODO: openid-authentication-2.0 11.* Verifying assertions
                if (messageValidated)
                {
                    IDictionary<string, string> attributeExchangeResponse = new Dictionary<string, string>();
                    foreach (var typeProperty in message.Properties.Values)
                    {
                        if (typeProperty.Namespace == "http://openid.net/srv/ax/1.0" &&
                            typeProperty.Name.StartsWith("type."))
                        {
                            string qname = "value." + typeProperty.Name.Substring("type.".Length) + "http://openid.net/srv/ax/1.0";
                            Property valueProperty;
                            if (message.Properties.TryGetValue(qname, out valueProperty))
                            {
                                attributeExchangeResponse.Add(typeProperty.Value, valueProperty.Value);
                            }
                        }
                    }

                    var responseNamespaces = new object[]
                    {
                        new XAttribute(XNamespace.Xmlns + "openid", "http://specs.openid.net/auth/2.0"), 
                        new XAttribute(XNamespace.Xmlns + "openid.ax", "http://openid.net/srv/ax/1.0")
                    };

                    var responseProperties = message.Properties
                        .Where(p => p.Value.Namespace != null)
                        .Select(p => (object)new XElement(XName.Get(p.Value.Name.Substring(0, p.Value.Name.Length - 1), p.Value.Namespace), p.Value.Value));

                    var responseMessage = new XElement("response", responseNamespaces.Concat(responseProperties).ToArray());

                    var context = new GoogleValidateLoginContext(
                        _request.Dictionary,
                        extra,
                        responseMessage,
                        attributeExchangeResponse);

                    await _options.Provider.ValidateLogin(context);
                    if (context.SigninPrincipal != null)
                    {
                        _response.SignIn(context.SigninPrincipal);
                    }
                    if (!string.IsNullOrEmpty(context.RedirectUri))
                    {
                        _response.Redirect(context.RedirectUri);
                        return true;
                    }
                }

                return false;
            }
            return false;
        }
    }
}
