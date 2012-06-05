using System;
using System.Linq;
using System.Threading;

namespace Gate.Middleware.StaticFiles
{
    internal enum BodyStreamCommand
    {
        Start,
        Pause,
        Stop,
        Cancel,
        Resume
    }

    internal enum BodyStreamState
    {
        Ready,
        Started,
        Paused,
        Stopped,
        Cancelled,
        Resumed
    }

    internal class BodyStream
    {
        private readonly StateMachine<BodyStreamCommand, BodyStreamState> stateMachine;

        public Func<ArraySegment<byte>, Action, bool> Write { get; private set; }
        public Action<Exception> End { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public BodyStream(Func<ArraySegment<byte>, Action, bool> write, Action<Exception> end, CancellationToken cancellationToken)
        {
            stateMachine = new StateMachine<BodyStreamCommand, BodyStreamState>();
            stateMachine.Initialize(BodyStreamState.Ready);

            stateMachine.MapTransition(BodyStreamCommand.Pause, BodyStreamState.Paused);
            stateMachine.MapTransition(BodyStreamCommand.Start, BodyStreamState.Started);
            stateMachine.MapTransition(BodyStreamCommand.Cancel, BodyStreamState.Cancelled);
            stateMachine.MapTransition(BodyStreamCommand.Resume, BodyStreamState.Resumed);
            stateMachine.MapTransition(BodyStreamCommand.Stop, BodyStreamState.Stopped);

            Write = write;
            End = end;
            CancellationToken = cancellationToken;
        }



        public void Start(Action start, Action dispose)
        {
            if (start == null)
            {
                throw new ArgumentNullException("start", "Missing start action for the BodyStream.");
            }

            stateMachine.On(BodyStreamCommand.Start, start);
            stateMachine.Invoke(BodyStreamCommand.Start);
            CancellationToken.Register(Cancel);

            if (dispose != null)
            {
                foreach (var command in new[] { BodyStreamCommand.Stop, BodyStreamCommand.Cancel })
                {
                    stateMachine.On(command, dispose);
                }
            }
        }

        public void Finish()
        {
            Stop();
            End(null);
        }

        public void SendBytes(ArraySegment<byte> part, Action continuation, Action complete)
        {
            if (!CanSend())
            {
                if (complete != null)
                {
                    complete.Invoke();
                }

                return;
            }

            Action resume = null;
            Action pause = () => { };

            if (continuation != null)
            {
                stateMachine.On(BodyStreamCommand.Resume, continuation);
                resume = Resume;
                pause = Pause;
            }

            // call on-next with back-pressure support
            if (Write(part, resume))
            {
                pause.Invoke();
            }

            if (complete != null)
            {
                complete.Invoke();
            }
        }

        public void Cancel()
        {
            stateMachine.Invoke(BodyStreamCommand.Cancel);
        }

        public void Pause()
        {
            stateMachine.Invoke(BodyStreamCommand.Pause);
        }

        public void Resume()
        {
            stateMachine.Invoke(BodyStreamCommand.Resume);
        }

        public void Stop()
        {
            stateMachine.Invoke(BodyStreamCommand.Stop);
        }

        public bool CanSend()
        {
            var validStates = new[] { BodyStreamState.Started, BodyStreamState.Resumed };
            return validStates.Contains(stateMachine.State);
        }
    }
}
