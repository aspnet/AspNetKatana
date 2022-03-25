// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Web;
using DotNetOpenAuth.OAuth2;

namespace Katana.Sandbox.WebClient
{
    public partial class ClientApp : System.Web.UI.Page
    {
        private WebServerClient _webServerClient;

        protected void Page_Load(object sender, EventArgs e)
        {
            var authorizationServer = new AuthorizationServerDescription
            {
                AuthorizationEndpoint = new Uri("http://localhost:18001/Katana.Sandbox.WebServer/Authorize"),
                TokenEndpoint = new Uri("http://localhost:18001/Katana.Sandbox.WebServer/Token")
            };
            _webServerClient = new WebServerClient(authorizationServer, "123456", "abcdef");

            if (string.IsNullOrEmpty(AccessToken.Text))
            {
                var authorizationState = _webServerClient.ProcessUserAuthorization(new HttpRequestWrapper(Request));
                if (authorizationState != null)
                {
                    AccessToken.Text = authorizationState.AccessToken;
                    RefreshToken.Text = authorizationState.RefreshToken;
                    Page.Form.Action = Request.Path;
                }
            }
        }

        protected void AuthorizeButton_Click(object sender, EventArgs e)
        {
            var userAuthorization = _webServerClient.PrepareRequestUserAuthorization(new[] { "bio", "notes" });
            userAuthorization.Send(Context);
            Response.End();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            HttpClient client;
            if (string.IsNullOrEmpty(AccessToken.Text))
            {
                client = new HttpClient();
            }
            else
            {
                client = new HttpClient(_webServerClient.CreateAuthorizingHandler(AccessToken.Text));
            }
            var response = client.GetAsync("http://localhost:18001/Katana.Sandbox.WebServer/api/me").Result;
            Label1.Text = response.Content.ReadAsStringAsync().Result;
        }

        protected void ResourceOwnerGrantButton_Click(object sender, EventArgs e)
        {
            var authorizationState = _webServerClient.ExchangeUserCredentialForToken(Username.Text, Password.Text, new[] { "bio", "notes" });
            if (authorizationState != null)
            {
                AccessToken.Text = authorizationState.AccessToken;
                RefreshToken.Text = authorizationState.RefreshToken;
            }
        }

        protected void RefreshButton_Click(object sender, EventArgs e)
        {
            var state = new AuthorizationState
            {
                AccessToken = AccessToken.Text,
                RefreshToken = RefreshToken.Text
            };
            if (_webServerClient.RefreshAuthorization(state))
            {
                AccessToken.Text = state.AccessToken;
                RefreshToken.Text = state.RefreshToken;
            }
        }
    }
}