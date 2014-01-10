// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;

namespace System.Web
{
    /// <summary>
    /// Provides extension methods for <see cref="HttpContextBase"/>.
    /// </summary>
    public static partial class HttpContextBaseExtensions
    {
        private static IDictionary<string, object> GetOwinEnvironment(this HttpContextBase context)
        {
            return (IDictionary<string, object>)context.Items[HttpContextItemKeys.OwinEnvironmentKey];
        }

        /// <summary>
        /// Gets the <see cref="IOwinContext"/> for the current request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IOwinContext GetOwinContext(this HttpContextBase context)
        {
            IDictionary<string, object> environment = GetOwinEnvironment(context);

            if (environment == null)
            {
                throw new InvalidOperationException(
                    Microsoft.Owin.Host.SystemWeb.Resources.HttpContext_OwinEnvironmentNotFound);
            }

            return new OwinContext(environment);
        }

        /// <summary>
        /// Gets the <see cref="IOwinContext"/> for the current request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IOwinContext GetOwinContext(this HttpRequestBase request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            return request.RequestContext.HttpContext.GetOwinContext();
        }
    }
}
