// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security.OAuth;

namespace Owin
{
    public static class OAuthAuthorizationServerExtensions
    {
        public static IAppBuilder UseOAuthAuthorizationServer(this IAppBuilder app, OAuthAuthorizationServerOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            app.Use(typeof(OAuthAuthorizationServerMiddleware), app, options);
            return app;
        }
    }
}
