// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Web.Cors;

namespace Microsoft.Owin.Cors
{
    /// <summary>
    /// Contains the options used by the CorsMiddleware
    /// </summary>
    public class CorsOptions
    {
        private static CorsPolicy _allowAll;

        /// <summary>
        /// A policy that allows all headers, all methods, any origin and supports credentials
        /// </summary>
        public static CorsPolicy AllowAll
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _allowAll, () =>
                {
                    return new CorsPolicy
                    {
                        AllowAnyHeader = true,
                        AllowAnyMethod = true,
                        AllowAnyOrigin = true,
                        SupportsCredentials = true
                    };
                });
            }
        }

        /// <summary>
        /// The cors policy to apply
        /// </summary>
        public CorsPolicy CorsPolicy { get; set; }

        /// <summary>
        /// The cors engine
        /// </summary>
        public ICorsEngine CorsEngine { get; set; }
    }
}
