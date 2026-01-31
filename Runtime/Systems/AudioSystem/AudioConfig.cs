using CFramework.Attachment.com.cnoom.cframework.systems.Runtime.Attributes;
using CFramework.Core.Log;
using UnityEngine;

namespace CFramework.Systems.AudioSystem
{
    // 可作为 ScriptableObject，也允许通过构造函数传入POCO配置
    [SingleAddressAsset("AudioConfig", "Configs")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Logging")]
        public string logTag = "Audio";
        public bool enableLogs = true;
        public ICFLogger.Level logLevel = ICFLogger.Level.Info;

        [Header("Volumes (0-1)"), Range(0, 1)]
        public float masterVolume = 1f;
        [Range(0, 1)]
        public float musicVolume = 1f;
        [Range(0, 1)]
        public float sfxVolume = 1f;

        [Header("Mute")]
        public bool masterMute;
        public bool musicMute;
        public bool sfxMute;

        [Header("Music Defaults")]
        public float defaultFadeInSeconds = 0.2f;
        public float defaultFadeOutSeconds = 0.2f;
        public bool preventReplaySame = true;

        [Header("SFX Pool")]
        public int sfxPoolInitialSize = 8;
        public int sfxPoolMaxSize = 32;
        public int globalMaxSfxConcurrency = 32;
        public int maxInstancesPerClip = 8;
        [Tooltip("每个 Clip 的最小重触发间隔（毫秒），0 表示不限制")]
        public int minIntervalPerClipMs;

        [Header("Behavior")]
        public bool respectGlobalPause = true;

        // 可扩展：预加载列表、Ducking等
    }
}