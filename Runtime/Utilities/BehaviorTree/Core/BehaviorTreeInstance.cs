using System;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Core
{
    public sealed class BehaviorTreeInstance
    {

        public BehaviorTreeInstance(TreeNode rootNode, BehaviorTreeContext context)
        {
            RootNode = rootNode ?? throw new ArgumentNullException(nameof(rootNode));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            IsActive = true;
        }

        public TreeNode RootNode { get; }

        public BehaviorTreeContext Context { get; }

        public bool IsActive { get; private set; }

        public ENodeState Tick(float deltaTime)
        {
            if(!IsActive)
            {
                return ENodeState.Failure;
            }

            Context.BeginTick(deltaTime);
            ENodeState result = RootNode.Execute(Context, deltaTime);
            if(result != ENodeState.Running)
            {
                IsActive = false;
                Context.Debugger?.OnTreeCompleted(this, result);
            }

            return result;
        }

        public void Restart()
        {
            RootNode.Abort(Context);
            IsActive = true;
            Context.Debugger?.OnTreeRestarted(this);
        }

        public void Abort()
        {
            if(!IsActive)
            {
                return;
            }

            RootNode.Abort(Context);
            IsActive = false;
            Context.Debugger?.OnTreeCompleted(this, ENodeState.Failure);
        }
    }
}