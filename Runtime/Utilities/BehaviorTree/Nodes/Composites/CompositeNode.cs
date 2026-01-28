using System.Collections.Generic;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Nodes.Composites
{
    public abstract class CompositeNode : TreeNode
    {
        private int _currentChildIndex;

        protected IReadOnlyList<TreeNode> GetChildren()
        {
            return Children;
        }

        protected override void OnInitialize(BehaviorTreeContext context)
        {
            _currentChildIndex = 0;
            OnCompositeInitialize(context);
        }

        protected override ENodeState OnTick(BehaviorTreeContext context, float deltaTime)
        {
            return OnCompositeTick(context, deltaTime, ref _currentChildIndex);
        }

        protected override void OnTerminate(BehaviorTreeContext context, ENodeState result)
        {
            OnCompositeTerminate(context, result);
            _currentChildIndex = 0;
        }

        protected abstract void OnCompositeInitialize(BehaviorTreeContext context);
        protected abstract ENodeState OnCompositeTick(BehaviorTreeContext context, float deltaTime, ref int currentIndex);
        protected abstract void OnCompositeTerminate(BehaviorTreeContext context, ENodeState result);
    }
}