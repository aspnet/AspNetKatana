using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Katana.Server.HttpListenerWrapper
{
    public static class Helpers
    {        
        public static async void CopyFromStreamToOwin(Stream readStream, Func<ArraySegment<byte>, Action, bool> write, 
            Action<Exception> end, CancellationToken cancellationToken)
        {
            try
            {
                bool done = false;
                bool expectCallback;
                byte[] buffer = new byte[1024 * 4];
                TaskCompletionSource<Object> waitForWrite;
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
                        waitForWrite = new TaskCompletionSource<Object>();
                        expectCallback = write(new ArraySegment<byte>(buffer, 0, read), () => waitForWrite.TrySetResult(null));
                        if (expectCallback)
                        {
                            await waitForWrite.Task;
                        }
                    }
                } while (!done);

                // Flush
                waitForWrite = new TaskCompletionSource<Object>();
                expectCallback = write(new ArraySegment<byte>(), () => waitForWrite.TrySetResult(null));
                if (expectCallback)
                {
                    await waitForWrite.Task;
                }

                end(null);
            }
            catch (Exception ex)
            {
                end(ex);
            }
        }
    }
}
