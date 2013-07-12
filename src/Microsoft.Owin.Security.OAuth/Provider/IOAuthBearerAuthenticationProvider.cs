// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public interface IOAuthBearerAuthenticationProvider
    {
        Task RequestToken(OAuthRequestTokenContext context);
        Task ValidateIdentity(OAuthValidateIdentityContext context);
    }

    public class OAuthRequestTokenContext : BaseContext
    {
        public OAuthRequestTokenContext(
            IOwinContext context,
            string token) : base(context)
        {
            Token = token;
        }

        public string Token { get; set; }
    }
}
