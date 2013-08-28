// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Microsoft.Owin.Infrastructure
{
    /// <summary>
    /// Response generation utilities.
    /// </summary>
    public static class WebUtilities
    {
        /// <summary>
        /// Append the given query to the uri.
        /// </summary>
        /// <param name="uri">The base uri.</param>
        /// <param name="queryString">The query string to append, if any.</param>
        /// <returns>The combine result.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Disassembled")]
        public static string AddQueryString(string uri, string queryString)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (string.IsNullOrEmpty(queryString))
            {
                return uri;
            }
            bool hasQuery = uri.IndexOf('?') != -1;
            return uri + (hasQuery ? "&" : "?") + queryString;
        }

        /// <summary>
        /// Append the given query key and value to the uri.
        /// </summary>
        /// <param name="uri">The base uri.</param>
        /// <param name="name">The name of the query key.</param>
        /// <param name="value">The query value.</param>
        /// <returns>The combine result.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Disassembled")]
        public static string AddQueryString(string uri, string name, string value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            bool hasQuery = uri.IndexOf('?') != -1;
            return uri + (hasQuery ? "&" : "?") + Uri.EscapeDataString(name) + "=" + Uri.EscapeDataString(value);
        }

        /// <summary>
        /// Append the given query keys and values to the uri.
        /// </summary>
        /// <param name="uri">The base uri.</param>
        /// <param name="queryString">A collection of name value query pairs to append.</param>
        /// <returns>The combine result.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Disassembled")]
        public static string AddQueryString(string uri, IDictionary<string, string> queryString)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (queryString == null)
            {
                throw new ArgumentNullException("queryString");
            }
            var sb = new StringBuilder();
            sb.Append(uri);
            bool hasQuery = uri.IndexOf('?') != -1;
            foreach (var parameter in queryString)
            {
                sb.Append(hasQuery ? '&' : '?');
                sb.Append(Uri.EscapeDataString(parameter.Key));
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(parameter.Value));
                hasQuery = true;
            }
            return sb.ToString();
        }
    }
}
