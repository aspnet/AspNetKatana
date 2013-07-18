// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthValidateCustomGrantContext : BaseContext
    {
        public OAuthValidateCustomGrantContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            string clientId,
            string grantType,
            IReadableStringCollection parameters)
            : base(context)
        {
            Options = options;
            ClientId = clientId;
            GrantType = grantType;
            Parameters = parameters;
        }

        public OAuthAuthorizationServerOptions Options { get; private set; }
        public string ClientId { get; private set; }
        public string GrantType { get; private set; }
        public IReadableStringCollection Parameters { get; private set; }

        public ClaimsIdentity Identity { get; private set; }
        public IDictionary<string, string> Extra { get; private set; }

        public bool IsValidated { get; private set; }

        public bool HasError { get; private set; }
        public string Error { get; private set; }
        public string ErrorDescription { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "error_uri is a string value in the protocol")]
        public string ErrorUri { get; private set; }

        public void Validated(ClaimsIdentity identity, IDictionary<string, string> extra)
        {
            Identity = identity;
            Extra = extra;
            IsValidated = true;
            HasError = false;
        }

        public void SetError(string error)
        {
            SetError(error, null);
        }

        public void SetError(string error,
            string errorDescription)
        {
            SetError(error, errorDescription, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "error_uri is a string value in the protocol")]
        public void SetError(string error,
            string errorDescription,
            string errorUri)
        {
            Error = error;
            ErrorDescription = errorDescription;
            ErrorUri = errorUri;
            HasError = true;
            IsValidated = false;
        }
    }
}
