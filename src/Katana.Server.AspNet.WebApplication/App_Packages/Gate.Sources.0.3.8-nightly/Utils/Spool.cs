using System;
using System.Threading;

namespace Gate.Utils
{
    internal class Spool
    {
        readonly bool _eagerPull;

        public Spool()
        {
        }

        public Spool(bool eagerPull)
        {
            _eagerPull = eagerPull;
        }

        public bool Push(ArraySegment<byte> data, Action continuation)
        {
            //todo - protect against concurrent async calls
            lock (_asyncPull)
            {
                // drop onto outstanding pull operations first
                while (data.Count != 0 && _asyncPull.Data.Count != 0)
                {
                    Drain(data, _asyncPull.Data, (a, b, c) =>
                    {
                        data = a;
                        _asyncPull.Data = b;
                        _asyncPull.Retval[0] += c;
                    });
                    if (_asyncPull.Data.Count == 0 && _asyncPull.Continuation != null)
                    {
                        var pullContinuation = _asyncPull.Continuation;
                        _asyncPull.Continuation = null;
                        pullContinuation();
                    }
                }

                // release partially filled when eager
                if (_eagerPull && _asyncPull.Retval[0] != 0 && _asyncPull.Continuation != null)
                {
                    var pullContinuation = _asyncPull.Continuation;
                    _asyncPull.Continuation = null;
                    pullContinuation();
                }

                // push fully consumed
                if (data.Count == 0)
                {
                    return false;
                }

                // delay if possible
                if (continuation != null)
                {
                    lock (_asyncPush)
                    {
                        _asyncPush.Data = data;
                        _asyncPush.Continuation = continuation;
                        return true;
                    }
                }

                // otherwise spool synchronously
                lock (_buffer)
                {
                    _buffer.Push(data);
                    return false;
                }
            }
        }

        public void PushComplete()
        {
            lock (_asyncPull)
            {
                _complete = true;
                if (_asyncPull.Continuation != null)
                {
                    var pullContinuation = _asyncPull.Continuation;
                    _asyncPull.Continuation = null;
                    pullContinuation();
                }
            }
        }

        public bool Pull(ArraySegment<byte> data, int[] retval, Action continuation)
        {
            Action exitGate = null;
            lock (_asyncPush)
            {
                // draw from buffer and outstanding push operations first
                while (data.Count != 0 && (_buffer.Data.Count != 0 || _asyncPush.Data.Count != 0))
                {
                    lock (_buffer)
                    {
                        _buffer.Drain(data, (d1, c) =>
                        {
                            data = d1;
                            retval[0] += c;
                        });
                    }
                    if (data.Count == 0) return false;
                    Drain(_asyncPush.Data, data, (d0, d1, c) =>
                    {
                        _asyncPush.Data = d0;
                        data = d1;
                        retval[0] += c;
                    });
                    if (_asyncPush.Data.Count == 0 && _asyncPush.Continuation != null)
                    {
                        var pushContinuation = _asyncPush.Continuation;
                        _asyncPush.Continuation = null;
                        pushContinuation();
                    }
                }
            }
                
            // return partially filled when eager
            if (_eagerPull && retval[0] != 0)
            {
                return false;
            }

            // pull fully satisfied
            if (data.Count == 0)
            {
                return false;
            }

            //todo - there's a simultaneous push-pull problem entering this lock...
            lock (_asyncPull)
            {
                lock (_asyncPush)
                {
                    if (_complete) return false;

                    _asyncPull.Data = data;
                    _asyncPull.Retval = retval;
                    if (continuation != null)
                    {
                        _asyncPull.Continuation = continuation;
                    }
                    else
                    {
                        var gate = new ManualResetEvent(false);
                        _asyncPull.Continuation = () => { gate.Set(); };
                        exitGate = () => { gate.WaitOne(); };
                    }
                }
            }


            if (exitGate != null)
            {
                exitGate();
                return false;
            }

            return true;
        }


        static void Drain(
            ArraySegment<byte> source,
            ArraySegment<byte> destination,
            Action<ArraySegment<byte>, ArraySegment<byte>, int> result)
        {
            var copied = Math.Min(source.Count, destination.Count);
            if (copied == 0) return;
            Array.Copy(source.Array, source.Offset, destination.Array, destination.Offset, copied);
            result(
                source.Count == copied ? Empty : new ArraySegment<byte>(source.Array, source.Offset + copied, source.Count - copied),
                destination.Count == copied ? Empty : new ArraySegment<byte>(destination.Array, destination.Offset + copied, destination.Count - copied),
                copied);
        }

        static readonly ArraySegment<byte> Empty = new ArraySegment<byte>(new byte[0], 0, 0);

        readonly AsyncOp _asyncPush = new AsyncOp();
        readonly AsyncOp _asyncPull = new AsyncOp();

        class AsyncOp
        {
            public AsyncOp()
            {
                Data = Empty;
                Retval = new int[1];
            }

            public ArraySegment<byte> Data { get; set; }
            public int[] Retval { get; set; }
            public Action Continuation { get; set; }
        }


        readonly Buffer _buffer = new Buffer();
        bool _complete;

        class Buffer
        {
            public Buffer()
            {
                Data = Empty;
            }

            public ArraySegment<byte> Data { get; set; }

            public void Push(ArraySegment<byte> data)
            {
                //TODO- rolling spool pages - spooling to a contiguous array is temporary
                var concat = new ArraySegment<byte>(new byte[Data.Count + data.Count]);
                Array.Copy(Data.Array, Data.Offset, concat.Array, 0, Data.Count);
                Array.Copy(data.Array, data.Offset, concat.Array, Data.Count, data.Count);
                Data = concat;
            }

            public void Drain(ArraySegment<byte> data, Action<ArraySegment<byte>, int> result)
            {
                var copied = Math.Min(data.Count, Data.Count);
                if (copied == 0) return;
                Array.Copy(Data.Array, Data.Offset, data.Array, data.Offset, copied);
                Data = new ArraySegment<byte>(Data.Array, Data.Offset + copied, Data.Count - copied);
                result(new ArraySegment<byte>(data.Array, data.Offset + copied, data.Count - copied), copied);
            }
        }
    }
}