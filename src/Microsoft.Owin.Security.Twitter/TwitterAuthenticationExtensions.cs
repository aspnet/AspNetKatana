// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Twitter;

namespace Owin
{
    /// <summary>
    /// Extension methods for using <see cref="TwitterAuthenticationMiddleware"/>
    /// </summary>
    public static class TwitterAuthenticationExtensions
    {
        /// <summary>
        /// Authenticate users using Twitter
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="options">Middleware configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseTwitterAuthentication(this IAppBuilder app, TwitterAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            app.Use(typeof(TwitterAuthenticationMiddleware), app, options);
            return app;
        }

        /// <summary>
        /// Authenticate users using Twitter
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="consumerKey">The Twitter-issued consumer key</param>
        /// <param name="consumerSecret">The Twitter-issued consumer secret</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseTwitterAuthentication(
            this IAppBuilder app,
            string consumerKey,
            string consumerSecret)
        {
            return UseTwitterAuthentication(
                app,
                new TwitterAuthenticationOptions
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                });
        }
    }
}
