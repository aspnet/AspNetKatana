// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FunctionalTests.Common;
using FunctionalTests.Facts.Security.Common;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.WsFederation;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.Federation
{
    public class WsFederationTest
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void Security_WsFederationAuthentication(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, WsFederationAuthenticationConfiguration);

                var handler = new HttpClientHandler() { AllowAutoRedirect = false };
                var httpClient = new HttpClient(handler);

                //Set the right metadata document in the server cache
                var kvps = new List<KeyValuePair<string, string>>();
                kvps.Add(new KeyValuePair<string, string>("metadata", File.ReadAllText(@"Facts\Security\Federation\federationmetadata.xml")));
                httpClient.PostAsync(applicationUrl + "metadata", new FormUrlEncodedContent(kvps));

                //Verify if the request is redirected to STS with right parameters
                var response = httpClient.GetAsync(applicationUrl).Result;
                Assert.Equal<string>("https://login.windows.net/4afbc689-805b-48cf-a24c-d4aa3248a248/wsfed", response.Headers.Location.AbsoluteUri.Replace(response.Headers.Location.Query, string.Empty));
                var queryItems = response.Headers.Location.ParseQueryString();
                Assert.Equal<string>("http://Automation1", queryItems["wtrealm"]);
                Assert.True(queryItems["wctx"].StartsWith("WsFedOwinState="), "wctx does not start with a WsFedOwinState=");
                Assert.True(queryItems["mystate"].EndsWith("customValue"), "wctx does not end with a &mystate=customValue");
                Assert.Equal<string>(applicationUrl + "signin-wsfed", queryItems["wreply"]);
                Assert.Equal<string>("wsignin1.0", queryItems["wa"]);

                //Send an invalid token and verify that the token is not honored
                httpClient = new HttpClient();
                kvps = new List<KeyValuePair<string, string>>();
                kvps.Add(new KeyValuePair<string, string>("wa", "wsignin1.0"));
                kvps.Add(new KeyValuePair<string, string>("wresult", File.ReadAllText(@"Facts\Security\Federation\InvalidToken.xml")));
                kvps.Add(new KeyValuePair<string, string>("wctx", queryItems["wctx"]));
                response = httpClient.PostAsync(queryItems["wreply"], new FormUrlEncodedContent(kvps)).Result;

                //Did the request end in the actual resource requested for
                Assert.Equal<string>("AuthenticationFailed", response.Content.ReadAsStringAsync().Result);
            }
        }

        //Store the metadata once and reuse the same
        public static string metadataXml;
        public void WsFederationAuthenticationConfiguration(IAppBuilder app)
        {
            app.UseStaticFiles();

            app.UseAuthSignInCookie();

            app.UseWsFederationAuthentication(new WsFederationAuthenticationOptions()
                {
                    Wtrealm = "http://Automation1",
                    MetadataAddress = "https://login.windows.net/4afbc689-805b-48cf-a24c-d4aa3248a248/federationmetadata/2007-06/federationmetadata.xml",
                    BackchannelHttpHandler = new WaadMetadataDocumentHandler(),
                    StateDataFormat = new CustomStateDataFormat(),
                    Notifications = new WsFederationAuthenticationNotifications()
                    {
                        MessageReceived = notification =>
                            {
                                Assert.True(notification.ProtocolMessage.Wctx.EndsWith("&mystate=customValue"), "wctx is not ending with &mystate=customValue");
                                notification.ProtocolMessage.Wctx = notification.ProtocolMessage.Wctx.Replace("&mystate=customValue", string.Empty);
                                notification.OwinContext.Set<bool>("MessageReceived", true);
                                return Task.FromResult(0);
                            },
                        RedirectToIdentityProvider = notification =>
                            {
                                if (notification.ProtocolMessage.IsSignInMessage)
                                {
                                    //Sign in message
                                    notification.ProtocolMessage.Wreply = notification.Request.Uri.AbsoluteUri + "signin-wsfed";
                                    notification.ProtocolMessage.Wctx += "&mystate=customValue";
                                }

                                return Task.FromResult(0);
                            },
                        SecurityTokenReceived = notification =>
                            {
                                notification.OwinContext.Set<bool>("SecurityTokenReceived", true);
                                return Task.FromResult(0);
                            },
                        SecurityTokenValidated = notification =>
                            {
                                var context = notification.AuthenticationTicket;

                                Assert.True(notification.OwinContext.Get<bool>("MessageReceived"), "MessageReceived notification not invoked");
                                Assert.True(notification.OwinContext.Get<bool>("SecurityTokenReceived"), "SecurityTokenReceived notification not invoked");

                                if (context.Identity != null)
                                {
                                    context.Identity.AddClaim(new Claim("ReturnEndpoint", "true"));
                                    context.Identity.AddClaim(new Claim("Authenticated", "true"));
                                    context.Identity.AddClaim(new Claim(context.Identity.RoleClaimType, "Guest", ClaimValueTypes.String));
                                }

                                return Task.FromResult(0);
                            },
                        AuthenticationFailed = notification =>
                            {
                                //Change the request url to something different and skip Wsfed. This new url will handle the request and let us know if this notification was invoked.
                                notification.OwinContext.Request.Path = new PathString("/AuthenticationFailed");
                                notification.SkipToNextMiddleware();
                                return Task.FromResult(0);
                            }
                    }
                });

            app.Map("/Logout", subApp =>
                {
                    subApp.Run(async context =>
                        {
                            if (context.Authentication.User.Identity.IsAuthenticated)
                            {
                                var authProperties = new AuthenticationProperties() { RedirectUri = context.Request.Uri.AbsoluteUri };
                                context.Authentication.SignOut(authProperties, WsFederationAuthenticationDefaults.AuthenticationType);
                                await context.Response.WriteAsync("Signing out...");
                            }
                            else
                            {
                                await context.Response.WriteAsync("SignedOut");
                            }
                        });
                });

            app.Map("/AuthenticationFailed", subApp =>
            {
                subApp.Run(async context =>
                {
                    await context.Response.WriteAsync("AuthenticationFailed");
                });
            });

            app.Map("/signout-wsfed", subApp =>
            {
                subApp.Run(async context =>
                {
                    await context.Response.WriteAsync("signout-wsfed");
                });
            });

            #region Utilities to set the metadata xml.
            app.Map("/metadata", subApp =>
            {
                subApp.Run(async context =>
                {
                    if (context.Request.Method == "POST")
                    {
                        var formParameters = await context.Request.ReadFormAsync();
                        metadataXml = formParameters.GetValues("metadata")[0];
                        await context.Response.WriteAsync("Received metadata");
                    }
                    else
                    {
                        context.Response.ContentType = "text/xml";
                        await context.Response.WriteAsync(metadataXml);
                    }
                });
            });
            #endregion

            app.UseExternalApplication(WsFederationAuthenticationDefaults.AuthenticationType);
        }

        private class WaadMetadataDocumentHandler : WebRequestHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var newResponse = new HttpResponseMessage() { Content = new StringContent(WsFederationTest.metadataXml, Encoding.UTF8, "text/xml") };
                return Task.FromResult<HttpResponseMessage>(newResponse);
            }
        }
    }
}