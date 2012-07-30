//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Owin;

    /// <summary>
    /// This wraps an HttpListenerResponse, populates it with the given response fields, and relays 
    /// the response body to the underlying stream.
    /// </summary>
    internal class OwinHttpListenerResponse
    {
        private HttpListenerResponse response;
        private Func<Stream, Task> bodyDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerResponse"/> class.
        /// Copies the status and headers into the response object.
        /// </summary>
        /// <param name="response">The response to copy the OWIN data into.</param>
        /// <param name="result">The status, headers, body, and properties.</param>
        public OwinHttpListenerResponse(HttpListenerResponse response, ResultParameters result)
        {
            Contract.Requires(response != null);
            Contract.Requires(result.Properties != null);
            this.response = response;
            this.bodyDelegate = result.Body;

            if (result.Status == 100)
            {
                throw new ArgumentOutOfRangeException("result.Status", result.Status, string.Empty);
            }

            // Status
            this.response.StatusCode = result.Status;
            
            // Optional reason phrase
            object reasonPhrase;
            if (result.Properties != null 
                && result.Properties.TryGetValue(Constants.ReasonPhraseKey, out reasonPhrase)
                && !string.IsNullOrWhiteSpace((string)reasonPhrase))
            {
                this.response.StatusDescription = (string)reasonPhrase;
            }

            // Version, e.g. HTTP/1.1
            object httpVersion;
            if (result.Properties != null
                && result.Properties.TryGetValue(Constants.HttpResponseProtocolKey, out httpVersion)
                && !string.IsNullOrWhiteSpace((string)httpVersion))
            {
                string httpVersionString = (string)httpVersion;
                Contract.Requires(httpVersionString.StartsWith("HTTP/"));
                Version version = Version.Parse(httpVersionString.Substring(httpVersionString.IndexOf('/') + 1));
                this.response.ProtocolVersion = version;
            }

            // Headers
            if (result.Headers != null && result.Headers.Count > 0)
            {
                this.CopyResponseHeaders(result.Headers);
            }
        }

        private void CopyResponseHeaders(IDictionary<string, string[]> responseHeaders)
        {
            foreach (KeyValuePair<string, string[]> header in responseHeaders)
            {
                foreach (string value in header.Value)
                {
                    this.AddHeaderValue(header.Key, value);
                }
            }

            string[] wwwAuthValues;
            if (responseHeaders.TryGetValue(Constants.WwwAuthenticateHeader, out wwwAuthValues))
            {
                // Uses InternalAdd to bypass a response header restriction, but to do so we must merge the values.
                this.response.AddHeader(Constants.WwwAuthenticateHeader, string.Join(", ", wwwAuthValues));
            }
        }

        private void AddHeaderValue(string header, string value)
        {
            try
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
                    // WWW-Autheticate is restricted and must use Response.AddHeader with a single 
                    // merged value.  See CopyResponseHeaders.
                }
                else
                {
                    this.response.Headers.Add(header, value);
                }
            }
            catch (Exception)
            {
                // TODO: Logging; Debug.Assert(false, "Bad response header: " + header);
                throw;
            }
        }

        // The caller will handle errors and abort the request.
        public async Task ProcessBodyAsync()
        {
            if (this.bodyDelegate == null)
            {
                this.response.Close();
            }
            else
            {
                Stream responseOutput = new HttpListenerStreamWrapper(this.response.OutputStream);
                await this.bodyDelegate(responseOutput);
                this.response.Close();
            }
        }
    }
}
