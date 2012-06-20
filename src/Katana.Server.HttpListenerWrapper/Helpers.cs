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
            Func<ArraySegment<byte>, Action, bool> write, 
            Action<Exception> end, 
            CancellationToken cancellationToken)
        {
            try
            {
                bool done = false;
                bool expectCallback;
                byte[] buffer = new byte[1024 * 4];
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
                        waitForWrite = new TaskCompletionSource<object>();
                        expectCallback = write(new ArraySegment<byte>(buffer, 0, read), () => waitForWrite.TrySetResult(null));
                        if (expectCallback)
                        {
                            await waitForWrite.Task;
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
