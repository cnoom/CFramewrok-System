using System;
using System.Collections.Generic;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Nodes.Composites
{
    /// <summary>
    ///     随机选择器，会在每次初始化时打乱子节点执行顺序。
    /// </summary>
    public sealed class RandomSelectorNode : CompositeNode
    {
        private readonly List<TreeNode> _shuffledChildren = new List<TreeNode>();

        protected override void OnCompositeInitialize(BehaviorTreeContext context)
        {
            _shuffledChildren.Clear();
            _shuffledChildren.AddRange(GetChildren());

            if(_shuffledChildren.Count <= 1)
            {
                return;
            }

            Random random = context.Random;
            for(int i = _shuffledChildren.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                (_shuffledChildren[i], _shuffledChildren[swapIndex]) = (_shuffledChildren[swapIndex], _shuffledChildren[i]);
            }
        }

        protected override ENodeState OnCompositeTick(BehaviorTreeContext context, float deltaTime, ref int currentIndex)
        {
            if(_shuffledChildren.Count == 0)
            {
                return ENodeState.Failure;
            }

            while (currentIndex < _shuffledChildren.Count)
            {
                ENodeState childState = _shuffledChildren[currentIndex].Execute(context, deltaTime);
                switch(childState)
                {
                    case ENodeState.Success:
                        return ENodeState.Success;
                    case ENodeState.Running:
                        return ENodeState.Running;
                    default:
                        currentIndex++;
                        break;
                }
            }

            return ENodeState.Failure;
        }

        protected override void OnCompositeTerminate(BehaviorTreeContext context, ENodeState result)
        {
            _shuffledChildren.Clear();
        }
    }
}