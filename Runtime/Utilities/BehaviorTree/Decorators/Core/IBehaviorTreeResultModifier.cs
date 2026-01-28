using CFramework.BehaviorTree.Core;

namespace CFramework.BehaviorTree.Decorators.Core
{
    /// <summary>
    ///     提供节点结果二次加工的装饰器接口，可在节点返回状态前进行转换。
    /// </summary>
    public interface IBehaviorTreeResultModifier
    {
        ENodeState ModifyResult(TreeNode node, BehaviorTreeContext context, float deltaTime, ENodeState currentState);
    }
}