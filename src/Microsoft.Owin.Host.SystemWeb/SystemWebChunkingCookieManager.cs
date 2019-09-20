// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Web;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    /// <summary>
    /// This handles cookies that are limited by per cookie length. It breaks down long cookies for responses, and reassembles them
    /// from requests. The cookies are stored in the System.Web object model rather than directly in the headers.
    /// </summary>
    public class SystemWebChunkingCookieManager : ICookieManager
    {
        /// <summary>
        /// This handles cookies that are limited by per cookie length. It breaks down long cookies for responses, and reassembles them
        /// from requests. The cookies are stored in the System.Web object model rather than directly in the headers.
        /// </summary>
        public SystemWebChunkingCookieManager()
        {
            ChunkSize = 4090;
            ThrowForPartialCookies = true;
            Fallback = new ChunkingCookieManager();
        }

        /// <summary>
        /// A fallback manager used if HttpContextBase can't be located.
        /// </summary>
        public ICookieManager Fallback { get; set; }

        /// <summary>
        /// The maximum size of cookie to send back to the client. If a cookie exceeds this size it will be broken down into multiple
        /// cookies. Set this value to null to disable this behavior. The default is 4090 characters, which is supported by all
        /// common browsers.
        ///
        /// Note that browsers may also have limits on the total size of all cookies per domain, and on the number of cookies per domain.
        /// </summary>
        public int? ChunkSize { get; set; }

        /// <summary>
        /// Throw if not all chunks of a cookie are available on a request for re-assembly.
        /// </summary>
        public bool ThrowForPartialCookies { get; set; }

        // Parse the "chunks:XX" to determine how many chunks there should be.
        private static int ParseChunksCount(string value)
        {
            if (value != null && value.StartsWith("chunks:", StringComparison.Ordinal))
            {
                string chunksCountString = value.Substring("chunks:".Length);
                int chunksCount;
                if (int.TryParse(chunksCountString, NumberStyles.None, CultureInfo.InvariantCulture, out chunksCount))
                {
                    return chunksCount;
                }
            }
            return 0;
        }

        /// <summary>
        /// Get the reassembled cookie. Non chunked cookies are returned normally.
        /// Cookies with missing chunks just have their "chunks:XX" header returned.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns>The reassembled cookie, if any, or null.</returns>
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

            var requestCookies = webContext.Request.Cookies;
            var escapedKey = Uri.EscapeDataString(key);
            var cookie = requestCookies[escapedKey];
            if (cookie == null)
            {
                return null;
            }

            var value = cookie.Value;
            int chunksCount = ParseChunksCount(value);
            if (chunksCount > 0)
            {
                bool quoted = false;
                string[] chunks = new string[chunksCount];
                for (int chunkId = 1; chunkId <= chunksCount; chunkId++)
                {
                    cookie = requestCookies[escapedKey + "C" + chunkId.ToString(CultureInfo.InvariantCulture)];
                    if (cookie == null)
                    {
                        if (ThrowForPartialCookies)
                        {
                            int totalSize = 0;
                            for (int i = 0; i < chunkId - 1; i++)
                            {
                                totalSize += chunks[i].Length;
                            }
                            throw new FormatException(
                                string.Format(CultureInfo.CurrentCulture, Resources.Exception_ImcompleteChunkedCookie, chunkId - 1, chunksCount, totalSize));
                        }
                        // Missing chunk, abort by returning the original cookie value. It may have been a false positive?
                        return Uri.UnescapeDataString(value);
                    }
                    string chunk = cookie.Value;
                    if (IsQuoted(chunk))
                    {
                        // Note: Since we assume these cookies were generated by our code, then we can assume that if one cookie has quotes then they all do.
                        quoted = true;
                        chunk = RemoveQuotes(chunk);
                    }
                    chunks[chunkId - 1] = chunk;
                }
                string merged = string.Join(string.Empty, chunks);
                if (quoted)
                {
                    merged = Quote(merged);
                }
                return Uri.UnescapeDataString(merged);
            }
            return Uri.UnescapeDataString(value);
        }

        /// <summary>
        /// Appends a new response cookie to the Set-Cookie header. If the cookie is larger than the given size limit
        /// then it will be broken down into multiple cookies as follows:
        /// Set-Cookie: CookieName=chunks:3; path=/
        /// Set-Cookie: CookieNameC1=Segment1; path=/
        /// Set-Cookie: CookieNameC2=Segment2; path=/
        /// Set-Cookie: CookieNameC3=Segment3; path=/
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
            bool sameSiteHasValue = options.SameSite.HasValue && SystemWebCookieManager.IsSameSiteAvailable;

            string escapedKey = Uri.EscapeDataString(key);
            string prefix = escapedKey + "=";

            string suffix = string.Concat(
                !domainHasValue ? null : "; domain=",
                !domainHasValue ? null : options.Domain,
                !pathHasValue ? null : "; path=",
                !pathHasValue ? null : options.Path,
                !expiresHasValue ? null : "; expires=",
                !expiresHasValue ? null : options.Expires.Value.ToString("ddd, dd-MMM-yyyy HH:mm:ss \\G\\M\\T", CultureInfo.InvariantCulture),
                !options.Secure ? null : "; secure",
                !options.HttpOnly ? null : "; HttpOnly",
                !sameSiteHasValue ? null : "; SameSite=",
                !sameSiteHasValue ? null : GetStringRepresentationOfSameSite(options.SameSite.Value)
                );

            value = value ?? string.Empty;
            bool quoted = false;
            if (IsQuoted(value))
            {
                quoted = true;
                value = RemoveQuotes(value);
            }
            string escapedValue = Uri.EscapeDataString(value);

            // Normal cookie
            if (!ChunkSize.HasValue || ChunkSize.Value > prefix.Length + escapedValue.Length + suffix.Length + (quoted ? 2 : 0))
            {
                var cookie = new HttpCookie(escapedKey, escapedValue);
                SetOptions(cookie, options, domainHasValue, pathHasValue, expiresHasValue);

                webContext.Response.AppendCookie(cookie);
            }
            else if (ChunkSize.Value < prefix.Length + suffix.Length + (quoted ? 2 : 0) + 10)
            {
                // 10 is the minimum data we want to put in an individual cookie, including the cookie chunk identifier "CXX".
                // No room for data, we can't chunk the options and name
                throw new InvalidOperationException(Resources.Exception_CookieLimitTooSmall);
            }
            else
            {
                // Break the cookie down into multiple cookies.
                // Key = CookieName, value = "Segment1Segment2Segment2"
                // Set-Cookie: CookieName=chunks:3; path=/
                // Set-Cookie: CookieNameC1="Segment1"; path=/
                // Set-Cookie: CookieNameC2="Segment2"; path=/
                // Set-Cookie: CookieNameC3="Segment3"; path=/
                int dataSizePerCookie = ChunkSize.Value - prefix.Length - suffix.Length - (quoted ? 2 : 0) - 3; // Budget 3 chars for the chunkid.
                int cookieChunkCount = (int)Math.Ceiling(escapedValue.Length * 1.0 / dataSizePerCookie);

                var cookie = new HttpCookie(escapedKey, "chunks:" + cookieChunkCount.ToString(CultureInfo.InvariantCulture));
                SetOptions(cookie, options, domainHasValue, pathHasValue, expiresHasValue);

                webContext.Response.AppendCookie(cookie);

                int offset = 0;
                for (int chunkId = 1; chunkId <= cookieChunkCount; chunkId++)
                {
                    int remainingLength = escapedValue.Length - offset;
                    int length = Math.Min(dataSizePerCookie, remainingLength);
                    string segment = escapedValue.Substring(offset, length);
                    offset += length;

                    cookie = new HttpCookie(escapedKey + "C" + chunkId.ToString(CultureInfo.InvariantCulture), quoted ? Quote(segment) : segment);
                    SetOptions(cookie, options, domainHasValue, pathHasValue, expiresHasValue);

                    webContext.Response.AppendCookie(cookie);
                }
            }
        }

        /// <summary>
        /// Deletes the cookie with the given key by setting an expired state. If a matching chunked cookie exists on
        /// the request, delete each chunk.
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

            var webContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
            if (webContext == null)
            {
                Fallback.DeleteCookie(context, key, options);
                return;
            }

            string escapedKey = Uri.EscapeDataString(key);

            var requestCookies = webContext.Request.Cookies;
            var cookie = requestCookies[escapedKey];
            string requestCookie = (cookie == null ? null : cookie.Value);

            int chunks = ParseChunksCount(requestCookie);

            AppendResponseCookie(
                context,
                key,
                string.Empty,
                new CookieOptions
                {
                    Path = options.Path,
                    Domain = options.Domain,
                    HttpOnly = options.HttpOnly,
                    SameSite = options.SameSite,
                    Secure = options.Secure,
                    Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                });

            for (int i = 1; i <= chunks; i++)
            {
                AppendResponseCookie(
                    context,
                    key + "C" + i.ToString(CultureInfo.InvariantCulture),
                    string.Empty,
                    new CookieOptions
                    {
                        Path = options.Path,
                        Domain = options.Domain,
                        HttpOnly = options.HttpOnly,
                        SameSite = options.SameSite,
                        Secure = options.Secure,
                        Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    });
            }
        }

        private static void SetOptions(HttpCookie cookie, CookieOptions options, bool domainHasValue, bool pathHasValue, bool expiresHasValue)
        {
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

            if (SystemWebCookieManager.IsSameSiteAvailable)
            {
                SystemWebCookieManager.SameSiteSetter.Invoke(cookie, new object[]
                {
                    options.SameSite ?? (SameSiteMode)(-1) // Unspecified
                });
            }
        }

        private static bool IsQuoted(string value)
        {
            return value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"';
        }

        private static string RemoveQuotes(string value)
        {
            return value.Substring(1, value.Length - 2);
        }

        private static string Quote(string value)
        {
            return '"' + value + '"';
        }

        private static string GetStringRepresentationOfSameSite(SameSiteMode siteMode)
        {
            switch (siteMode)
            {
                case SameSiteMode.None:
                    return "None";
                case SameSiteMode.Lax:
                    return "Lax";
                case SameSiteMode.Strict:
                    return "Strict";
                default:
                    throw new ArgumentOutOfRangeException("siteMode",
                        string.Format(CultureInfo.InvariantCulture, "Unexpected SameSiteMode value: {0}", siteMode));
            }
        }
    }
}
