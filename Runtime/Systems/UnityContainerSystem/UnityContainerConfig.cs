using CFramework.Attachment.com.cnoom.cframework.systems.Runtime.Attributes;
using UnityEngine;

namespace CFramework.Systems.UnityContainerSystem
{
    /// <summary>
    ///     key 冲突时的处理策略
    /// </summary>
    public enum DuplicatePolicy
    {
        [InspectorName("保持第一个")]
        KeepFirst,

        [InspectorName("替换")]
        Replace,

        [InspectorName("抛出异常")]
        Throw,

        [InspectorName("仅警告")]
        Warn,

        [InspectorName("警告并替换")]
        WarnAndReplace
    }

    /// <summary>
    ///     自动注册时机
    /// </summary>
    public enum AutoRegisterTiming
    {
        [InspectorName("Awake 时注册")]
        Awake,

        [InspectorName("Start 时注册")]
        Start
    }

    /// <summary>
    ///     Unity 容器系统配置
    /// </summary>
    [CreateAssetMenu(fileName = "UnityContainerConfig", menuName = "CFramework/Unity Container Config")]
    [SingleAddressAsset("CF_UnityContainerConfig", "Default Local Group", "UnityContainerConfig")]
    public class UnityContainerConfig : ScriptableObject
    {
        [Tooltip("日志输出tag")]
        public string tag;

        [Tooltip("全局 key 冲突处理策略")]
        public DuplicatePolicy globalDuplicatePolicy = DuplicatePolicy.Warn;
    }
}