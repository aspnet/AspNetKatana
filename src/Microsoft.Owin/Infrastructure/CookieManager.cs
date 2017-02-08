// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Infrastructure
{
    /// <summary>
    /// An implementation of ICookieManager that writes directly to IOwinContext.Response.Cookies.
    /// </summary>
    public class CookieManager : ICookieManager
    {
        /// <summary>
        /// Read a cookie with the given name from the request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetRequestCookie(IOwinContext context, string key)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var requestCookies = context.Request.Cookies;
            var value = requestCookies[key];
            return value;
        }

        /// <summary>
        /// Appends a new response cookie to the Set-Cookie header.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public void AppendResponseCookie(IOwinContext context, string key, string value, CookieOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            context.Response.Cookies.Append(key, value, options);
        }

        /// <summary>
        /// Deletes the cookie with the given key by appending an expired cookie.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="options"></param>
        public void DeleteCookie(IOwinContext context, string key, CookieOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            context.Response.Cookies.Delete(key, options);
        }
    }
}
