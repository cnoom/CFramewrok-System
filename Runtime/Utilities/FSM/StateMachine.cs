using System;
using System.Collections.Generic;
using CFramework.Data;

namespace CFramework.FSM
{
    public class StateMachine<TState> : IDisposable
    {
        private readonly object _lock = new object();
        private readonly Dictionary<TState, IState> _states = new Dictionary<TState, IState>();
        private TState _currentKey;
        private IState _currentState;
        private bool _disposed;
        private bool _hasCurrentState;

        public StateMachine(Blackboard blackboard = null)
        {
            Blackboard = blackboard ?? new Blackboard();
        }
        public Blackboard Blackboard { get; }
        public Action OnDispose { get; set; }

        public bool HasCurrentState
        {
            get
            {
                lock (_lock)
                {
                    return _hasCurrentState;
                }
            }
        }

        public TState CurrentKey
        {
            get
            {
                lock (_lock)
                {
                    return _currentKey;
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if(_disposed) return;
                _disposed = true;
                _currentState?.Exit(Blackboard);
                _currentState = null;
                _hasCurrentState = false;
            }

            // 清除事件订阅者
            OnStateChanged = null;
            OnDispose?.Invoke();
            OnDispose = null;
        }

        public event Action<TState, TState> OnStateChanged; // (from, to)

        public void AddState(TState key, IState state)
        {
            if(key == null) throw new ArgumentNullException(nameof(key));
            if(state == null) throw new ArgumentNullException(nameof(state));

            lock (_lock)
            {
                _states[key] = state;
            }
        }

        public bool HasState(TState key)
        {
            lock (_lock)
            {
                return _states.ContainsKey(key);
            }
        }

        public void SetInitialState(TState key)
        {
            IState newState = null;
            IState oldState = null;

            lock (_lock)
            {
                if(!_states.TryGetValue(key, out IState state))
                {
                    throw new KeyNotFoundException($"Initial state key '{key}' not found.");
                }

                newState = state;
                oldState = _currentState;
                _currentState = state;
                _currentKey = key;
                _hasCurrentState = true;
            }

            // 在锁外调用 Enter，避免死锁
            oldState?.Exit(Blackboard);
            newState.Enter(Blackboard);
        }

        public bool TryChangeState(TState key)
        {
            IState newState = null;
            IState oldState = null;
            TState fromKey = default;
            TState toKey = default;
            var success = false;

            lock (_lock)
            {
                if(_disposed) return false;

                if(!_states.TryGetValue(key, out IState state))
                {
                    return false;
                }

                fromKey = _hasCurrentState ? _currentKey : default;
                toKey = key;
                oldState = _currentState;
                newState = state;
                _currentState = state;
                _currentKey = key;
                _hasCurrentState = true;
                success = true;
            }

            if(success)
            {
                try
                {
                    // 在锁外调用 Enter/Exit，避免死锁
                    oldState?.Exit(Blackboard);
                    newState.Enter(Blackboard);
                    OnStateChanged?.Invoke(fromKey, toKey);
                }
                catch
                {
                    // 如果 Enter 抛出异常，恢复旧状态
                    lock (_lock)
                    {
                        _currentState = oldState;
                        _currentKey = fromKey;
                        _hasCurrentState = oldState != null;
                    }
                    throw;
                }
            }

            return success;
        }

        public void ChangeState(TState key)
        {
            IState newState = null;
            IState oldState = null;
            TState fromKey = default;
            TState toKey = default;

            lock (_lock)
            {
                if(_disposed) throw new ObjectDisposedException(nameof(StateMachine<TState>));

                if(!_states.TryGetValue(key, out IState state))
                {
                    throw new KeyNotFoundException($"State key '{key}' not found.");
                }

                fromKey = _hasCurrentState ? _currentKey : default;
                toKey = key;
                oldState = _currentState;
                newState = state;
                _currentState = state;
                _currentKey = key;
                _hasCurrentState = true;
            }

            try
            {
                // 在锁外调用 Enter/Exit，避免死锁
                oldState?.Exit(Blackboard);
                newState.Enter(Blackboard);
                OnStateChanged?.Invoke(fromKey, toKey);
            }
            catch
            {
                // 如果 Enter 抛出异常，恢复旧状态
                lock (_lock)
                {
                    _currentState = oldState;
                    _currentKey = fromKey;
                    _hasCurrentState = oldState != null;
                }
                throw;
            }
        }

        public void Update()
        {
            IState currentState = null;

            lock (_lock)
            {
                if(_disposed) return;
                currentState = _currentState;
            }

            currentState?.Update(Blackboard);
        }

        public override string ToString()
        {
            return $"StateMachine<{typeof(TState).Name}>";
        }
    }
}