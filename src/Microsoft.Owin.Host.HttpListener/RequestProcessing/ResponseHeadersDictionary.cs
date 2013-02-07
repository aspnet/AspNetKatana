// <copyright file="ResponseHeadersDictionary.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Globalization;
using System.Net;

namespace Microsoft.Owin.Host.HttpListener.RequestProcessing
{
    // This class exposes the response headers collection as a mutable dictionary, and re-maps restricted headers
    // to their associated properties.
    internal sealed class ResponseHeadersDictionary : HeadersDictionaryBase
    {
        private readonly HttpListenerResponse _response;

        internal ResponseHeadersDictionary(HttpListenerResponse response)
            : base()
        {
            _response = response;
            Headers = _response.Headers;
        }

        public override bool TryGetValue(string header, out string[] value)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            if (header.Equals(Constants.ContentLengthHeader, StringComparison.OrdinalIgnoreCase))
            {
                if (_response.ContentLength64 != 0)
                {
                    value = new[] { _response.ContentLength64.ToString(CultureInfo.InvariantCulture) };
                    return true;
                }
            }
            return base.TryGetValue(header, out value);
        }

        protected override string[] Get(string header)
        {
            if (header.Equals(Constants.ContentLengthHeader, StringComparison.OrdinalIgnoreCase))
            {
                if (_response.ContentLength64 != 0)
                {
                    return new[] { _response.ContentLength64.ToString(CultureInfo.InvariantCulture) };
                }
            }
            return base.Get(header);
        }

        public override void Add(string header, string value)
        {
            // Some header values are restricted
            if (header.Equals(Constants.ContentLengthHeader, StringComparison.OrdinalIgnoreCase))
            {
                _response.ContentLength64 = long.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (header.Equals(Constants.TransferEncodingHeader, StringComparison.OrdinalIgnoreCase)
                && value.Equals("chunked", StringComparison.OrdinalIgnoreCase))
            {
                // TODO: what about a mixed format value like chunked, otherTransferEncoding?
                _response.SendChunked = true;
            }
            else if (header.Equals(Constants.ConnectionHeader, StringComparison.OrdinalIgnoreCase)
                && value.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                _response.KeepAlive = false;
            }
            else if (header.Equals(Constants.KeepAliveHeader, StringComparison.OrdinalIgnoreCase)
                && value.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                // HTTP/1.0 semantics
                _response.KeepAlive = true;
            }
            else if (header.Equals(Constants.WwwAuthenticateHeader, StringComparison.OrdinalIgnoreCase))
            {
                // WWW-Authenticate is restricted and must use Response.AddHeader with a single 
                // merged value.  See CopyResponseHeaders.
                if (ContainsKey(Constants.WwwAuthenticateHeader))
                {
                    string[] wwwAuthValues = Get(Constants.WwwAuthenticateHeader);
                    var newHeader = new string[wwwAuthValues.Length + 1];
                    wwwAuthValues.CopyTo(newHeader, 0);
                    newHeader[newHeader.Length - 1] = value;

                    // Uses InternalAdd to bypass a response header restriction, but to do so we must merge the values.
                    _response.AddHeader(Constants.WwwAuthenticateHeader, string.Join(", ", newHeader));
                }
                else
                {
                    // Uses InternalAdd to bypass a response header restriction, but to do so we must merge the values.
                    _response.AddHeader(Constants.WwwAuthenticateHeader, value);
                }
            }
            else
            {
                base.Add(header, value);
            }
        }

        public override bool Remove(string header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            if (!ContainsKey(header))
            {
                return false;
            }

            // Some header values are restricted
            if (header.Equals(Constants.ContentLengthHeader, StringComparison.OrdinalIgnoreCase))
            {
                _response.ContentLength64 = 0;
            }
            else if (header.Equals(Constants.TransferEncodingHeader, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: what about a mixed format value like chunked, otherTransferEncoding?
                _response.SendChunked = false;
            }
            else if (header.Equals(Constants.ConnectionHeader, StringComparison.OrdinalIgnoreCase))
            {
                _response.KeepAlive = true;
            }
            else if (header.Equals(Constants.KeepAliveHeader, StringComparison.OrdinalIgnoreCase))
            {
                // HTTP/1.0 semantics
                _response.KeepAlive = false;
            }
            else
            {
                return base.Remove(header);
            }
            return true;
        }
    }
}
