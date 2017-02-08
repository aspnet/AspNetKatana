// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Google;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.Google
{
    public class GoogleOAuth2AuthorizeParameters
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.HttpListener)]
        public async Task Security_GoogleOAuth2AuthorizeParameters(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, GoogleOAuth2Configuration);
                var handler = new HttpClientHandler() { AllowAutoRedirect = false };
                var httpClient = new HttpClient(handler);

                // Unauthenticated request - verify Redirect url
                var response = await httpClient.GetAsync(applicationUrl);
                Assert.Equal<string>("https://accounts.google.com/o/oauth2/auth", response.Headers.Location.AbsoluteUri.Replace(response.Headers.Location.Query, string.Empty));
                var queryItems = response.Headers.Location.ParseQueryString();
                Assert.Equal<string>("custom_accessType", queryItems["access_type"]);
                Assert.Equal<string>("custom_approval_prompt", queryItems["approval_prompt"]);
                Assert.Equal<string>("custom_login_hint", queryItems["login_hint"]);
            }
        }

        public void GoogleOAuth2Configuration(IAppBuilder app)
        {
            app.UseAuthSignInCookie();

            var option = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "581497791735.apps.googleusercontent.com",
                ClientSecret = "-N8rQkJ_MKbhpaxyjdVYbFpO",
            };

            app.UseGoogleAuthentication(option);

            app.Run(async context =>
                {
                    if (context.Authentication.User == null || !context.Authentication.User.Identity.IsAuthenticated)
                    {
                        var authenticationProperties = new AuthenticationProperties();
                        authenticationProperties.Dictionary.Add("access_type", "custom_accessType");
                        authenticationProperties.Dictionary.Add("approval_prompt", "custom_approval_prompt");
                        authenticationProperties.Dictionary.Add("login_hint", "custom_login_hint");

                        context.Authentication.Challenge(authenticationProperties, "Google");
                        await context.Response.WriteAsync("Unauthorized");
                    }
                });
        }
    }
}
