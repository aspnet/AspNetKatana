// <copyright file="FederationAuthenticationContext.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Specialized;
using System.Globalization;
using System.IdentityModel.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Services;
using System.IdentityModel.Services.Configuration;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Owin.Security.Infrastructure;
using Owin.Types;
using Owin.Types.Extensions;
using Owin.Types.Helpers;

namespace Microsoft.Owin.Security.Federation
{
    internal class FederationAuthenticationHandler : AuthenticationHandler<FederationAuthenticationOptions>
    {
        private readonly FederationConfiguration _federationConfiguration;

        public FederationAuthenticationHandler(FederationConfiguration federationConfiguration)
        {
            _federationConfiguration = federationConfiguration;
        }

        public override async Task<bool> Invoke()
        {
            return await InvokeReplyPath();
        }

        protected override async Task<AuthenticationTicket> AuthenticateCore()
        {
            if (!string.Equals(Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var form = await Request.ReadForm();

            WSFederationMessage message = WSFederationMessage.CreateFromNameValueCollection(
                new Uri(_federationConfiguration.WsFederationConfiguration.Realm),
                form);

            var signIn = message as SignInResponseMessage;
            if (signIn == null)
            {
                return null;
            }

            XmlDictionaryReader xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(signIn.Result), XmlDictionaryReaderQuotas.Max);
            var federationSerializer = new WSFederationSerializer(xmlReader);
            var serializationContext = new WSTrustSerializationContext(_federationConfiguration.IdentityConfiguration.SecurityTokenHandlerCollectionManager);
            RequestSecurityTokenResponse securityTokenResponse = federationSerializer.CreateResponse(signIn, serializationContext);
            string xml = securityTokenResponse.RequestedSecurityToken.SecurityTokenXml.OuterXml;

            SecurityToken securityToken = ReadToken(xml);

            var securityTokenReceivedContext = new SecurityTokenReceivedContext(securityToken);
            await Options.Provider.SecurityTokenReceived(securityTokenReceivedContext);

            ClaimsPrincipal principal = AuthenticateToken(securityToken, Request.Uri.AbsoluteUri);

            var securityTokenValidatedContext = new SecurityTokenValidatedContext(principal);
            await Options.Provider.SecurityTokenValidated(securityTokenValidatedContext);

            return new AuthenticationTicket(
                securityTokenValidatedContext.ClaimsPrincipal.Identities.FirstOrDefault(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        protected override async Task ApplyResponseChallenge()
        {
            if (Response.StatusCode != 401)
            {
                return;
            }

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                string issuer = _federationConfiguration.WsFederationConfiguration.Issuer;
                string realm = _federationConfiguration.WsFederationConfiguration.Realm;
                var message = new SignInRequestMessage(new Uri(issuer), realm);
                message.Freshness = _federationConfiguration.WsFederationConfiguration.Freshness;
                message.CurrentTime = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture) + "Z";
                message.AuthenticationType = _federationConfiguration.WsFederationConfiguration.AuthenticationType;
                message.HomeRealm = _federationConfiguration.WsFederationConfiguration.HomeRealm;
                message.Policy = _federationConfiguration.WsFederationConfiguration.Policy;
                message.Reply = _federationConfiguration.WsFederationConfiguration.Reply;
                message.Resource = _federationConfiguration.WsFederationConfiguration.Resource;
                message.Request = _federationConfiguration.WsFederationConfiguration.Request;
                message.RequestPtr = _federationConfiguration.WsFederationConfiguration.RequestPtr;

                string prefix = Request.Scheme + "://" + Request.Host + Request.PathBase;

                string queryString = Request.QueryString;
                string redirectUri = string.IsNullOrEmpty(queryString)
                    ? prefix + Request.Path
                    : prefix + Request.Path + "?" + queryString;

                message.Context = redirectUri;

                Response.Redirect(message.RequestUrl);
            }
        }

        public async Task<bool> InvokeReplyPath()
        {
            if (Options.ReturnPath != null &&
                String.Equals(Options.ReturnPath, Request.Path, StringComparison.OrdinalIgnoreCase))
            {
                var model = await Authenticate();

                if (model == null)
                {
                    return false;
                }

                string redirectUri = model.Extra.RedirectUrl;

                if (!string.IsNullOrEmpty(Options.SigninAsAuthenticationType))
                {
                    ClaimsIdentity grantIdentity = model.Identity;
                    if (!string.Equals(grantIdentity.AuthenticationType, Options.SigninAsAuthenticationType, StringComparison.Ordinal))
                    {
                        grantIdentity = new ClaimsIdentity(grantIdentity.Claims, Options.SigninAsAuthenticationType, grantIdentity.NameClaimType, grantIdentity.RoleClaimType);
                    }

                    Response.Grant(grantIdentity, model.Extra.Properties);
                }
                Response.Redirect(redirectUri);
                return true;
            }
            return false;
        }

        private SecurityToken ReadToken(string text)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(text)))
            {
                reader.MoveToContent();

                SecurityTokenHandlerCollection handlers = _federationConfiguration.IdentityConfiguration.SecurityTokenHandlers;
                return handlers.CanReadToken(reader) ? handlers.ReadToken(reader) : null;
            }
        }

        private ClaimsPrincipal AuthenticateToken(SecurityToken token, string resourceName)
        {
            IdentityConfiguration identityConfiguration = _federationConfiguration.IdentityConfiguration;
            var incomingPrincipal = new ClaimsPrincipal(identityConfiguration.SecurityTokenHandlers.ValidateToken(token));
            return identityConfiguration.ClaimsAuthenticationManager.Authenticate(resourceName, incomingPrincipal);
        }
    }
}
