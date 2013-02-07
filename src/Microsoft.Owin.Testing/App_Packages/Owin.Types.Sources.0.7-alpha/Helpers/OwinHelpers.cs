using System;
using System.Collections.Generic;
using System.Linq;

namespace Owin.Types.Helpers
{
#region OwinHelpers.Forwarded

    internal static partial class OwinHelpers
    {
        public static string GetForwardedScheme(OwinRequest request)
        {
            var headers = request.Headers;

            var forwardedSsl = GetHeader(headers, "X-Forwarded-Ssl");
            if (forwardedSsl != null && string.Equals(forwardedSsl, "on", StringComparison.OrdinalIgnoreCase))
            {
                return "https";
            }

            var forwardedScheme = GetHeader(headers, "X-Forwarded-Scheme");
            if (!string.IsNullOrWhiteSpace(forwardedScheme))
            {
                return forwardedScheme;
            }

            var forwardedProto = GetHeaderSplit(headers, "X-Forwarded-Proto");
            if (forwardedProto != null)
            {
                forwardedScheme = forwardedProto.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(forwardedScheme))
                {
                    return forwardedScheme;
                }
            }

            return request.Scheme;
        }

        public static string GetForwardedHost(OwinRequest request)
        {
            var headers = request.Headers;

            var forwardedHost = GetHeaderSplit(headers, "X-Forwarded-Host");
            if (forwardedHost != null)
            {
                return forwardedHost.Last();
            }

            var host = GetHeader(headers, "Host");
            if (!string.IsNullOrWhiteSpace(host))
            {
                return host;
            }

            var localIpAddress = request.LocalIpAddress ?? "localhost";
            var localPort = request.LocalPort;
            return string.IsNullOrWhiteSpace(localPort) ? localIpAddress : (localIpAddress + ":" + localPort);
        }

        public static Uri GetForwardedUri(OwinRequest request)
        {
            var queryString = request.QueryString;

            return string.IsNullOrWhiteSpace(queryString) 
                ? new Uri(GetForwardedScheme(request) + "://" + GetForwardedHost(request) + request.PathBase + request.Path) 
                : new Uri(GetForwardedScheme(request) + "://" + GetForwardedHost(request) + request.PathBase + request.Path + "?" + queryString);
        }

        public static OwinRequest ApplyForwardedScheme(OwinRequest request)
        {
            request.Scheme = GetForwardedScheme(request);
            return request;
        }

        public static OwinRequest ApplyForwardedHost(OwinRequest request)
        {
            request.Host = GetForwardedHost(request);
            return request;
        }

        public static OwinRequest ApplyForwardedUri(OwinRequest request)
        {
            return ApplyForwardedHost(ApplyForwardedScheme(request));
        }

    }
#endregion

#region OwinHelpers.Header

    internal static partial class OwinHelpers
    {
        public static string GetHeader(IDictionary<string, string[]> headers, string key)
        {
            string[] values = GetHeaderUnmodified(headers, key);
            return values == null ? null : string.Join(",", values);
        }

        public static IEnumerable<string> GetHeaderSplit(IDictionary<string, string[]> headers, string key)
        {
            string[] values = GetHeaderUnmodified(headers,key);
            return values == null ? null : values.SelectMany(SplitHeader);
        }

        public static string[] GetHeaderUnmodified(IDictionary<string, string[]> headers, string key)
        {
            string[] values;
            return headers.TryGetValue(key, out values) ? values : null;
        }

        private static readonly Func<string, string[]> SplitHeader = header => header.Split(new[] { ',' });

        public static void SetHeader(IDictionary<string, string[]> headers, string key, string value)
        {
            headers[key] = new[] { value };
        }

        public static void SetHeaderJoined(IDictionary<string, string[]> headers, string key, params string[] values)
        {
            headers[key] = new[] { string.Join(",", values) };
        }

        public static void SetHeaderJoined(IDictionary<string, string[]> headers, string key, IEnumerable<string> values)
        {
            SetHeaderJoined(headers, key, values.ToArray());
        }

        public static void SetHeaderUnmodified(IDictionary<string, string[]> headers, string key, params string[] values)
        {
            headers[key] = values;
        }

        public static void SetHeaderUnmodified(IDictionary<string, string[]> headers, string key, IEnumerable<string> values)
        {
            headers[key] = values.ToArray();
        }

        public static void AddHeader(IDictionary<string, string[]> headers, string key, string value)
        {
            AddHeaderUnmodified(headers, key, value);
        }

        public static void AddHeaderJoined(IDictionary<string, string[]> headers, string key, params string[] values)
        {
            var existing = GetHeaderUnmodified(headers, key);
            if (existing == null)
            {
                SetHeaderJoined(headers, key, values);
            }
            else
            {
                SetHeaderJoined(headers, key, existing.Concat(values));
            }
        }

        public static void AddHeaderJoined(IDictionary<string, string[]> headers, string key, IEnumerable<string> values)
        {
            var existing = GetHeaderUnmodified(headers, key);
            SetHeaderJoined(headers, key, existing == null ? values : existing.Concat(values));
        }

        public static void AddHeaderUnmodified(IDictionary<string, string[]> headers, string key, params string[] values)
        {
            var existing = GetHeaderUnmodified(headers, key);
            if (existing == null)
            {
                SetHeaderUnmodified(headers, key, values);
            }
            else
            {
                SetHeaderUnmodified(headers, key, existing.Concat(values));
            }
        }

        public static void AddHeaderUnmodified(IDictionary<string, string[]> headers, string key, IEnumerable<string> values)
        {
            var existing = GetHeaderUnmodified(headers, key);
            SetHeaderUnmodified(headers, key, existing == null ? values : existing.Concat(values));
        }
    }
#endregion

#region OwinHelpers.MethodOverride

    internal static partial class OwinHelpers
    {
        public static string GetMethodOverride(OwinRequest request)
        {
            var method = request.Method;
            if (!string.Equals("POST", method, StringComparison.OrdinalIgnoreCase))
            {
                // override has no effect on POST 
                return method;
            }

            var methodOverride = GetHeader(request.Headers, "X-Http-Method-Override");
            if (string.IsNullOrEmpty(methodOverride))
            {
                return method;
            }

            return methodOverride;
        }

        public static OwinRequest ApplyMethodOverride(OwinRequest request)
        {
            request.Method = GetMethodOverride(request);
            return request;
        }
    }
#endregion

#region OwinHelpers.Uri

    internal static partial class OwinHelpers
    {
        public static string GetHost(OwinRequest request)
        {
            var headers = request.Headers;

            var host = GetHeader(headers, "Host");
            if (!string.IsNullOrWhiteSpace(host))
            {
                return host;
            }

            var localIpAddress = request.LocalIpAddress ?? "localhost";
            var localPort = request.LocalPort;
            return string.IsNullOrWhiteSpace(localPort) ? localIpAddress : (localIpAddress + ":" + localPort);
        }

        public static Uri GetUri(OwinRequest request)
        {
            var queryString = request.QueryString;

            return string.IsNullOrWhiteSpace(queryString)
                ? new Uri(request.Scheme + "://" + GetHost(request) + request.PathBase + request.Path)
                : new Uri(request.Scheme + "://" + GetHost(request) + request.PathBase + request.Path + "?" + queryString);
        }
    }
#endregion

}
