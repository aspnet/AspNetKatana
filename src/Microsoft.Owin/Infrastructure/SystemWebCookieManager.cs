// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Web;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    /// <summary>
    /// An implementation of ICookieManager that uses the System.Web.HttpContextBase object model.
    /// </summary>
    public class SystemWebCookieManager : ICookieManager
    {
        // .NET 4.7.2, but requries a patch to emit SameSite=None
        internal static readonly bool IsSameSiteAvailable;
        internal static readonly MethodInfo SameSiteSetter;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static SystemWebCookieManager()
        {
            var systemWeb = typeof(HttpContextBase).Assembly;
            IsSameSiteAvailable = systemWeb.GetType("System.Web.SameSiteMode") != null;
            if (IsSameSiteAvailable)
            {
                SameSiteSetter = typeof(HttpCookie).GetProperty("SameSite").SetMethod;
            }
        }

        /// <summary>
        /// Creates a new instance of SystemWebCookieManager.
        /// </summary>
        public SystemWebCookieManager()
        {
            Fallback = new CookieManager();
        }

        /// <summary>
        /// A fallback manager used if HttpContextBase can't be located.
        /// </summary>
        public ICookieManager Fallback { get; set; }

        /// <summary>
        /// Reads the requested cookie from System.Web.HttpContextBase.Request.Cookies.
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

            var webContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
            if (webContext == null)
            {
                return Fallback.GetRequestCookie(context, key);
            }

            var escapedKey = Uri.EscapeDataString(key);
            var cookie = webContext.Request.Cookies[escapedKey];
            if (cookie == null)
            {
                return null;
            }
            return Uri.UnescapeDataString(cookie.Value);
        }

        /// <summary>
        /// Appends the requested cookie to System.Web.HttpContextBase.Response.Cookies.
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

            var webContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
            if (webContext == null)
            {
                Fallback.AppendResponseCookie(context, key, value, options);
                return;
            }

            bool domainHasValue = !string.IsNullOrEmpty(options.Domain);
            bool pathHasValue = !string.IsNullOrEmpty(options.Path);
            bool expiresHasValue = options.Expires.HasValue;

            var escapedKey = Uri.EscapeDataString(key);
            var cookie = new HttpCookie(escapedKey, Uri.EscapeDataString(value));
            if (domainHasValue)
            {
                cookie.Domain = options.Domain;
            }
            if (pathHasValue)
            {
                cookie.Path = options.Path;
            }
            if (expiresHasValue)
            {
                cookie.Expires = options.Expires.Value;
            }
            if (options.Secure)
            {
                cookie.Secure = true;
            }
            if (options.HttpOnly)
            {
                cookie.HttpOnly = true;
            }
            if (IsSameSiteAvailable)
            {
                SameSiteSetter.Invoke(cookie, new object[]
                {
                    options.SameSite ?? (SameSiteMode)(-1) // Unspecified
                });
            }

            webContext.Response.AppendCookie(cookie);
        }

        /// <summary>
        /// Deletes the requested cookie by appending an expired cookie to System.Web.HttpContextBase.Response.Cookies.
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

            AppendResponseCookie(
                context,
                key,
                string.Empty,
                new CookieOptions
                {
                    Path = options.Path,
                    Domain = options.Domain,
                    HttpOnly = options.HttpOnly,
                    Secure = options.Secure,
                    SameSite = options.SameSite,
                    Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                });
        }
    }
}
