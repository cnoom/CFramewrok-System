using System;
using System.Collections.Generic;
using CFramework.Core;
using CFramework.Core.Attributes;
using CFramework.Core.BroadcastSystem;
using CFramework.Core.CommandSystem;

namespace CFramework.FSM
{
    public class StateMachineBinder<TState> : IDisposable
    {

        private readonly Dictionary<(TState from, TState to), IBindHandler<TState>> _broadcastBindHandlers = new Dictionary<(TState from, TState to), IBindHandler<TState>>();
        private readonly Dictionary<(TState from, TState to), IBindHandler<TState>> _commandBindHandlers = new Dictionary<(TState from, TState to), IBindHandler<TState>>();

        public StateMachineBinder(StateMachine<TState> stateMachine)
        {
            StateMachine = stateMachine;
            StateMachine.OnDispose += Dispose;
        }
        public StateMachine<TState> StateMachine { get; }

        public void Dispose()
        {
            ClearHandler(_broadcastBindHandlers);
            ClearHandler(_commandBindHandlers);
            StateMachine.OnDispose -= Dispose;
        }

        public void BindBroadcast<TBroadcastData>(TState from, TState to,
            Func<TBroadcastData, bool> canTransition = null) where TBroadcastData : IBroadcastData
        {
            if(!CheckState(from, to))
            {
                CF.LogWarning($"{StateMachine} From:{from} To:{to} 不存在状态，绑定失败");
                return;
            }

            BroadcastBindHandler<TState, TBroadcastData> handler = new BroadcastBindHandler<TState, TBroadcastData>(from, to, StateMachine, canTransition);
            if(!EnsureSingleHandler(from, to, _broadcastBindHandlers))
            {
                CF.LogWarning($"{StateMachine} From:{from} To:{to} 存在旧的广播绑定，已移除旧的绑定");
            }

            _broadcastBindHandlers[(from, to)] = handler;
        }

        public void BindCommand<TCommandData>(TState from, TState to, Func<TCommandData, bool> canTransition = null)
            where TCommandData : ICommandData
        {
            if(!CheckState(from, to))
            {
                CF.LogWarning($"{StateMachine} From:{from} To:{to} 不存在状态，绑定失败");
                return;
            }

            CommandBindHandler<TState, TCommandData> handler = new CommandBindHandler<TState, TCommandData>(from, to, StateMachine, canTransition);
            if(!EnsureSingleHandler(from, to, _commandBindHandlers))
            {
                CF.LogWarning($"{StateMachine} From:{from} To:{to} 存在旧的命令绑定，已移除旧的绑定");
            }

            _commandBindHandlers[(from, to)] = handler;
        }

        public void UnBind(TState from, TState to)
        {
            UnBindBroadcast(from, to);
            UnBindCommand(from, to);
        }

        public void UnBindBroadcast(TState from, TState to)
        {
            if(!_broadcastBindHandlers.TryGetValue((from, to), out IBindHandler<TState> handler)) return;
            handler.Dispose();
            _broadcastBindHandlers.Remove((from, to));
        }

        public void UnBindCommand(TState from, TState to)
        {
            if(!_commandBindHandlers.TryGetValue((from, to), out IBindHandler<TState> handler)) return;
            handler.Dispose();
            _commandBindHandlers.Remove((from, to));
        }

        private bool EnsureSingleHandler(TState from, TState to,
            Dictionary<(TState from, TState to), IBindHandler<TState>> handlers)
        {
            if(!handlers.TryGetValue((from, to), out IBindHandler<TState> oldHandler)) return true;
            oldHandler.Dispose();
            handlers.Remove((from, to));
            return false;
        }

        private bool CheckState(TState from, TState to)
        {
            if(StateMachine.HasState(from) && StateMachine.HasState(to)) return true;
            return false;
        }

        private void ClearHandler(Dictionary<(TState from, TState to), IBindHandler<TState>> handlers)
        {
            foreach (IBindHandler<TState> handler in handlers.Values)
            {
                handler.Dispose();
            }

            handlers.Clear();
        }
    }

    internal interface IBindHandler<TState> : IDisposable
    {
        public TState From { get; set; }
        public TState To { get; set; }
        public StateMachine<TState> StateMachine { get; }
    }

    internal class BroadcastBindHandler<TState, TBroadcastData> : IBindHandler<TState>
        where TBroadcastData : IBroadcastData
    {

        public BroadcastBindHandler(TState from, TState to, StateMachine<TState> machine,
            Func<TBroadcastData, bool> canTransition)
        {
            From = from;
            To = to;
            StateMachine = machine;
            CanTransition = canTransition;
            CF.TryRegisterHandler(this);
        }
        public Func<TBroadcastData, bool> CanTransition { get; set; }
        public TState From { get; set; }
        public TState To { get; set; }
        public StateMachine<TState> StateMachine { get; }

        public void Dispose()
        {
            CF.TryUnregisterHandler(this);
        }

        [BroadcastHandler]
        private void OnBroadcast(TBroadcastData t)
        {
            if(StateMachine == null) return;
            if(!StateMachine.HasCurrentState) return;
            if(!EqualityComparer<TState>.Default.Equals(StateMachine.CurrentKey, From)) return;
            if(CanTransition == null || CanTransition(t)) ChangeState();
        }

        private void ChangeState()
        {
            StateMachine.TryChangeState(To);
        }
    }

    internal class CommandBindHandler<TState, TCommandData> : IBindHandler<TState>
    {

        public CommandBindHandler(TState from, TState to, StateMachine<TState> machine,
            Func<TCommandData, bool> canTransition)
        {
            From = from;
            To = to;
            StateMachine = machine;
            CanTransition = canTransition;
            CF.TryRegisterHandler(this);
        }
        public Func<TCommandData, bool> CanTransition { get; set; }
        public TState From { get; set; }
        public TState To { get; set; }
        public StateMachine<TState> StateMachine { get; }

        public void Dispose()
        {
            CF.TryUnregisterHandler(this);
        }

        [CommandHandler]
        private void OnCommand(TCommandData t)
        {
            if(StateMachine == null) return;
            if(!StateMachine.HasCurrentState) return;
            if(!EqualityComparer<TState>.Default.Equals(StateMachine.CurrentKey, From)) return;
            if(CanTransition == null || CanTransition(t)) ChangeState();
        }

        private void ChangeState()
        {
            StateMachine.TryChangeState(To);
        }
    }
}