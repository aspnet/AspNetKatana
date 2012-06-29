namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Common static helper APIs accessed frome mutiple classes.
    /// </summary>
    internal static class Helpers
    {
        internal static async void CopyFromStreamToOwin(
            Stream readStream, 
            Func<ArraySegment<byte>, Action<Exception>, bool> write, 
            Action<Exception> end, 
            CancellationToken cancellationToken)
        {
            try
            {
                bool done = false;
                bool expectCallback;
                byte[] buffer = new byte[1024 * 4]; // Katana#3 - Pool these buffers?

                // Recycle unused TCS instances to limit redundant allocations when writes complete syncronously in a tight loop.
                TaskCompletionSource<object> recycledTcs = null;
                TaskCompletionSource<object> waitForWrite;
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    int read = await readStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (read == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        waitForWrite = recycledTcs ?? new TaskCompletionSource<object>();
                        expectCallback = write(
                            new ArraySegment<byte>(buffer, 0, read),
                            ex =>
                            {
                                if (ex == null)
                                {
                                    waitForWrite.TrySetResult(null);
                                }
                                else
                                {
                                    waitForWrite.TrySetException(ex);
                                }
                            });
                        if (expectCallback)
                        {
                            recycledTcs = null;
                            await waitForWrite.Task;
                        }
                        else
                        {
                            recycledTcs = waitForWrite;
                        }
                    }
                }
                while (!done);

                end(null);
            }
            catch (Exception ex)
            {
                end(ex);
            }
        }
    }
}
