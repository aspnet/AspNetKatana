using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Owin;

namespace Katana.WebApi.CallContent
{
    public class BodyDelegateWrapper : HttpContent
    {
        private readonly BodyDelegate _body;
        private readonly CancellationToken _callDisposed;

        public BodyDelegateWrapper(BodyDelegate body, CancellationToken callDisposed)
        {
            _body = body;
            _callDisposed = callDisposed;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var tcs = new TaskCompletionSource<object>();
            _body.Invoke(
                (data, callback) =>
                {
                    if (data.Array == null)
                    {
                        stream.Flush();
                        return false;
                    }
                    if (callback == null)
                    {
                        stream.Write(data.Array, data.Offset, data.Count);
                        return false;
                    }
                    var sr = stream.BeginWrite(
                        data.Array,
                        data.Offset,
                        data.Count,
                        ar =>
                        {
                            try
                            {
                                if (ar.CompletedSynchronously)
                                {
                                    return;
                                }
                                stream.EndWrite(ar);
                                callback();
                            }
                            catch
                            {
                                // TODO: pass exception in callback
                                callback();
                            }
                        }, null);
                    if (sr.CompletedSynchronously == false)
                    {
                        return true;
                    }
                    stream.EndWrite(sr);
                    return false;
                },
                ex =>
                {
                    if (ex == null)
                    {
                        tcs.TrySetResult(null);
                    }
                    else
                    {
                        tcs.TrySetException(ex);
                    }
                },
                _callDisposed);
            return tcs.Task;
        }

        protected override bool TryComputeLength(out long length)
        {
            throw new NotImplementedException();
        }
    }
}