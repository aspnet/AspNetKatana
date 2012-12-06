using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    // TODO: Attempt overlapped writes?
    // FYI: In most cases the source will be a FileStream and the destination will be to the network.
    internal class StreamCopyOperation
    {
        private const int DefaultBufferSize = 1024 * 16;

        private TaskCompletionSource<object> _tcs;
        private Stream _source;
        private Stream _destination;
        private long? _bytesRemaining;
        private CancellationToken _cancel;
        private byte[] _buffer;

        private AsyncCallback _readCallback;
        private AsyncCallback _writeCallback;

        internal StreamCopyOperation(Stream source, Stream destination, CancellationToken cancel)
            : this(source, destination, null, DefaultBufferSize, cancel)
        {
        }

        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, CancellationToken cancel)
            : this(source, destination, bytesRemaining, DefaultBufferSize, cancel)
        {
        }

        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, int bufferSize, CancellationToken cancel)
            : this(source, destination, bytesRemaining, new byte[bufferSize], cancel)
        {
        }

        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, byte[] buffer, CancellationToken cancel)
        {
            Contract.Assert(source != null);
            Contract.Assert(destination != null);
            Contract.Assert(!bytesRemaining.HasValue || bytesRemaining.Value >= 0);
            Contract.Assert(buffer != null);

            _source = source;
            _destination = destination;
            _bytesRemaining = bytesRemaining;
            _cancel = cancel;
            _buffer = buffer;

            _tcs = new TaskCompletionSource<object>();
            _readCallback = new AsyncCallback(ReadCallback);
            _writeCallback = new AsyncCallback(WriteCallback);
        }

        internal byte[] Buffer
        {
            get { return _buffer; }
        }

        internal Task Start()
        {
            ReadNextSegment();
            return _tcs.Task;
        }

        private void Complete()
        {
            _tcs.TrySetResult(null);
        }

        private bool CheckCancelled()
        {
            if (_cancel.IsCancellationRequested)
            {
                _tcs.TrySetCanceled();
                return true;
            }
            return false;
        }

        private void Fail(Exception ex)
        {
            _tcs.TrySetException(ex);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void ReadNextSegment()
        {
            // The natural end of the range.
            if (_bytesRemaining.HasValue && _bytesRemaining.Value <= 0)
            {
                Complete();
                return;
            }

            if (CheckCancelled())
            {
                return;
            }

            try
            {
                int readLength = _buffer.Length;
                if (_bytesRemaining.HasValue)
                {
                    readLength = (int)Math.Min(_bytesRemaining.Value, (long)readLength);
                }
                IAsyncResult async = _source.BeginRead(_buffer, 0, readLength, _readCallback, null);

                if (async.CompletedSynchronously)
                {
                    int read = _source.EndRead(async);
                    WriteToOutputStream(read);
                }
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void ReadCallback(IAsyncResult async)
        {
            if (async.CompletedSynchronously)
            {
                return;
            }
                
            try
            {
                int read = _source.EndRead(async);
                WriteToOutputStream(read);
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void WriteToOutputStream(int count)
        {
            if (_bytesRemaining.HasValue)
            {
                _bytesRemaining -= count;
            }

            // End of the source stream.
            if (count == 0)
            {
                Complete();
                return;
            }

            if (CheckCancelled())
            {
                return;
            }

            try
            {
                IAsyncResult async = _destination.BeginWrite(_buffer, 0, count, _writeCallback, null);
                if (async.CompletedSynchronously)
                {
                    _destination.EndWrite(async);
                    ReadNextSegment();
                }
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="Redirecting")]
        private void WriteCallback(IAsyncResult async)
        {
            if (async.CompletedSynchronously)
            {
                return;
            }

            try
            {
                _destination.EndWrite(async);
                ReadNextSegment();
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }
    }
}
