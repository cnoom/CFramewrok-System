using System;
using System.Collections.Generic;
using System.Threading;
using CFramework.Core;
using CFramework.Core.Attributes;
using CFramework.Core.Interfaces.LifeScope;
using CFramework.Core.Log;
using CFramework.Core.ModuleSystem;
using CFramework.Systems.AssetsSystem;
using CFramework.Systems.SaveSystem;
using CFramework.Systems.SaveSystem.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CFramework.Systems.AudioSystem
{
    [AutoModule("c音频系统", "管理游戏音效和背景音乐的播放、暂停和音量控制"), ModuleDependsOn(typeof(IAssetsSystem)), ModuleDependsOn(typeof(ISaveSystem))]
    public sealed class AudioSystemModule : IModule, IAudioSystem, IRegisterAsync, IUnRegister, IUpdate, IPauseHandler
    {

        // Save
        private const string SaveSlotId = "audio";
        private const string SaveKeyInitialized = "initialized";
        private const string SaveKeyMasterVolume = "masterVolume";
        private const string SaveKeyMusicVolume = "musicVolume";
        private const string SaveKeySfxVolume = "sfxVolume";
        private const string SaveKeyMasterMute = "masterMute";
        private const string SaveKeyMusicMute = "musicMute";
        private const string SaveKeySfxMute = "sfxMute";
        private readonly List<AudioSource> _activeSfx = new List<AudioSource>();
        private readonly Dictionary<string, int> _clipActiveCounts = new Dictionary<string, int>();
        private readonly Dictionary<string, double> _clipLastPlayTimeSec = new Dictionary<string, double>();
        private readonly Dictionary<AudioSource, string> _sfxKeys = new Dictionary<AudioSource, string>();

        // SFX pool
        private readonly List<AudioSource> _sfxPool = new List<AudioSource>();
        private readonly Dictionary<AudioSource, int> _sfxPriorities = new Dictionary<AudioSource, int>();
        private AudioConfig _config;
        private string _currentMusicKey;
        private string _fadingOutKey;

        // Previous (fading out) music
        private AudioSource _fadingOutSource;
        private CFLogger _logger;
        private bool _masterMute;

        // Volumes
        private float _masterVolume = 1f;

        // Music
        private AudioSource _musicA;
        private string _musicAKey;
        private AudioSource _musicB;
        private string _musicBKey;
        private float _musicFadeDuration;
        private float _musicFadeTimeLeft;
        private bool _musicMute;
        private float _musicStartVolume = 1f;
        private float _musicTargetVolume = 1f;
        private float _musicVolume = 1f;
        private float _prevFadeDuration;
        private float _prevFadeTimeLeft;
        private float _prevStartVolume; // logical volume before master/music factors
        private bool _sfxMute;
        private float _sfxVolume = 1f;

        private Slot _slot;
        private bool _usingA = true;

        #region Pause

        public void OnApplicationPause(bool isPaused)
        {
            if(!(_config != null && _config.respectGlobalPause)) return;
            AudioSource src = _usingA ? _musicA : _musicB;
            if(isPaused)
            {
                if(src && src.isPlaying) src.Pause();
                foreach (AudioSource s in _activeSfx)
                {
                    if(s && s.isPlaying)
                        s.Pause();
                }
            }
            else
            {
                if(src && src.clip && !src.isPlaying) src.UnPause();
                foreach (AudioSource s in _activeSfx)
                {
                    if(s && s.clip && !s.isPlaying)
                        s.UnPause();
                }
            }
        }

        #endregion

        public async UniTask RegisterAsync(CancellationToken cancellationToken)
        {
            _logger = CF.CreateLogger("Audio");

            await CF.Execute(new AssetsCommands.RegisterAssetReceiver(typeof(AudioConfig)));
            // 直接通过 AssetsSystem 加载 AudioConfig
            _config = await CF.Query<AssetsQueries.Asset, AudioConfig>(
                new AssetsQueries.Asset("AudioConfig"));

            if(!_config)
            {
                _logger.LogWarning("AudioConfig 未找到，使用默认值。");
                _masterVolume = 1f;
                _musicVolume = 1f;
                _sfxVolume = 1f;
            }
            else
            {
                _masterVolume = Mathf.Clamp01(_config.masterVolume);
                _musicVolume = Mathf.Clamp01(_config.musicVolume);
                _sfxVolume = Mathf.Clamp01(_config.sfxVolume);
                _masterMute = _config.masterMute;
                _musicMute = _config.musicMute;
                _sfxMute = _config.sfxMute;
            }

            await LoadPersistedAudioSettingsAsync();

            CreateAudioListenerIfNone();
            CreateMusicSources();
            PrewarmSfxPool(_config != null ? _config.sfxPoolInitialSize : 8);
        }


        public void UnRegister()
        {
            StopAll();
            foreach (AudioSource s in _sfxPool)
            {
                if(s)
                    Object.Destroy(s.gameObject);
            }
            _sfxPool.Clear();
            _activeSfx.Clear();
            if(_musicA) Object.Destroy(_musicA.gameObject);
            if(_musicB) Object.Destroy(_musicB.gameObject);

            // 释放 AudioConfig 引用
            if(!_config) return;
            CF.Execute(new AssetsCommands.ReleaseAsset<ScriptableObject>("CF_AudioConfig")).Forget();
            _musicAKey = null;
            _config = null;
        }

        public void Update()
        {
            // Music fade-in (current)
            if(_musicFadeTimeLeft > 0f)
            {
                _musicFadeTimeLeft -= Time.deltaTime;
                float t = _musicFadeDuration > 0f ? 1f - _musicFadeTimeLeft / _musicFadeDuration : 1f;
                float v = Mathf.Lerp(_musicStartVolume, _musicTargetVolume, t);
                AudioSource src = _usingA ? _musicA : _musicB;
                if(src) src.volume = ApplyMasterMusic(v);
                if(_musicFadeTimeLeft <= 0f)
                {
                    _musicFadeTimeLeft = 0f;
                }
            }

            // Music fade-out (previous)
            if(_fadingOutSource && _prevFadeTimeLeft > 0f)
            {
                _prevFadeTimeLeft -= Time.deltaTime;
                float t = _prevFadeDuration > 0f ? 1f - _prevFadeTimeLeft / _prevFadeDuration : 1f;
                float v = Mathf.Lerp(_prevStartVolume, 0f, t);
                _fadingOutSource.volume = ApplyMasterMusic(v);
                if(_prevFadeTimeLeft <= 0f)
                {
                    _prevFadeTimeLeft = 0f;
                    // stop and cleanup previous
                    string toReleaseKey = _fadingOutKey;
                    AudioClip audioSource = _fadingOutSource.clip;
                    _fadingOutSource.Stop();
                    _fadingOutSource.clip = null;
                    _fadingOutSource.volume = 0f;
                    _fadingOutSource = null;
                    _fadingOutKey = null;
                    if(!string.IsNullOrEmpty(toReleaseKey))
                    {
                        CF.Execute(new AssetsCommands.ReleaseAsset<AudioClip>(toReleaseKey)).Forget();
                    }
                }
            }

            // 清理完成的 SFX
            for(int i = _activeSfx.Count - 1; i >= 0; i--)
            {
                AudioSource s = _activeSfx[i];
                if(!s || !s.isPlaying)
                {
                    string toReleaseKey = null;
                    AudioClip clip = null;
                    if(s && _sfxKeys.TryGetValue(s, out string k))
                    {
                        toReleaseKey = k;
                        clip = s.clip;
                        _sfxKeys.Remove(s);
                        // 维护每 Clip 并发计数
                        if(_clipActiveCounts.TryGetValue(k, out int cnt))
                        {
                            cnt = Mathf.Max(0, cnt - 1);
                            if(cnt == 0)
                            {
                                _clipActiveCounts.Remove(k);
                            }
                            else
                            {
                                _clipActiveCounts[k] = cnt;
                            }
                        }
                    }

                    if(s)
                    {
                        s.Stop();
                        s.clip = null;
                        s.volume = 0f;
                        _sfxPriorities.Remove(s);
                    }

                    _activeSfx.RemoveAt(i);
                    if(s) _sfxPool.Add(s);
                    if(!string.IsNullOrEmpty(toReleaseKey))
                    {
                        CF.Execute(new AssetsCommands.ReleaseAsset<AudioClip>(toReleaseKey)).Forget();
                    }
                }
                else
                {
                    // 应用音量
                    s.volume = ApplyMasterSfx(s.volume);
                }
            }
        }

        private void CreateAudioListenerIfNone()
        {
            if(Object.FindObjectOfType<AudioListener>() == null)
            {
                GameObject go = new GameObject("CF_AudioListener");
                go.AddComponent<AudioListener>();
                Object.DontDestroyOnLoad(go);
            }
        }

        private void CreateMusicSources()
        {
            _musicA = CreateSource("CF_Music_A");
            _musicB = CreateSource("CF_Music_B");
            _musicA.loop = true;
            _musicB.loop = true;
            _musicA.playOnAwake = false;
            _musicB.playOnAwake = false;
            _musicA.volume = 0f;
            _musicB.volume = 0f;
        }

        private AudioSource CreateSource(string name)
        {
            GameObject go = new GameObject(name);
            Object.DontDestroyOnLoad(go);
            return go.AddComponent<AudioSource>();
        }

        private void PrewarmSfxPool(int count)
        {
            count = Mathf.Max(count, 0);
            for(var i = 0; i < count; i++)
            {
                AudioSource s = CreateSource("CF_SFX");
                s.playOnAwake = false;
                s.loop = false;
                s.volume = 0f;
                _sfxPool.Add(s);
            }
        }

        private async UniTask LoadPersistedAudioSettingsAsync()
        {
            try
            {
                Slot container =
                    await CF.Query<SaveQueries.CurrentProfileSlot, Slot>(
                        new SaveQueries.CurrentProfileSlot(SaveSlotId));
                if(container == null)
                {
                    return;
                }

                _slot = container;

                bool initialized = container.GetBool(SaveKeyInitialized);
                if(!initialized)
                {
                    container.SetFloat(SaveKeyMasterVolume, _masterVolume);
                    container.SetFloat(SaveKeyMusicVolume, _musicVolume);
                    container.SetFloat(SaveKeySfxVolume, _sfxVolume);
                    container.SetBool(SaveKeyMasterMute, _masterMute);
                    container.SetBool(SaveKeyMusicMute, _musicMute);
                    container.SetBool(SaveKeySfxMute, _sfxMute);
                    container.SetBool(SaveKeyInitialized, true);
                }
                else
                {
                    _masterVolume = Mathf.Clamp01(container.GetFloat(SaveKeyMasterVolume, _masterVolume));
                    _musicVolume = Mathf.Clamp01(container.GetFloat(SaveKeyMusicVolume, _musicVolume));
                    _sfxVolume = Mathf.Clamp01(container.GetFloat(SaveKeySfxVolume, _sfxVolume));
                    _masterMute = container.GetBool(SaveKeyMasterMute, _masterMute);
                    _musicMute = container.GetBool(SaveKeyMusicMute, _musicMute);
                    _sfxMute = container.GetBool(SaveKeySfxMute, _sfxMute);
                }
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
            }
        }

        private void PersistVolumes()
        {
            if(_slot == null)
            {
                return;
            }

            _slot.SetFloat(SaveKeyMasterVolume, _masterVolume);
            _slot.SetFloat(SaveKeyMusicVolume, _musicVolume);
            _slot.SetFloat(SaveKeySfxVolume, _sfxVolume);
        }

        private void PersistMutes()
        {
            if(_slot == null)
            {
                return;
            }

            _slot.SetBool(SaveKeyMasterMute, _masterMute);
            _slot.SetBool(SaveKeyMusicMute, _musicMute);
            _slot.SetBool(SaveKeySfxMute, _sfxMute);
        }

        private float ApplyMasterMusic(float v)
        {
            if(_masterMute || _musicMute) return 0f;
            return Mathf.Clamp01(v * _masterVolume * _musicVolume);
        }

        private float ApplyMasterSfx(float v)
        {
            if(_masterMute || _sfxMute) return 0f;
            return Mathf.Clamp01(v * _masterVolume * _sfxVolume);
        }

        private float CurrentMusicFactor()
        {
            if(_masterMute || _musicMute) return 0f;
            return Mathf.Clamp01(_masterVolume * _musicVolume);
        }

        private void StopAll()
        {
            // release music keys
            if(!string.IsNullOrEmpty(_musicAKey))
            {
                CF.Execute(new AssetsCommands.ReleaseAsset<AudioClip>(_musicAKey));
                _musicAKey = null;
            }

            if(!string.IsNullOrEmpty(_musicBKey))
            {
                CF.Execute(new AssetsCommands.ReleaseAsset<AudioClip>(_musicBKey));
                _musicBKey = null;
            }

            if(!string.IsNullOrEmpty(_fadingOutKey))
            {
                CF.Execute(new AssetsCommands.ReleaseAsset<AudioClip>(_fadingOutKey));
                _fadingOutKey = null;
            }

            if(_musicA)
            {
                _musicA.Stop();
                _musicA.clip = null;
                _musicA.volume = 0f;
            }

            if(_musicB)
            {
                _musicB.Stop();
                _musicB.clip = null;
                _musicB.volume = 0f;
            }

            // release all sfx keys
            foreach (KeyValuePair<AudioSource, string> kv in _sfxKeys)
            {
                if(!string.IsNullOrEmpty(kv.Value))
                {
                    CF.Execute(new AssetsCommands.ReleaseAsset<AudioClip>(kv.Value));
                }
            }

            _sfxKeys.Clear();

            foreach (AudioSource s in _activeSfx)
            {
                if(s)
                {
                    s.Stop();
                    s.clip = null;
                    s.volume = 0f;
                    _sfxPool.Add(s);
                }
            }

            _activeSfx.Clear();
            _fadingOutSource = null;
            _prevFadeTimeLeft = 0f;
            _prevFadeDuration = 0f;
            _prevStartVolume = 0f;
            _musicFadeTimeLeft = 0f;
            _musicFadeDuration = 0f;
        }

        private AudioSource AcquireSfxSource()
        {
            if(_sfxPool.Count > 0)
            {
                AudioSource s = _sfxPool[_sfxPool.Count - 1];
                _sfxPool.RemoveAt(_sfxPool.Count - 1);
                return s;
            }

            int max = _config ? _config.sfxPoolMaxSize : 32;
            if(_activeSfx.Count + _sfxPool.Count < max)
            {
                AudioSource s = CreateSource("CF_SFX");
                s.playOnAwake = false;
                s.loop = false;
                return s;
            }

            return null;
        }

        #region Command Handlers

        [CommandHandler]
        private async UniTask OnPlayMusic(AudioCommands.PlayMusic cmd, CancellationToken token)
        {
            try
            {
                if(string.IsNullOrEmpty(cmd.ClipKey)) return;
                if(_config != null && _config.preventReplaySame && _currentMusicKey == cmd.ClipKey &&
                   !cmd.AllowReplaySame)
                {
                    _logger?.LogDebug($"PlayMusic 忽略重复: {cmd.ClipKey}");
                    return;
                }

                // 通过 AssetsSystemModule 异步加载音频
                AudioClip clip = await CF.Query<AssetsQueries.Asset, AudioClip>(new AssetsQueries.Asset(cmd.ClipKey));
                if(clip == null)
                {
                    CF.Broadcast(new AudioBroadcasts.AudioError("LoadFail", "AudioClip is null", cmd.ClipKey)).Forget();
                    return;
                }

                AudioSource next = _usingA ? _musicB : _musicA;
                AudioSource prev = _usingA ? _musicA : _musicB;

                // 准备下一首（淡入起点0），记录 key 用于释放
                next.clip = clip;
                next.loop = cmd.Loop;
                next.volume = ApplyMasterMusic(0f);
                next.Play();
                if(next == _musicA) _musicAKey = cmd.ClipKey;
                else _musicBKey = cmd.ClipKey;

                // 触发上一首淡出（若存在）
                string prevKeyForBroadcast = null;
                if(prev && prev.isPlaying && prev.clip)
                {
                    prevKeyForBroadcast = prev.clip.name;
                    _fadingOutSource = prev;
                    _fadingOutKey = prev == _musicA ? _musicAKey : _musicBKey;
                    // 移除 A/B 槽上的 key，避免重复释放
                    if(prev == _musicA) _musicAKey = null;
                    else _musicBKey = null;
                    _prevFadeDuration = Mathf.Max(0f, cmd.FadeOutSeconds);
                    _prevFadeTimeLeft = _prevFadeDuration;
                    // 将当前有效体感音量还原到逻辑音量作为淡出起点
                    float factor = CurrentMusicFactor();
                    _prevStartVolume = factor > 0f ? Mathf.Clamp01(prev.volume / factor) : 0f;
                }

                _currentMusicKey = cmd.ClipKey;
                _usingA = !_usingA;

                // 配置新曲淡入参数
                _musicStartVolume = 0f;
                _musicTargetVolume = Mathf.Clamp01(cmd.Volume);
                _musicFadeDuration = Mathf.Max(0f, cmd.FadeInSeconds);
                _musicFadeTimeLeft = _musicFadeDuration;

                string method = prevKeyForBroadcast != null && _prevFadeDuration > 0f && _musicFadeDuration > 0f
                    ? "crossfade"
                    : _musicFadeDuration > 0f
                        ? "fade"
                        : "direct";
                CF.Broadcast(new AudioBroadcasts.MusicChanged(prevKeyForBroadcast, cmd.ClipKey, method)).Forget();
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
                CF.Broadcast(new AudioBroadcasts.AudioError("PlayMusic", e.Message, cmd.ClipKey)).Forget();
            }
        }

        [CommandHandler]
        private UniTask OnStopMusic(AudioCommands.StopMusic cmd, CancellationToken ct)
        {
            try
            {
                AudioSource src = _usingA ? _musicA : _musicB;
                if(!src || !src.isPlaying)
                {
                    return UniTask.CompletedTask;
                }

                float fadeOut = Mathf.Max(0f, cmd.FadeOutSeconds);
                if(fadeOut <= 0f)
                {
                    string toReleaseKey = src == _musicA ? _musicAKey : _musicBKey;
                    if(!string.IsNullOrEmpty(toReleaseKey))
                    {
                        CF.Execute(new AssetsCommands.ReleaseAsset<AudioClip>(toReleaseKey));
                        if(src == _musicA) _musicAKey = null;
                        else _musicBKey = null;
                    }

                    src.Stop();
                    src.clip = null;
                    src.volume = 0f;
                    _currentMusicKey = null;
                    return UniTask.CompletedTask;
                }

                // 使用淡出，当前源作为 _fadingOutSource，且不切换使用指针
                _fadingOutSource = src;
                _fadingOutKey = src == _musicA ? _musicAKey : _musicBKey;
                _prevFadeDuration = fadeOut;
                _prevFadeTimeLeft = fadeOut;
                float factor = CurrentMusicFactor();
                _prevStartVolume = factor > 0f ? Mathf.Clamp01(src.volume / factor) : 0f;

                // 清空当前曲目标，停止淡入驱动
                _currentMusicKey = null;
                _musicFadeTimeLeft = 0f;
                _musicFadeDuration = 0f;
                return UniTask.CompletedTask;
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
                CF.Broadcast(new AudioBroadcasts.AudioError("StopMusic", e.Message, string.Empty));
                return UniTask.CompletedTask;
            }
        }

        [CommandHandler]
        private async UniTask OnPlaySfx(AudioCommands.PlaySfx cmd, CancellationToken token)
        {
            try
            {
                if(string.IsNullOrEmpty(cmd.ClipKey)) return;
                AudioClip clip = await CF.Query<AssetsQueries.Asset, AudioClip>(new AssetsQueries.Asset(cmd.ClipKey));
                if(!clip)
                {
                    CF.Broadcast(new AudioBroadcasts.AudioError("LoadFail", "AudioClip is null", cmd.ClipKey)).Forget();
                    return;
                }

                AudioSource src = AcquireSfxSource();
                if(!src)
                {
                    CF.Broadcast(new AudioBroadcasts.AudioError("SfxPoolFull", "No available AudioSource",
                        cmd.ClipKey)).Forget();
                    return;
                }

                src.clip = clip;
                src.loop = cmd.Loop;
                src.pitch = Mathf.Clamp(cmd.Pitch, 0.1f, 3f);
                src.volume = Mathf.Clamp01(cmd.Volume);
                src.spatialBlend = 0f; // 2D
                src.Play();
                _activeSfx.Add(src);
                _sfxKeys[src] = cmd.ClipKey;
                CF.Broadcast(new AudioBroadcasts.SfxPlayed(cmd.ClipKey, src.volume)).Forget();
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
                CF.Broadcast(new AudioBroadcasts.AudioError("PlaySfx", e.Message, cmd.ClipKey)).Forget();
            }
        }

        [CommandHandler]
        private UniTask OnStopAllSfx(AudioCommands.StopAllSfx _, CancellationToken ct)
        {
            try
            {
                for(int i = _activeSfx.Count - 1; i >= 0; i--)
                {
                    AudioSource s = _activeSfx[i];
                    if(s)
                    {
                        if(_sfxKeys.TryGetValue(s, out string k))
                        {
                            _sfxKeys.Remove(s);
                            // 更新并发计数
                            if(_clipActiveCounts.TryGetValue(k, out int cnt))
                            {
                                cnt = Mathf.Max(0, cnt - 1);
                                if(cnt == 0)
                                {
                                    _clipActiveCounts.Remove(k);
                                }
                                else
                                {
                                    _clipActiveCounts[k] = cnt;
                                }
                            }

                            CF.Execute(new AssetsCommands.ReleaseAsset<AudioClip>(k));
                        }

                        _sfxPriorities.Remove(s);
                        s.Stop();
                        s.clip = null;
                        s.volume = 0f;
                        _sfxPool.Add(s);
                    }
                }

                _activeSfx.Clear();
                return UniTask.CompletedTask;
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
                CF.Broadcast(new AudioBroadcasts.AudioError("StopAllSfx", e.Message, string.Empty));
                return UniTask.CompletedTask;
            }
        }

        [CommandHandler]
        private UniTask OnSetVolume(AudioCommands.SetVolume cmd, CancellationToken ct)
        {
            try
            {
                switch(cmd.Category)
                {
                    case AudioCategory.Master: _masterVolume = Mathf.Clamp01(cmd.Volume); break;
                    case AudioCategory.Music: _musicVolume = Mathf.Clamp01(cmd.Volume); break;
                    case AudioCategory.Sfx: _sfxVolume = Mathf.Clamp01(cmd.Volume); break;
                }

                CF.Broadcast(new AudioBroadcasts.VolumeChanged(cmd.Category, cmd.Volume));

                // 立即应用到当前音乐源
                AudioSource src = _usingA ? _musicA : _musicB;
                if(src) src.volume = ApplyMasterMusic(src.volume);
                if(_fadingOutSource) _fadingOutSource.volume = ApplyMasterMusic(_fadingOutSource.volume);

                PersistVolumes();
                return UniTask.CompletedTask;
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
                CF.Broadcast(new AudioBroadcasts.AudioError("SetVolume", e.Message, cmd.Category.ToString()));
                return UniTask.CompletedTask;
            }
        }


        [CommandHandler]
        private UniTask OnSetMute(AudioCommands.SetMute cmd, CancellationToken ct)
        {
            try
            {
                switch(cmd.Category)
                {
                    case AudioCategory.Master: _masterMute = cmd.Mute; break;
                    case AudioCategory.Music: _musicMute = cmd.Mute; break;
                    case AudioCategory.Sfx: _sfxMute = cmd.Mute; break;
                }

                CF.Broadcast(new AudioBroadcasts.MuteChanged(cmd.Category, cmd.Mute));

                // 立即应用
                AudioSource src = _usingA ? _musicA : _musicB;
                if(src) src.volume = ApplyMasterMusic(src.volume);
                foreach (AudioSource s in _activeSfx)
                {
                    if(s)
                        s.volume = ApplyMasterSfx(s.volume);
                }
                if(_fadingOutSource) _fadingOutSource.volume = ApplyMasterMusic(_fadingOutSource.volume);

                PersistMutes();
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
                CF.Broadcast(new AudioBroadcasts.AudioError("SetMute", e.Message, cmd.Category.ToString()));
            }

            return UniTask.CompletedTask;
        }

        #endregion

        #region Queries

        [QueryHandler]
        private UniTask<float> OnGetVolume(AudioQueries.Volume q, CancellationToken ct)
        {
            float result = q.Category switch
            {
                AudioCategory.Master => _masterVolume,
                AudioCategory.Music => _musicVolume,
                AudioCategory.Sfx => _sfxVolume,
                _ => 1f
            };
            return UniTask.FromResult(result);
        }

        [QueryHandler]
        private UniTask<bool> OnGetMute(AudioQueries.Mute q, CancellationToken ct)
        {
            bool result = q.Category switch
            {
                AudioCategory.Master => _masterMute,
                AudioCategory.Music => _musicMute,
                AudioCategory.Sfx => _sfxMute,
                _ => false
            };
            return UniTask.FromResult(result);
        }

        public readonly struct ActiveMusicInfo
        {
            public readonly string Key;
            public readonly bool IsPlaying;

            public ActiveMusicInfo(string key, bool isPlaying)
            {
                Key = key;
                IsPlaying = isPlaying;
            }
        }

        [QueryHandler]
        private UniTask<ActiveMusicInfo> OnGetActiveMusic(AudioQueries.GetActiveMusic _, CancellationToken ct)
        {
            AudioSource src = _usingA ? _musicA : _musicB;
            return UniTask.FromResult(new ActiveMusicInfo(_currentMusicKey, src && src.isPlaying));
        }

        #endregion
    }
}