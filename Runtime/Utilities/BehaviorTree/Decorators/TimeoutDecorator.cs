using System;
using System.Collections.Generic;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Decorators
{
    /// <summary>
    ///     限制节点运行时长，超过阈值则强制返回 Failure。
    /// </summary>
    public sealed class TimeoutDecorator : IBehaviorTreeDecorator, IBehaviorTreeResultModifier
    {
        private readonly Dictionary<string, float> _elapsedTimeByNode = new Dictionary<string, float>();
        private readonly float _timeoutSeconds;

        public TimeoutDecorator(float timeoutSeconds)
        {
            if(timeoutSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
            }

            _timeoutSeconds = timeoutSeconds;
        }


        public string Name => "Timeout";

        public bool ShouldExecute(TreeNode node, BehaviorTreeContext context)
        {
            return true;
        }

        public void OnNodeFinished(TreeNode node, BehaviorTreeContext context, ENodeState result)
        {
            _elapsedTimeByNode.Remove(node.Guid);
        }

        public void OnNodeAborted(TreeNode node, BehaviorTreeContext context)
        {
            _elapsedTimeByNode.Remove(node.Guid);
        }

        public ENodeState ModifyResult(TreeNode node, BehaviorTreeContext context, float deltaTime, ENodeState currentState)
        {
            if(currentState == ENodeState.Running)
            {
                float elapsed = _elapsedTimeByNode.TryGetValue(node.Guid, out float stored) ? stored : 0f;
                elapsed += deltaTime;

                if(elapsed >= _timeoutSeconds)
                {
                    _elapsedTimeByNode[node.Guid] = 0f;
                    return ENodeState.Failure;
                }

                _elapsedTimeByNode[node.Guid] = elapsed;
                return ENodeState.Running;
            }

            _elapsedTimeByNode.Remove(node.Guid);
            return currentState;

        }
    }
}