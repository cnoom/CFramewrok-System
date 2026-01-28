using System;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Nodes.Composites
{
    /// <summary>
    ///     重复执行单一子节点，可配置重复次数以及在子节点失败时是否继续尝试。
    /// </summary>
    public sealed class RepeaterNode : TreeNode
    {
        private readonly bool _continueOnFailure;
        private readonly int _repeatCount;
        private int _completedIterations;

        public RepeaterNode(int repeatCount = -1, bool continueOnFailure = false)
        {
            if(repeatCount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(repeatCount));
            }

            _repeatCount = repeatCount;
            _continueOnFailure = continueOnFailure;
            DisplayName = repeatCount > 0 ? $"Repeat x{repeatCount}" : "Repeat Infinite";
        }

        protected override void OnInitialize(BehaviorTreeContext context)
        {
            _completedIterations = 0;
        }

        protected override ENodeState OnTick(BehaviorTreeContext context, float deltaTime)
        {
            if(Children.Count == 0)
            {
                return ENodeState.Failure;
            }

            TreeNode child = Children[0];
            ENodeState childState = child.Execute(context, deltaTime);

            if(childState == ENodeState.Running)
            {
                return ENodeState.Running;
            }

            if(childState == ENodeState.Failure && !_continueOnFailure)
            {
                return ENodeState.Failure;
            }

            _completedIterations++;

            if(_repeatCount > 0 && _completedIterations >= _repeatCount)
            {
                return ENodeState.Success;
            }

            return ENodeState.Running;
        }

        protected override void OnTerminate(BehaviorTreeContext context, ENodeState result)
        {
        }
    }
}