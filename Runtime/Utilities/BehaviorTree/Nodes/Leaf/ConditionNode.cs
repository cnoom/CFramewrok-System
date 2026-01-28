using System;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Nodes.Leaf
{
    public sealed class ConditionNode : TreeNode
    {
        private readonly Func<BehaviorTreeContext, bool> _predicate;

        public ConditionNode(Func<BehaviorTreeContext, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            DisplayName = "Condition";
        }

        protected override void OnInitialize(BehaviorTreeContext context)
        {
        }

        protected override ENodeState OnTick(BehaviorTreeContext context, float deltaTime)
        {
            return _predicate(context) ? ENodeState.Success : ENodeState.Failure;
        }

        protected override void OnTerminate(BehaviorTreeContext context, ENodeState result)
        {
        }
    }
}