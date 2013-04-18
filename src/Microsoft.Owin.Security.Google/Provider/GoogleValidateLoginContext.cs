// <copyright file="GoogleValidateLoginContext.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Xml.Linq;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Google
{
    public class GoogleReturnEndpointContext : ReturnEndpointContext
    {
        public GoogleReturnEndpointContext(IDictionary<string, object> environment, ClaimsIdentity identity, IDictionary<string, string> extra) : base(environment, identity, extra)
        {
        }
    }

    public class GoogleValidateLoginContext
    {
        public GoogleValidateLoginContext(
            IDictionary<string, object> environment,
            IDictionary<string, string> extra,
            XElement responseMessage,
            IDictionary<string, string> attributeExchangeProperties)
        {
            Environment = environment;
            Extra = extra;
            ResponseMessage = responseMessage;
            AttributeExchangeProperties = attributeExchangeProperties;

            XElement claimedId = responseMessage.Element(XName.Get("claimed_id", "http://specs.openid.net/auth/2.0"));
            if (claimedId != null)
            {
                Id = claimedId.Value;
            }

            string value;
            if (attributeExchangeProperties.TryGetValue("http://axschema.org/contact/email", out value))
            {
                Email = value;
            }
            if (attributeExchangeProperties.TryGetValue("http://axschema.org/namePerson", out value))
            {
                Name = value;
            }
            if (attributeExchangeProperties.TryGetValue("http://axschema.org/namePerson/first", out value))
            {
                FirstName = value;
            }
            if (attributeExchangeProperties.TryGetValue("http://axschema.org/namePerson/last", out value))
            {
                LastName = value;
            }
        }

        public IDictionary<string, object> Environment { get; private set; }
        public IDictionary<string, string> Extra { get; private set; }
        public XElement ResponseMessage { get; set; }

        public IDictionary<string, string> AttributeExchangeProperties { get; private set; }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }

        public IPrincipal SigninPrincipal { get; private set; }

        public string RedirectUri
        {
            get
            {
                string value;
                return Extra.TryGetValue("security.ReturnUri", out value) ? value : null;
            }
            private set { Extra["security.ReturnUri"] = value; }
        }

        public void SignIn(IPrincipal principal)
        {
            SigninPrincipal = principal;
        }

        public void CancelSignIn()
        {
            SigninPrincipal = null;
        }

        public void Redirect(string redirectUri)
        {
            RedirectUri = redirectUri;
        }

        public void CancelRedirect()
        {
            RedirectUri = null;
        }
    }
}
