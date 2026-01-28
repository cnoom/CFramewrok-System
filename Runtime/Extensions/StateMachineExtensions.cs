using System;
using CFramework.Data;
using CFramework.FSM;

namespace CFramework.Extensions
{
    public class State : IState
    {
        public Action<Blackboard> OnEnter { get; set; }
        public Action<Blackboard> OnUpdate { get; set; }
        public Action<Blackboard> OnExit { get; set; }

        public void Enter(Blackboard blackboard)
        {
            OnEnter?.Invoke(blackboard);
        }

        public void Update(Blackboard blackboard)
        {
            OnUpdate?.Invoke(blackboard);
        }

        public void Exit(Blackboard blackboard)
        {
            OnExit?.Invoke(blackboard);
        }
    }

    public static class StateMachineExtensions
    {
        public static void AddStateEx<T>(this StateMachine<T> stateMachine, T key, Action<Blackboard> onEnter = null,
            Action<Blackboard> onUpdate = null, Action<Blackboard> onExit = null)
        {
            stateMachine.AddState(key, new State
            {
                OnEnter = onEnter,
                OnUpdate = onUpdate,
                OnExit = onExit
            });
        }
    }
}