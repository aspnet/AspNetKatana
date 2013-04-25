using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Owin.Security.Infrastructure
{
    public static class WebUtils
    {
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
            var hasQuery = uri.IndexOf('?') != -1;
            return uri + (hasQuery ? "&" : "?") + queryString;
        }

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
            var hasQuery = uri.IndexOf('?') != -1;
            return uri + (hasQuery ? "&" : "?") + Uri.EscapeDataString(name) + "=" + Uri.EscapeDataString(value);
        }

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
            var hasQuery = uri.IndexOf('?') != -1;
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
