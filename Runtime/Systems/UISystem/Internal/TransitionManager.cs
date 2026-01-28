using System;
using CFramework.Core.Log;
using CFramework.Systems.UISystem.Transitions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CFramework.Systems.UISystem.Internal
{
    /// <summary>
    ///     过渡动画管理器实现
    /// </summary>
    internal sealed class TransitionManager : ITransitionManager
    {
        private readonly CFLogger _logger;

        public TransitionManager(CFLogger logger)
        {
            _logger = logger;
        }

        public async UniTask PlayInAsync(GameObject root, string transitionName, float seconds)
        {
            if(!root) return;

            try
            {
                IUITransition transition = await TransitionsFactory.GetAsync(transitionName);
                await transition.PlayInAsync(root, seconds);
                TransitionsFactory.Release(transitionName);
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
            }
        }

        public async UniTask PlayOutAsync(GameObject root, string transitionName, float seconds)
        {
            if(!root) return;

            try
            {
                IUITransition transition = await TransitionsFactory.GetAsync(transitionName);
                if(!root) return;
                await transition.PlayOutAsync(root, seconds);
                TransitionsFactory.Release(transitionName);
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
            }
        }
    }
}