using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Core
{
    public interface IBehaviorTreeDebugger
    {
        void OnNodeEnter(TreeNode node, BehaviorTreeContext context);
        void OnNodeExit(TreeNode node, BehaviorTreeContext context, ENodeState result, float durationSeconds);
        void OnNodeAborted(TreeNode node, BehaviorTreeContext context);
        void OnTreeCompleted(BehaviorTreeInstance instance, ENodeState result);
        void OnTreeRestarted(BehaviorTreeInstance instance);
    }
}