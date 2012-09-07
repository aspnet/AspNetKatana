//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace Microsoft.HttpListener.Owin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// This wraps an HttpListenerResponse, populates it with the given response fields, and relays 
    /// the response body to the underlying stream.
    /// </summary>
    internal class OwinHttpListenerResponse
    {
        private IDictionary<string, object> environment;
        private HttpListenerContext context;
        private HttpListenerResponse response;
        private RequestLifetimeMonitor lifetime;
        private bool responseProcessed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerResponse"/> class.
        /// Sets up the Environment with the necessary request state items.
        /// </summary>
        public OwinHttpListenerResponse(HttpListenerContext context, IDictionary<string, object> environment, RequestLifetimeMonitor lifetime)
        {
            Contract.Requires(context != null);
            Contract.Requires(environment != null);
            this.context = context;
            this.response = context.Response;
            this.environment = environment;
            this.lifetime = lifetime;

            HttpListenerStreamWrapper outputStream = new HttpListenerStreamWrapper(this.response.OutputStream);
            outputStream.OnFirstWrite = ResponseBodyStarted;
            this.environment.Add(Constants.ResponseBodyKey, outputStream);

            ResponseHeadersDictionary headers = new ResponseHeadersDictionary(this.response);
            this.environment.Add(Constants.ResponseHeadersKey, headers);
        }
        
        private void ResponseBodyStarted()
        {
            if (lifetime.TryStartResponse())
            {
                ProcessResponse();
            }
            else
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        public void Close()
        {
            if (!responseProcessed)
            {
                this.ProcessResponse();
                this.response.Close();
            }
        }

        // Set the status code and reason phrase from the environment.
        private void ProcessResponse()
        {
            responseProcessed = true;

            object temp;
            if (this.environment.TryGetValue(Constants.ResponseStatusCodeKey, out temp))
            {
                int statusCode = (int)temp;
                if (statusCode == 100 || statusCode < 100 || statusCode >= 1000)
                {
                    throw new ArgumentOutOfRangeException(Constants.ResponseStatusCodeKey, statusCode, string.Empty);
                }

                // Status
                this.response.StatusCode = statusCode;
            }
                        
            // Optional reason phrase
            object reasonPhrase;
            if (this.environment.TryGetValue(Constants.ResponseReasonPhraseKey, out reasonPhrase)
                && !string.IsNullOrWhiteSpace((string)reasonPhrase))
            {
                this.response.StatusDescription = (string)reasonPhrase;
            }

            /* // response.ProtocolVersion is ignored by Http.Sys.  It always sends 1.1
            // Version, e.g. HTTP/1.1
            object httpVersion;
            if (this.environment.TryGetValue(Constants.HttpResponseProtocolKey, out httpVersion)
                && !string.IsNullOrWhiteSpace((string)httpVersion))
            {
                string httpVersionString = (string)httpVersion;
                Contract.Requires(httpVersionString.StartsWith("HTTP/"));
                Version version = Version.Parse(httpVersionString.Substring(httpVersionString.IndexOf('/') + 1));
                this.response.ProtocolVersion = version;
            }
            */
        }
        /*
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
                    // WWW-Authenticate is restricted and must use Response.AddHeader with a single 
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
        }*/
    }
}
