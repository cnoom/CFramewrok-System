using System;
using System.Collections.Generic;
using CFramework.BehaviorTree.Core;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Nodes.Composites
{
    public sealed class ParallelNode : CompositeNode
    {
        public ParallelPolicy SuccessPolicy { get; set; } = ParallelPolicy.Any;
        public ParallelPolicy FailurePolicy { get; set; } = ParallelPolicy.Any;
        public int SuccessThreshold { get; set; } = 1;
        public int FailureThreshold { get; set; } = 1;

        protected override void OnCompositeInitialize(BehaviorTreeContext context)
        {
        }

        protected override ENodeState OnCompositeTick(BehaviorTreeContext context, float deltaTime, ref int currentIndex)
        {
            IReadOnlyList<TreeNode> children = GetChildren();
            if(children.Count == 0)
            {
                return ENodeState.Success;
            }

            var successCount = 0;
            var failureCount = 0;
            var runningCount = 0;

            foreach (TreeNode child in children)
            {
                ENodeState state = child.Execute(context, deltaTime);
                switch(state)
                {
                    case ENodeState.Success:
                        successCount++;
                        break;
                    case ENodeState.Failure:
                        failureCount++;
                        break;
                    default:
                        runningCount++;
                        break;
                }
            }

            if(EvaluatePolicy(SuccessPolicy, SuccessThreshold, successCount, children.Count))
            {
                return ENodeState.Success;
            }

            if(EvaluatePolicy(FailurePolicy, FailureThreshold, failureCount, children.Count))
            {
                return ENodeState.Failure;
            }

            return runningCount > 0 ? ENodeState.Running : ENodeState.Failure;
        }

        protected override void OnCompositeTerminate(BehaviorTreeContext context, ENodeState result)
        {
        }

        private static bool EvaluatePolicy(ParallelPolicy policy, int threshold, int achieved, int total)
        {
            return policy switch
            {
                ParallelPolicy.All => achieved == total,
                ParallelPolicy.Threshold => achieved >= Math.Max(1, threshold),
                _ => achieved > 0
            };
        }
    }
}