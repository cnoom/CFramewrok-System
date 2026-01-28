using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Core
{
    public interface IBehaviorTreeDecorator
    {
        string Name { get; }
        bool ShouldExecute(TreeNode node, BehaviorTreeContext context);
        void OnNodeFinished(TreeNode node, BehaviorTreeContext context, ENodeState result);
        void OnNodeAborted(TreeNode node, BehaviorTreeContext context);
    }
}