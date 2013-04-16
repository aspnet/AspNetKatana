// <copyright file="ClientApp.cs" company="Microsoft Open Technologies, Inc.">
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
                AuthorizationEndpoint = new Uri("http://localhost:18421/Authorize"),
                TokenEndpoint = new Uri("http://localhost:18421/Token")
            };
            _webServerClient = new WebServerClient(authorizationServer, "123456", "abcdef");
            
            if (string.IsNullOrEmpty(AccessToken.Text))
            {
                var authorizationState = _webServerClient.ProcessUserAuthorization(new HttpRequestWrapper(Request));
                if (authorizationState == null)
                {
                    var userAuthorization = _webServerClient.PrepareRequestUserAuthorization(new[] { "bio", "notes" });
                    userAuthorization.Send(Context);
                    Response.End();
                }
                else
                {
                    AccessToken.Text = authorizationState.AccessToken;
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            var client = new HttpClient(_webServerClient.CreateAuthorizingHandler(AccessToken.Text));
            var response = client.GetAsync("http://localhost:18421/api/me").Result;
            Label1.Text = response.Content.ReadAsStringAsync().Result;
        }
    }
}