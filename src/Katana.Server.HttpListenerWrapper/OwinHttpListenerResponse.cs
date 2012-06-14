using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Katana.Server.HttpListenerWrapper
{
    internal class OwinHttpListenerResponse
    {
        private HttpListenerResponse response;
        private TaskCompletionSource<object> tcs;
        public Task Completion { get { return tcs.Task; } }

        public OwinHttpListenerResponse(HttpListenerResponse response, string responseStatus, 
            IDictionary<string, string[]> responseHeaders)
        {
            Debug.Assert(response != null);
            this.response = response;
            tcs = new TaskCompletionSource<object>();

            // Status
            string[] status = responseStatus.Split(' ');
            Debug.Assert(status.Length == 2);
            response.StatusCode = Int32.Parse(status[0]);
            response.StatusDescription = status[1];

            // Headers
            CopyResponseHeaders(responseHeaders);
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
                        response.ContentLength64 = Int32.Parse(value);
                    }
                    else if (header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
                        && value.Equals("chunked", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO, what about a mixed format value like chunked, otherTransferEncoding?
                        response.SendChunked = true;
                    }
                    else if (header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase)
                        && value.Equals("close", StringComparison.OrdinalIgnoreCase))
                    {
                        response.KeepAlive = false;
                    }
                    else if (header.Key.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase)
                        && value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: If this is an HTTP 1.0 request we and there is a KeepAlive header, we need to set response.KeepAlive = true;
                        response.KeepAlive = true;
                    }
                    else if (header.Key.Equals("WWW-Authenticate", StringComparison.OrdinalIgnoreCase))
                    {
                        // Uses InternalAdd to bypass a response header restriction
                        response.AddHeader(header.Key, value);
                    }
                    // else if ...
                    else
                    {
                        try
                        {
                            response.Headers.Add(header.Key, value);
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
                    return Flush(complete);
                }

                if (complete == null)
                {
                    response.OutputStream.Write(data.Array, data.Offset, data.Count);
                    return false;
                }

                Task writeTask = response.OutputStream.WriteAsync(data.Array, data.Offset, data.Count);
                if (writeTask.IsCompleted)
                {
                    if (writeTask.IsFaulted)
                    {
                        End(writeTask.Exception);
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
                End(ex);
                return false;
            }
        }

        private bool Flush(Action complete)
        {
            Task flushTask = response.OutputStream.FlushAsync();
            if (flushTask.IsCompleted)
            {
                if (flushTask.IsFaulted)
                {
                    End(flushTask.Exception);
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
                response.Abort();
            }
            else
            {
                try
                {
                    response.Close();
                }
                catch (Exception ex1)
                {
                    Debug.Assert(false, "Close exception: " + ex1.ToString());
                    response.Abort();
                }
            }
            tcs.TrySetResult(null); // We don't care about user errors here, go process another request.
        }
    }
}
