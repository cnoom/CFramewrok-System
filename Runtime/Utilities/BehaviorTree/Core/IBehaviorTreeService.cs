using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Core
{
    public interface IBehaviorTreeService
    {
        float UpdateInterval { get; }
        void OnAttached(TreeNode node, BehaviorTreeContext context);
        void TickService(TreeNode node, BehaviorTreeContext context, float deltaTime);
        void OnDetached(TreeNode node, BehaviorTreeContext context);
    }
}