using System;
using CFramework.Core.Attributes;
using CFramework.Core.BroadcastSystem;
using CFramework.Core.CommandSystem;

namespace CFramework.FSM
{
    public interface ITransitionHandler
    {
        Action Action { get; set; }
        Func<bool> CanTransition { get; set; }
        public Type EventDataType { get; }
    }

    public class TransitionBroadcastHandler<TBroadcastData, TState> : ITransitionHandler
        where TBroadcastData : IBroadcastData
    {
        public TState State { get; set; }
        public Action Action { get; set; }
        public Func<bool> CanTransition { get; set; }
        public Type EventDataType => typeof(TBroadcastData);

        [BroadcastHandler]
        private void OnBroadcast(TBroadcastData t)
        {
            if(CanTransition?.Invoke() ?? true) Action?.Invoke();
        }
    }

    public class TransitionCommandHandler<TCommandData, TState> : ITransitionHandler where TCommandData : ICommandData
    {
        public TState State { get; set; }
        public Action Action { get; set; }
        public Func<bool> CanTransition { get; set; }
        public Type EventDataType => typeof(TCommandData);

        [CommandHandler]
        private void OnCommand(TCommandData t)
        {
            if(CanTransition?.Invoke() ?? true) Action?.Invoke();
        }
    }
}