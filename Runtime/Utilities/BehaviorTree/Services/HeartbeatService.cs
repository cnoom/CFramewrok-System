using System;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Services
{
    public sealed class HeartbeatService : IBehaviorTreeService
    {
        private readonly Action<TreeNode, BehaviorTreeContext> _heartbeatAction;

        public HeartbeatService(float intervalSeconds, Action<TreeNode, BehaviorTreeContext> heartbeatAction)
        {
            UpdateInterval = intervalSeconds;
            _heartbeatAction = heartbeatAction;
        }

        public float UpdateInterval { get; }

        public void OnAttached(TreeNode node, BehaviorTreeContext context)
        {
        }

        public void TickService(TreeNode node, BehaviorTreeContext context, float deltaTime)
        {
            _heartbeatAction?.Invoke(node, context);
        }

        public void OnDetached(TreeNode node, BehaviorTreeContext context)
        {
        }
    }
}