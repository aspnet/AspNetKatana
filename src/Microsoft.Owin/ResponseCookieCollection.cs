// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Owin.Infrastructure;
using System.Globalization;

namespace Microsoft.Owin
{
    // Write-only, to the Set-Cookie header
    public class ResponseCookieCollection
    {
        public ResponseCookieCollection(IHeaderDictionary headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            Headers = headers;
        }

        private IHeaderDictionary Headers { get; set; }

        public void Append(string key, string value)
        {
            Headers.Append(Constants.Headers.SetCookie, Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value) + "; path=/");
        }

        public void Append(string key, string value, CookieOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var domainHasValue = !string.IsNullOrEmpty(options.Domain);
            var pathHasValue = !string.IsNullOrEmpty(options.Path);
            var expiresHasValue = options.Expires.HasValue;

            var setCookieValue = string.Concat(
                Uri.EscapeDataString(key),
                "=",
                Uri.EscapeDataString(value ?? string.Empty),
                !domainHasValue ? null : "; domain=",
                !domainHasValue ? null : options.Domain,
                !pathHasValue ? null : "; path=",
                !pathHasValue ? null : options.Path,
                !expiresHasValue ? null : "; expires=",
                !expiresHasValue ? null : options.Expires.Value.ToString("ddd, dd-MMM-yyyy HH:mm:ss ", CultureInfo.InvariantCulture) + "GMT",
                !options.Secure ? null : "; secure",
                !options.HttpOnly ? null : "; HttpOnly");
            Headers.Append("Set-Cookie", setCookieValue);
        }

        // Sets expired cookie
        public void Delete(string key)
        {
            Func<string, bool> predicate = value => value.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase);

            var deleteCookies = new[] { Uri.EscapeDataString(key) + "=; expires=Thu, 01-Jan-1970 00:00:00 GMT" };
            var existingValues = Headers.GetValues(Constants.Headers.SetCookie);
            if (existingValues == null || existingValues.Count == 0)
            {
                Headers.SetValues(Constants.Headers.SetCookie, deleteCookies);
            }
            else
            {
                Headers.SetValues(Constants.Headers.SetCookie, existingValues.Where(value => !predicate(value)).Concat(deleteCookies).ToArray());
            }
        }

        public void Delete(string key, CookieOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var domainHasValue = !string.IsNullOrEmpty(options.Domain);
            var pathHasValue = !string.IsNullOrEmpty(options.Path);

            Func<string, bool> rejectPredicate;
            if (domainHasValue)
            {
                rejectPredicate = value =>
                    value.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase) &&
                        value.IndexOf("domain=" + options.Domain, StringComparison.OrdinalIgnoreCase) != -1;
            }
            else if (pathHasValue)
            {
                rejectPredicate = value =>
                    value.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase) &&
                        value.IndexOf("path=" + options.Path, StringComparison.OrdinalIgnoreCase) != -1;
            }
            else
            {
                rejectPredicate = value => value.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase);
            }

            var existingValues = Headers.GetValues(Constants.Headers.SetCookie);
            if (existingValues != null)
            {
                Headers.SetValues(Constants.Headers.SetCookie, existingValues.Where(value => !rejectPredicate(value)).ToArray());
            }

            Append(key, string.Empty, new CookieOptions
            {
                Path = options.Path,
                Domain = options.Domain,
                Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });
        }
    }
}
