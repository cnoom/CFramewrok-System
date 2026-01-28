using System.Collections.Generic;
using System.Threading;
using CFramework.Core;
using CFramework.Core.Attributes;
using CFramework.Core.Interfaces.LifeScope;
using CFramework.Core.Log;
using CFramework.Core.ModuleSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CFramework.Systems.TimerSystem
{
    /// <summary>
    ///     统一管理按时间触发的定时任务。
    /// </summary>
    [AutoModule("c计时器系统", "统一处理项目中的时间")]
    public sealed class TimerSystemModule : IModule, IRegister, IUnRegister, IUpdate, IPauseHandler, ITimerSystem
    {
        private readonly List<string> _iterationBuffer = new List<string>();

        private readonly Dictionary<string, TimerEntry> _timers = new Dictionary<string, TimerEntry>();

        private bool _applicationPaused;
        private CFLogger _logger;

        public void OnApplicationPause(bool isPaused)
        {
            _applicationPaused = isPaused;
        }

        public void Register()
        {
            _logger = CF.CreateLogger("Timer");
        }

        public void UnRegister()
        {
            _timers.Clear();
            _iterationBuffer.Clear();
        }

        public void Update()
        {
            if(_applicationPaused) return;
            if(_timers.Count == 0) return;

            float scaledDelta = Time.deltaTime;
            float unscaledDelta = Time.unscaledDeltaTime;

            _iterationBuffer.Clear();
            foreach (KeyValuePair<string, TimerEntry> kv in _timers)
            {
                _iterationBuffer.Add(kv.Key);
            }

            for(var i = 0; i < _iterationBuffer.Count; i++)
            {
                string id = _iterationBuffer[i];
                if(!_timers.TryGetValue(id, out TimerEntry entry))
                {
                    continue;
                }

                if(entry.Paused)
                {
                    _timers[id] = entry;
                    continue;
                }

                float dt = entry.UseUnscaledTime ? unscaledDelta : scaledDelta;
                entry.RemainingSeconds -= dt;

                if(entry.RemainingSeconds > 0f)
                {
                    _timers[id] = entry;
                    continue;
                }

                entry.FiredCount++;
                bool willRepeat = entry.Loop && entry.RemainingLoops != 1;

                CF.Broadcast(new TimerBroadcasts.TimerCompleted(
                    entry.Id,
                    entry.Tag,
                    entry.FiredCount,
                    willRepeat));

                if(entry.Loop && entry.RemainingLoops != 1)
                {
                    if(entry.RemainingLoops > 0)
                    {
                        entry.RemainingLoops--;
                    }

                    entry.RemainingSeconds += entry.DurationSeconds;
                    _timers[id] = entry;
                }
                else
                {
                    _timers.Remove(id);
                }
            }
        }

        private struct TimerEntry
        {
            public string Id;
            public float DurationSeconds;
            public float RemainingSeconds;
            public bool Loop;
            public int RemainingLoops;
            public bool UseUnscaledTime;
            public string Tag;
            public bool Paused;
            public int FiredCount;
        }

        #region Command Handlers

        [CommandHandler]
        private UniTask OnStartTimer(TimerCommands.StartTimer cmd, CancellationToken token)
        {
            if(string.IsNullOrEmpty(cmd.Id))
            {
                _logger?.LogWarning("StartTimerCommand 收到空 Id，已忽略。");
                return UniTask.CompletedTask;
            }

            if(cmd.DurationSeconds <= 0f)
            {
                _logger?.LogWarning($"StartTimerCommand DurationSeconds <= 0: {cmd.DurationSeconds}");
                return UniTask.CompletedTask;
            }

            TimerEntry entry = new TimerEntry
            {
                Id = cmd.Id,
                DurationSeconds = cmd.DurationSeconds,
                RemainingSeconds = cmd.DurationSeconds,
                Loop = cmd.Loop,
                RemainingLoops = cmd.RepeatCount,
                UseUnscaledTime = cmd.UseUnscaledTime,
                Tag = cmd.Tag,
                Paused = false,
                FiredCount = 0
            };

            _timers[cmd.Id] = entry;
            return UniTask.CompletedTask;
        }

        [CommandHandler]
        private UniTask OnStopTimer(TimerCommands.StopTimer cmd, CancellationToken token)
        {
            string id = cmd.Id;
            if(string.IsNullOrEmpty(id)) return UniTask.CompletedTask;

            if(_timers.TryGetValue(id, out TimerEntry entry))
            {
                _timers.Remove(id);
                CF.Broadcast(new TimerBroadcasts.TimerCancelled(entry.Id, entry.Tag));
            }

            return UniTask.CompletedTask;
        }

        [CommandHandler]
        private UniTask OnStopTimersByTag(TimerCommands.StopTimersByTag cmd, CancellationToken token)
        {
            string tag = cmd.Tag;
            if(string.IsNullOrEmpty(tag)) return UniTask.CompletedTask;

            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, TimerEntry> kv in _timers)
            {
                if(kv.Value.Tag == tag)
                {
                    toRemove.Add(kv.Key);
                }
            }

            for(var i = 0; i < toRemove.Count; i++)
            {
                string id = toRemove[i];
                if(_timers.TryGetValue(id, out TimerEntry entry))
                {
                    _timers.Remove(id);
                    CF.Broadcast(new TimerBroadcasts.TimerCancelled(entry.Id, entry.Tag));
                }
            }

            return UniTask.CompletedTask;
        }

        [CommandHandler]
        private UniTask OnPauseTimer(TimerCommands.PauseTimer cmd, CancellationToken token)
        {
            string id = cmd.Id;
            if(string.IsNullOrEmpty(id)) return UniTask.CompletedTask;
            if(!_timers.TryGetValue(id, out TimerEntry entry)) return UniTask.CompletedTask;

            entry.Paused = true;
            _timers[id] = entry;
            return UniTask.CompletedTask;
        }

        [CommandHandler]
        private UniTask OnResumeTimer(TimerCommands.ResumeTimer cmd, CancellationToken token)
        {
            string id = cmd.Id;
            if(string.IsNullOrEmpty(id)) return UniTask.CompletedTask;
            if(!_timers.TryGetValue(id, out TimerEntry entry)) return UniTask.CompletedTask;

            entry.Paused = false;
            _timers[id] = entry;
            return UniTask.CompletedTask;
        }

        [CommandHandler]
        private UniTask OnPauseTimersByTag(TimerCommands.PauseTimersByTag cmd, CancellationToken token)
        {
            string tag = cmd.Tag;
            if(string.IsNullOrEmpty(tag)) return UniTask.CompletedTask;

            foreach (string key in _timers.Keys)
            {
                TimerEntry entry = _timers[key];
                if(entry.Tag == tag)
                {
                    entry.Paused = true;
                    _timers[key] = entry;
                }
            }

            return UniTask.CompletedTask;
        }

        [CommandHandler]
        private UniTask OnResumeTimersByTag(TimerCommands.ResumeTimersByTag cmd, CancellationToken token)
        {
            string tag = cmd.Tag;
            if(string.IsNullOrEmpty(tag)) return UniTask.CompletedTask;

            foreach (string key in _timers.Keys)
            {
                TimerEntry entry = _timers[key];
                if(entry.Tag == tag)
                {
                    entry.Paused = false;
                    _timers[key] = entry;
                }
            }

            return UniTask.CompletedTask;
        }

        #endregion

        #region Query Handlers

        [QueryHandler]
        private UniTask<bool> OnHasTimer(TimerQueries.HasTimer q, CancellationToken token)
        {
            if(string.IsNullOrEmpty(q.Id)) return UniTask.FromResult(false);
            return UniTask.FromResult(_timers.ContainsKey(q.Id));
        }

        [QueryHandler]
        private UniTask<float> OnGetRemainingSeconds(TimerQueries.GetRemainingSeconds q, CancellationToken token)
        {
            if(string.IsNullOrEmpty(q.Id)) return UniTask.FromResult(-1f);
            if(!_timers.TryGetValue(q.Id, out TimerEntry entry)) return UniTask.FromResult(-1f);
            return UniTask.FromResult(Mathf.Max(0f, entry.RemainingSeconds));
        }

        [QueryHandler]
        private UniTask<TimerInfo> OnGetTimerInfo(TimerQueries.GetTimerInfo q, CancellationToken token)
        {
            if(string.IsNullOrEmpty(q.Id)) return default;
            if(!_timers.TryGetValue(q.Id, out TimerEntry entry)) return default;

            return UniTask.FromResult(new TimerInfo(
                entry.Id,
                entry.DurationSeconds,
                Mathf.Max(0f, entry.RemainingSeconds),
                entry.Loop,
                entry.RemainingLoops,
                entry.UseUnscaledTime,
                entry.Tag,
                entry.Paused));
        }

        #endregion
    }
}