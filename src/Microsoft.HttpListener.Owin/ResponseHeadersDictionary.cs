//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Net;

namespace Microsoft.HttpListener.Owin
{
    // This class exposes the response headers collection as a mutable dictionary, and re-maps restricted headers
    // to their associated properties.
    internal class ResponseHeadersDictionary : HeadersDictionaryBase
    {
        private HttpListenerResponse response;
        private WebHeaderCollection responseHeaders;

        internal ResponseHeadersDictionary(HttpListenerResponse response)
            : base(response.Headers)
        {
            this.response = response;
            this.responseHeaders = response.Headers;
        }

        public override void Add(string header, string value)
        {
            // Some header values are restricted
            if (header.Equals(Constants.ContentLengthHeader, StringComparison.OrdinalIgnoreCase))
            {
                this.response.ContentLength64 = long.Parse(value);
            }
            else if (header.Equals(Constants.TransferEncodingHeader, StringComparison.OrdinalIgnoreCase)
                && value.Equals("chunked", StringComparison.OrdinalIgnoreCase))
            {
                // TODO: what about a mixed format value like chunked, otherTransferEncoding?
                this.response.SendChunked = true;
            }
            else if (header.Equals(Constants.ConnectionHeader, StringComparison.OrdinalIgnoreCase)
                && value.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                this.response.KeepAlive = false;
            }
            else if (header.Equals(Constants.KeepAliveHeader, StringComparison.OrdinalIgnoreCase)
                && value.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                // HTTP/1.0 semantics
                this.response.KeepAlive = true;
            }
            else if (header.Equals(Constants.WwwAuthenticateHeader, StringComparison.OrdinalIgnoreCase))
            {
                // WWW-Authenticate is restricted and must use Response.AddHeader with a single 
                // merged value.  See CopyResponseHeaders.
                if (this.ContainsKey(Constants.WwwAuthenticateHeader))
                {
                    string[] wwwAuthValues = Get(Constants.WwwAuthenticateHeader);
                    string[] newHeader = new string[wwwAuthValues.Length + 1];
                    wwwAuthValues.CopyTo(newHeader, 0);
                    newHeader[newHeader.Length - 1] = value;

                    // Uses InternalAdd to bypass a response header restriction, but to do so we must merge the values.
                    this.response.AddHeader(Constants.WwwAuthenticateHeader, string.Join(", ", newHeader));
                }
                else
                {
                    // Uses InternalAdd to bypass a response header restriction, but to do so we must merge the values.
                    this.response.AddHeader(Constants.WwwAuthenticateHeader, value);
                }
            }
            else
            {
                base.Add(header, value);
            }
        }

        public override bool Remove(string header)
        {
            // TODO:
            return base.Remove(header);
        }
    }
}
