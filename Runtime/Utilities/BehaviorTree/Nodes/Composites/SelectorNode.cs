using System.Collections.Generic;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Nodes.Composites
{
    public sealed class SelectorNode : CompositeNode
    {
        protected override void OnCompositeInitialize(BehaviorTreeContext context)
        {
        }

        protected override ENodeState OnCompositeTick(BehaviorTreeContext context, float deltaTime, ref int currentIndex)
        {
            IReadOnlyList<TreeNode> children = GetChildren();
            if(children.Count == 0)
            {
                return ENodeState.Failure;
            }

            while (currentIndex < children.Count)
            {
                ENodeState childState = children[currentIndex].Execute(context, deltaTime);
                switch(childState)
                {
                    case ENodeState.Running:
                        return ENodeState.Running;
                    case ENodeState.Success:
                        return ENodeState.Success;
                    default:
                        currentIndex++;
                        break;
                }
            }

            return ENodeState.Failure;
        }

        protected override void OnCompositeTerminate(BehaviorTreeContext context, ENodeState result)
        {
        }
    }
}