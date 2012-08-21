
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.WebApi.Owin.CallHeaders
{
	partial class MessageHeadersWrapper
	{
        private static bool IsContentHeader(string header)
        {
            switch (header.Length)
            {
                case 5:
                    return "Allow".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 19:
                    return "Content-Disposition".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 16:
                    return "Content-Encoding".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Content-Language".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Content-Location".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 14:
                    return "Content-Length".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 11:
                    return "Content-MD5".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 13:
                    return "Content-Range".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Last-Modified".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 12:
                    return "Content-Type".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 7:
                    return "Expires".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
            }
            return false;
        }

        private static bool IsMessageHeader(string header)
        {
            switch (header.Length)
            {
                case 6:
                    return "Accept".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Expect".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Server".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Pragma".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 14:
                    return "Accept-Charset".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 15:
                    return "Accept-Encoding".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Accept-Language".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 13:
                    return "Authorization".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "If-None-Match".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Accept-Ranges".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Cache-Control".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 4:
                    return "From".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Host".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "ETag".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Vary".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Date".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 8:
                    return "If-Match".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "If-Range".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Location".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 17:
                    return "If-Modified-Since".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Transfer-Encoding".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 19:
                    return "If-Unmodified-Since".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Proxy-Authorization".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 12:
                    return "Max-Forwards".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 5:
                    return "Range".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 7:
                    return "Referer".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Trailer".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Upgrade".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Warning".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 2:
                    return "TE".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 10:
                    return "User-Agent".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Connection".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 3:
                    return "Age".Equals(header, StringComparison.OrdinalIgnoreCase)
                        || "Via".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 18:
                    return "Proxy-Authenticate".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 11:
                    return "Retry-After".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
                case 16:
                    return "WWW-Authenticate".Equals(header, StringComparison.OrdinalIgnoreCase)
                    ;
            }
            return false;
        }	}
}
