using System;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Nodes.Leaf
{
    public sealed class ActionNode : TreeNode
    {
        private readonly Action<BehaviorTreeContext> _onInitialize;
        private readonly Action<BehaviorTreeContext, ENodeState> _onTerminate;
        private readonly Func<BehaviorTreeContext, float, ENodeState> _onTick;

        public ActionNode(
            Func<BehaviorTreeContext, float, ENodeState> onTick,
            Action<BehaviorTreeContext> onInitialize = null,
            Action<BehaviorTreeContext, ENodeState> onTerminate = null)
        {
            _onTick = onTick ?? throw new ArgumentNullException(nameof(onTick));
            _onInitialize = onInitialize;
            _onTerminate = onTerminate;
            DisplayName = "Action";
        }

        protected override void OnInitialize(BehaviorTreeContext context)
        {
            _onInitialize?.Invoke(context);
        }

        protected override ENodeState OnTick(BehaviorTreeContext context, float deltaTime)
        {
            return _onTick(context, deltaTime);
        }

        protected override void OnTerminate(BehaviorTreeContext context, ENodeState result)
        {
            _onTerminate?.Invoke(context, result);
        }
    }
}