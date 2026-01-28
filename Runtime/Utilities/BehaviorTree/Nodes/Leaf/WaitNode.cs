using System;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Nodes.Leaf
{
    /// <summary>
    ///     等待指定时长后返回成功，可选随机波动用于减少同步化行为。
    /// </summary>
    public sealed class WaitNode : TreeNode
    {
        private readonly float _baseDuration;
        private readonly float _randomVariance;
        private float _elapsedTime;

        private float _targetDuration;

        public WaitNode(float durationSeconds, float randomVarianceSeconds = 0f)
        {
            if(durationSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            if(randomVarianceSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(randomVarianceSeconds));
            }

            _baseDuration = durationSeconds;
            _randomVariance = randomVarianceSeconds;
            DisplayName = "Wait";
        }

        protected override void OnInitialize(BehaviorTreeContext context)
        {
            _elapsedTime = 0f;
            _targetDuration = _baseDuration;

            if(_randomVariance > 0f)
            {
                var variance = (float)(context.Random.NextDouble() * _randomVariance);
                _targetDuration = Math.Max(0f, _baseDuration + variance);
            }
        }

        protected override ENodeState OnTick(BehaviorTreeContext context, float deltaTime)
        {
            _elapsedTime += deltaTime;
            return _elapsedTime >= _targetDuration ? ENodeState.Success : ENodeState.Running;
        }

        protected override void OnTerminate(BehaviorTreeContext context, ENodeState result)
        {
        }
    }
}