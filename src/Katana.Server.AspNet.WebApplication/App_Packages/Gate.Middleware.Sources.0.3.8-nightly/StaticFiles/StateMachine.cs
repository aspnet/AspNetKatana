using System;
using System.Collections.Generic;

namespace Gate.Middleware.StaticFiles
{
    internal class StateMachine<TCommand, TState>
        where TCommand : struct
        where TState : struct
    {
        private readonly IDictionary<TCommand, Action> handlers = new Dictionary<TCommand, Action>();
        private readonly IDictionary<TCommand, TState> transitions = new Dictionary<TCommand, TState>();

        public TState State { get; private set; }

        public void Initialize(TState state)
        {
            State = state;
        }

        public StateMachine<TCommand, TState> MapTransition(TCommand command, TState state)
        {
            transitions[command] = state;
            return this;
        }

        public void On(TCommand command, Action handler)
        {
            handlers[command] = handler;
        }

        public void Invoke(TCommand command)
        {
            if (transitions.ContainsKey(command))
            {
                State = transitions[command];
            }

            if (handlers.ContainsKey(command))
            {
                handlers[command].Invoke();
            }
        }
    }
}
