// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security.OAuth;

namespace Owin
{
    public static class OAuthBearerAuthenticationExtensions
    {
        public static IAppBuilder UseOAuthBearerAuthentication(this IAppBuilder app, OAuthBearerAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            app.Use(typeof(OAuthBearerAuthenticationMiddleware), app, options);
            app.UseStageMarker(PipelineStage.Authenticate);
            return app;
        }
    }
}
