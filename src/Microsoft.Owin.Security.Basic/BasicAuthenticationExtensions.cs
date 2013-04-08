// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security.Basic;

namespace Owin
{
    /// <summary></summary>
    public static class BasicAuthenticationExtensions
    {
        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="provider"></param>
        public static void UseBasicAuthentication(this IAppBuilder appBuilder, IBasicAuthenticationProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            BasicAuthenticationOptions options = new BasicAuthenticationOptions
            {
                Provider = provider
            };

            UseBasicAuthentication(appBuilder, options);
        }

        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="provider"></param>
        /// <param name="realm"></param>
        public static void UseBasicAuthentication(this IAppBuilder appBuilder, IBasicAuthenticationProvider provider,
            string realm)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            BasicAuthenticationOptions options = new BasicAuthenticationOptions
            {
                Provider = provider,
                Realm = realm
            };

            UseBasicAuthentication(appBuilder, options);
        }

        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="options"></param>
        public static void UseBasicAuthentication(this IAppBuilder appBuilder, BasicAuthenticationOptions options)
        {
            if (appBuilder == null)
            {
                throw new ArgumentNullException("appBuilder");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            appBuilder.Use(typeof(BasicAuthenticationMiddleware), options);
            appBuilder.MarkStage("Authenticate");
        }
    }
}
