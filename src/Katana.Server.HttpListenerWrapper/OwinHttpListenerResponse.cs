namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// This wraps an HttpListenerResponse, populates it with the given response fields, and relays 
    /// the response body to the underlying stream.
    /// </summary>
    internal class OwinHttpListenerResponse
    {
        private HttpListenerResponse response;
        private TaskCompletionSource<object> tcs;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerResponse"/> class.
        /// Copies the status and headers into the response object.
        /// </summary>
        /// <param name="response">The response to copy the OWIN data into.</param>
        /// <param name="responseStatus">The response status code and reason phrase.  e.g. "200 OK"</param>
        /// <param name="responseHeaders">The response headers to copy into the response object.</param>
        public OwinHttpListenerResponse(
            HttpListenerResponse response, 
            string responseStatus, 
            IDictionary<string, string[]> responseHeaders)
        {
            Contract.Requires(response != null);
            this.response = response;
            this.tcs = new TaskCompletionSource<object>();

            // Status
            Contract.Requires(responseStatus.Length >= 3);
            response.StatusCode = int.Parse(responseStatus.Substring(0, 3));
            if (responseStatus.Length > 4)
            {
                response.StatusDescription = responseStatus.Substring(4);
            }

            // Headers
            this.CopyResponseHeaders(responseHeaders);
        }

        public Task Completion 
        { 
            get 
            { 
                return this.tcs.Task; 
            } 
        }

        private void CopyResponseHeaders(IDictionary<string, string[]> responseHeaders)
        {
            foreach (KeyValuePair<string, string[]> header in responseHeaders)
            {
                foreach (string value in header.Value)
                {
                    // Some header values are restricted
                    if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    {
                        this.response.ContentLength64 = long.Parse(value);
                    }
                    else if (header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
                        && value.Equals("chunked", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO, what about a mixed format value like chunked, otherTransferEncoding?
                        this.response.SendChunked = true;
                    }
                    else if (header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase)
                        && value.Equals("close", StringComparison.OrdinalIgnoreCase))
                    {
                        this.response.KeepAlive = false;
                    }
                    else if (header.Key.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase)
                        && value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: If this is an HTTP 1.0 request we and there is a KeepAlive header, we need to set response.KeepAlive = true;
                        this.response.KeepAlive = true;
                    }
                    else if (header.Key.Equals("WWW-Authenticate", StringComparison.OrdinalIgnoreCase))
                    {
                        // Uses InternalAdd to bypass a response header restriction
                        this.response.AddHeader(header.Key, value);
                    }
                    else
                    {
                        try
                        {
                            this.response.Headers.Add(header.Key, value);
                        }
                        catch (ArgumentException)
                        {
                            Debug.Assert(false, "Reserved header: " + header.Key);
                            throw;
                        }
                    }
                }
            }
        }

        // Returns true if the callback will be invoked
        internal bool Write(ArraySegment<byte> data, Action complete)
        {
            try
            {
                if (data.Array == null || data.Count == 0)
                {
                    return this.Flush(complete);
                }

                if (complete == null)
                {
                    this.response.OutputStream.Write(data.Array, data.Offset, data.Count);
                    return false;
                }

                Task writeTask = this.response.OutputStream.WriteAsync(data.Array, data.Offset, data.Count);
                if (writeTask.IsCompleted)
                {
                    if (writeTask.IsFaulted)
                    {
                        this.End(writeTask.Exception);
                    }

                    return false;
                }
                else
                {
                    writeTask.ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            End(task.Exception);
                        }

                        try
                        {
                            complete();
                        }
                        catch (Exception ex)
                        {
                            End(ex);
                        }
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                this.End(ex);
                return false;
            }
        }

        private bool Flush(Action complete)
        {
            if (complete == null)
            {
                this.response.OutputStream.Flush();
                return false;
            }

            Task flushTask = this.response.OutputStream.FlushAsync();
            if (flushTask.IsCompleted)
            {
                if (flushTask.IsFaulted)
                {
                    this.End(flushTask.Exception);
                }

                return false;
            }
            else
            {
                flushTask.ContinueWith(task => 
                {
                    if (task.IsFaulted)
                    {
                        End(task.Exception);
                    }

                    try
                    {
                        complete();
                    }
                    catch (Exception ex)
                    {
                        End(ex);
                    }
                });
                return false;
            }
        }

        internal void End(Exception ex)
        {
            if (ex != null)
            {
                Debug.Assert(false, "User exception: " + ex.ToString());
                this.response.Abort();
            }
            else
            {
                try
                {
                    this.response.Close();
                }
                catch (Exception ex1)
                {
                    Debug.Assert(false, "Close exception: " + ex1.ToString());
                    this.response.Abort();
                }
            }

            this.tcs.TrySetResult(null); // We don't care about user errors here, go process another request.
        }
    }
}
