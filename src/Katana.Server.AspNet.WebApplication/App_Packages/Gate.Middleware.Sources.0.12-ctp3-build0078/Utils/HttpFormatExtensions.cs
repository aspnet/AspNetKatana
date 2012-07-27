using System;
using System.Web;

namespace Gate.Middleware.Utils
{
    internal static class HttpFormatExtensions
    {
        public static string ToHttpDateString(this DateTime date)
        {
            // Sun, 06 Nov 1994 08:49:37 GMT
            return date.ToUniversalTime().ToString("R");
        }
    }
}