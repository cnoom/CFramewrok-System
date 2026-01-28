using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Decorators
{
    /// <summary>
    ///     反转节点结果：Success 与 Failure 对调，Running 状态保持不变。
    /// </summary>
    public sealed class InverterDecorator : IBehaviorTreeDecorator, IBehaviorTreeResultModifier
    {
        public string Name => "Inverter";

        public bool ShouldExecute(TreeNode node, BehaviorTreeContext context)
        {
            return true;
        }

        public void OnNodeFinished(TreeNode node, BehaviorTreeContext context, ENodeState result)
        {
        }

        public void OnNodeAborted(TreeNode node, BehaviorTreeContext context)
        {
        }

        public ENodeState ModifyResult(TreeNode node, BehaviorTreeContext context, float deltaTime, ENodeState currentState)
        {
            return currentState switch
            {
                ENodeState.Success => ENodeState.Failure,
                ENodeState.Failure => ENodeState.Success,
                _ => currentState
            };
        }
    }
}