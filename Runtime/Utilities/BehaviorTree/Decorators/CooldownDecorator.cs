using System.Collections.Generic;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Decorators
{
    public sealed class CooldownDecorator : IBehaviorTreeDecorator
    {
        private readonly float _cooldownSeconds;
        private readonly Dictionary<string, float> _nextAvailableTime = new Dictionary<string, float>();

        public CooldownDecorator(float cooldownSeconds)
        {
            _cooldownSeconds = cooldownSeconds;
        }

        public string Name => "Cooldown";

        public bool ShouldExecute(TreeNode node, BehaviorTreeContext context)
        {
            if(!_nextAvailableTime.TryGetValue(node.Guid, out float nextTime))
            {
                return true;
            }

            return context.ElapsedTime >= nextTime;
        }

        public void OnNodeFinished(TreeNode node, BehaviorTreeContext context, ENodeState result)
        {
            if(result == ENodeState.Running)
            {
                return;
            }

            _nextAvailableTime[node.Guid] = context.ElapsedTime + _cooldownSeconds;
        }

        public void OnNodeAborted(TreeNode node, BehaviorTreeContext context)
        {
            _nextAvailableTime[node.Guid] = context.ElapsedTime + _cooldownSeconds;
        }
    }
}