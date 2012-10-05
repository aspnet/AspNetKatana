//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Katana.Sample.Mvc4.WebApplication.Models;
using Microsoft.Web.WebPages.OAuth;

namespace Katana.Sample.Mvc4.WebApplication
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            // OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            // OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            // OAuthWebSecurity.RegisterFacebookClient(
            //    appId: "",
            //    appSecret: "");

            // OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
