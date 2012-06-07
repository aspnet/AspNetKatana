using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Katana.Server.AspNet
{
    public partial class OwinCallContext : IAsyncResult
    {
        private AsyncCallback _cb;
        private Exception _exception;

        public OwinCallContext(AsyncCallback cb, object extraData)
        {
            _cb = cb ?? NoopAsyncCallback;
            AsyncState = extraData;
        }

        private static readonly AsyncCallback NoopAsyncCallback =
            ar => { };

        private static readonly AsyncCallback ExtraAsyncCallback =
            ar => Trace.WriteLine("OwinHttpHandler: more than one call to complete the same AsyncResult");

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public bool IsCompleted { get; private set; }

        public WaitHandle AsyncWaitHandle
        {
            get { throw new InvalidOperationException("Entering a blocking wait state is not allowed"); }
        }

        public object AsyncState { get; private set; }

        public bool CompletedSynchronously { get; private set; }

        public CancellationToken CallDisposed { get { return _cancellationTokenSource.Token; } }

        public void Complete(bool completedSynchronously, Exception exception)
        {
            _exception = exception;
            CompletedSynchronously = completedSynchronously;
            IsCompleted = true;
            try
            {
                Interlocked.Exchange(ref _cb, ExtraAsyncCallback).Invoke(this);
            }
            catch (Exception ex)
            {
                // TODO: certain exception must never be caught - find out what those are and
                // rethrow if ex is one of them
                Trace.WriteLine("OwinHttpHandler: AsyncResult callback threw an exception. " + ex.Message);
            }
            _cancellationTokenSource.Cancel(false);
        }

        public static void End(IAsyncResult result)
        {
            if (!(result is OwinCallContext))
            {
                throw new InvalidOperationException("EndProcessRequest must be called with return value of BeginProcessRequest");
            }
            var self = ((OwinCallContext)result);
            if (self._exception != null)
            {
                throw new TargetInvocationException(self._exception);
            }
            if (!self.IsCompleted)
            {
                throw new InvalidOperationException("Calling EndProcessRequest before IsComplete is true is not allowed");
            }
        }
    }
}