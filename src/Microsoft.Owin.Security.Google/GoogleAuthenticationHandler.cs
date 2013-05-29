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
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Google.Infrastructure;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.Google
{
    internal class GoogleAuthenticationHandler : AuthenticationHandler<GoogleAuthenticationOptions>
    {
        private readonly ILogger _logger;
        private IDictionary<string, string> _errorDetails;

        public GoogleAuthenticationHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task<bool> Invoke()
        {
            if (Options.ReturnEndpointPath != null &&
                String.Equals(Options.ReturnEndpointPath, Request.Path, StringComparison.OrdinalIgnoreCase))
            {
                return await InvokeReturnPath();
            }
            return false;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCore()
        {
            _logger.WriteVerbose("AuthenticateCore");

            AuthenticationExtra extra = null;

            try
            {
                IDictionary<string, string[]> query = Request.GetQuery();

                extra = UnpackState(query);
                if (extra == null)
                {
                    _logger.WriteWarning("Invalid return state", null);
                    return null;
                }

                // Anti-CSRF
                if (!ValidateCorrelationId(extra, _logger))
                {
                    return new AuthenticationTicket(null, extra);
                }

                var message = await ParseRequestMessage(query);

                bool messageValidated = false;

                Property mode;
                if (!message.Properties.TryGetValue("mode.http://specs.openid.net/auth/2.0", out mode))
                {
                    _logger.WriteWarning("Missing mode parameter", null);
                    return new AuthenticationTicket(null, extra);
                }

                if (string.Equals("cancel", mode.Value, StringComparison.Ordinal))
                {
                    _logger.WriteWarning("User cancelled signin request", null);
                    return new AuthenticationTicket(null, extra);
                }

                if (string.Equals("id_res", mode.Value, StringComparison.Ordinal))
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
                        var verifyMessage = new Message(verifyBody, strict: false);
                        Property isValid;
                        if (verifyMessage.Properties.TryGetValue("is_valid.http://specs.openid.net/auth/2.0", out isValid))
                        {
                            if (string.Equals("true", isValid.Value, StringComparison.Ordinal))
                            {
                                messageValidated = true;
                            }
                            else
                            {
                                messageValidated = false;
                            }
                        }
                    }
                }

                // TODO: openid-authentication-2.0 11.* Verifying assertions
                if (messageValidated)
                {
                    IDictionary<string, string> attributeExchangeProperties = new Dictionary<string, string>();
                    foreach (var typeProperty in message.Properties.Values)
                    {
                        if (typeProperty.Namespace == "http://openid.net/srv/ax/1.0" &&
                            typeProperty.Name.StartsWith("type."))
                        {
                            string qname = "value." + typeProperty.Name.Substring("type.".Length) + "http://openid.net/srv/ax/1.0";
                            Property valueProperty;
                            if (message.Properties.TryGetValue(qname, out valueProperty))
                            {
                                attributeExchangeProperties.Add(typeProperty.Value, valueProperty.Value);
                            }
                        }
                    }

                    var responseNamespaces = new object[]
                    {
                        new XAttribute(XNamespace.Xmlns + "openid", "http://specs.openid.net/auth/2.0"),
                        new XAttribute(XNamespace.Xmlns + "openid.ax", "http://openid.net/srv/ax/1.0")
                    };

                    IEnumerable<object> responseProperties = message.Properties
                        .Where(p => p.Value.Namespace != null)
                        .Select(p => (object)new XElement(XName.Get(p.Value.Name.Substring(0, p.Value.Name.Length - 1), p.Value.Namespace), p.Value.Value));

                    var responseMessage = new XElement("response", responseNamespaces.Concat(responseProperties).ToArray());

                    var identity = new ClaimsIdentity(Options.AuthenticationType);
                    XElement claimedId = responseMessage.Element(XName.Get("claimed_id", "http://specs.openid.net/auth/2.0"));
                    if (claimedId != null)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, claimedId.Value, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                    }

                    string firstValue;
                    if (attributeExchangeProperties.TryGetValue("http://axschema.org/namePerson/first", out firstValue))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.GivenName, firstValue, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                    }
                    string lastValue;
                    if (attributeExchangeProperties.TryGetValue("http://axschema.org/namePerson/last", out lastValue))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Surname, lastValue, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                    }
                    string nameValue;
                    if (attributeExchangeProperties.TryGetValue("http://axschema.org/namePerson", out nameValue))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Name, nameValue, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                    }
                    else if (!string.IsNullOrEmpty(firstValue) && !string.IsNullOrEmpty(lastValue))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Name, firstValue + " " + lastValue, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                    }
                    else if (!string.IsNullOrEmpty(firstValue))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Name, firstValue, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                    }
                    else if (!string.IsNullOrEmpty(lastValue))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Name, lastValue, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                    }
                    string emailValue;
                    if (attributeExchangeProperties.TryGetValue("http://axschema.org/contact/email", out emailValue))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Email, emailValue, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                    }

                    var context = new GoogleAuthenticatedContext(
                        Request.Environment,
                        identity,
                        extra,
                        responseMessage,
                        attributeExchangeProperties);

                    await Options.Provider.Authenticated(context);

                    return new AuthenticationTicket(context.Identity, context.Extra);
                }

                return new AuthenticationTicket(null, extra);
            }
            catch (Exception ex)
            {
                _logger.WriteError("Authentication failed", ex);
                return new AuthenticationTicket(null, extra);
            }
        }

        private AuthenticationExtra UnpackState(IDictionary<string, string[]> query)
        {
            string[] values;
            if (query.TryGetValue("state", out values) && values.Length == 1)
            {
                return Options.StateDataHandler.Unprotect(values[0]);
            }
            return null;
        }

        private async Task<Message> ParseRequestMessage(IDictionary<string, string[]> query)
        {
            if (Request.Method == "POST")
            {
                var form = new Dictionary<string, string[]>();
                await Request.ReadForm(form);
                return new Message(form, strict: true);
            }
            return new Message(query, strict: true);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "MemoryStream.Dispose is idempotent")]
        protected override async Task ApplyResponseChallenge()
        {
            if (Response.StatusCode != 401)
            {
                return;
            }

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                string requestPrefix = Request.Scheme + "://" + Request.Host;

                var state = challenge.Extra;
                if (string.IsNullOrEmpty(state.RedirectUrl))
                {
                    state.RedirectUrl = WebUtilities.AddQueryString(
                        requestPrefix + Request.PathBase + Request.Path, 
                        Request.QueryString);
                }

                // Anti-CSRF
                GenerateCorrelationId(state);
                
                string redirectUri = requestPrefix + RequestPathBase + Options.ReturnEndpointPath + "?state=" + Uri.EscapeDataString(Options.StateDataHandler.Protect(state));

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

                Response.StatusCode = 302;
                Response.SetHeader("Location", authorizationEndpoint);
            }
        }

        public async Task<bool> InvokeReturnPath()
        {
            var model = await Authenticate();

            var context = new GoogleReturnEndpointContext(Request.Environment, model, _errorDetails);
            context.SignInAsAuthenticationType = Options.SignInAsAuthenticationType;
            context.RedirectUri = model.Extra.RedirectUrl;
            model.Extra.RedirectUrl = null;

            await Options.Provider.ReturnEndpoint(context);

            if (context.SignInAsAuthenticationType != null && context.Identity != null)
            {
                ClaimsIdentity signInIdentity = context.Identity;
                if (!string.Equals(signInIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
                {
                    signInIdentity = new ClaimsIdentity(signInIdentity.Claims, context.SignInAsAuthenticationType, signInIdentity.NameClaimType, signInIdentity.RoleClaimType);
                }
                Response.Grant(signInIdentity, context.Extra);
            }

            if (!context.IsRequestCompleted && context.RedirectUri != null)
            {
                Response.Redirect(context.RedirectUri);
                context.RequestCompleted();
            }

            return context.IsRequestCompleted;
        }
    }
}
