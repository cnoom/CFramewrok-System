using CFramework.Core;
using CFramework.Systems.AssetsSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CFramework.Systems.UISystem.Transitions
{
    public static class TransitionsFactory
    {
        // 异步获取过渡：通过 AssetsSystem 以类名为 key 加载 ScriptableObject
        public static async UniTask<IUITransition> GetAsync(string name)
        {
            UITransition asset = await CF.Query(new AssetsQueries.Asset<UITransition>(name));
            if(!asset)
            {
                // 找不到则回退到内建实例（保证基础可用）
                switch(name)
                {
                    case nameof(FadeTransition): return ScriptableObject.CreateInstance<FadeTransition>();
                    case nameof(NoopTransition): return ScriptableObject.CreateInstance<NoopTransition>();
                    default: return ScriptableObject.CreateInstance<FadeTransition>();
                }
            }

            return asset;
        }

        // 使用完成后释放资源（引用计数归还）
        public static void Release(string name)
        {
            CF.Execute(new AssetsCommands.ReleaseAsset<UITransition>(name));
        }
    }
}