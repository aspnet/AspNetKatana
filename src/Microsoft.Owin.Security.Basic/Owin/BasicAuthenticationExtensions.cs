// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Security;
using Microsoft.Owin.Security;

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
            UseBasicAuthentication(appBuilder, provider, null);
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

            UseBasicAuthentication(appBuilder, new BasicAuthenticationProtocol(provider, realm));
        }

        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="protocol"></param>
        public static void UseBasicAuthentication(this IAppBuilder appBuilder, IBasicAuthenticationProtocol protocol)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            BasicAuthenticationOptions options = new BasicAuthenticationOptions
            {
                Protocol = protocol
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
