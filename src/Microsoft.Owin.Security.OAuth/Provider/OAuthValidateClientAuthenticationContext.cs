// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Text;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthValidateClientAuthenticationContext : BaseValidatingClientContext
    {
        public OAuthValidateClientAuthenticationContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            IReadableStringCollection parameters)
            : base(context, options, null)
        {
            Parameters = parameters;
        }

        public IReadableStringCollection Parameters { get; private set; }

        public bool Validated(string clientId)
        {
            ClientId = clientId;

            return Validated();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Justification = "Optimized for usage")]
        public bool TryGetBasicCredentials(out string clientId, out string clientSecret)
        {
            // Client Authentication http://tools.ietf.org/html/rfc6749#section-2.3
            // Client Authentication Password http://tools.ietf.org/html/rfc6749#section-2.3.1
            string authorization = Request.Headers.Get("Authorization");
            if (!string.IsNullOrWhiteSpace(authorization) && authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                byte[] data = Convert.FromBase64String(authorization.Substring("Basic ".Length).Trim());
                string text = Encoding.UTF8.GetString(data);
                int delimiterIndex = text.IndexOf(':');
                if (delimiterIndex >= 0)
                {
                    clientId = text.Substring(0, delimiterIndex);
                    clientSecret = text.Substring(delimiterIndex + 1);
                    ClientId = clientId;
                    return true;
                }
            }

            clientId = null;
            clientSecret = null;
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Justification = "Optimized for usage")]
        public bool TryGetFormCredentials(out string clientId, out string clientSecret)
        {
            clientId = Parameters.Get(Constants.Parameters.ClientId);
            if (!String.IsNullOrEmpty(clientId))
            {
                clientSecret = Parameters.Get(Constants.Parameters.ClientSecret);
                ClientId = clientId;
                return true;
            }
            clientId = null;
            clientSecret = null;
            return false;
        }
    }
}
