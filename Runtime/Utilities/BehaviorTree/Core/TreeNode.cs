using System;
using System.Collections.Generic;
using CFramework.BehaviorTree.Decorators.Core;

namespace CFramework.BehaviorTree.Core
{
    [Serializable]
    public abstract class TreeNode
    {
        private readonly List<TreeNode> _children = new List<TreeNode>();
        private readonly List<IBehaviorTreeDecorator> _decorators = new List<IBehaviorTreeDecorator>();
        private readonly List<IBehaviorTreeService> _services = new List<IBehaviorTreeService>();
        private readonly Dictionary<IBehaviorTreeService, float> _serviceTimers = new Dictionary<IBehaviorTreeService, float>();

        private bool _isInitialized;
        private float _timeInState;

        protected TreeNode()
        {
            Guid = System.Guid.NewGuid().ToString("N");
        }

        public string Guid { get; private set; }
        public string DisplayName { get; set; }
        public IReadOnlyList<TreeNode> Children => _children;
        public IReadOnlyList<IBehaviorTreeDecorator> Decorators => _decorators;
        public IReadOnlyList<IBehaviorTreeService> Services => _services;

        public void AddChild(TreeNode child)
        {
            if(child == null) throw new ArgumentNullException(nameof(child));
            _children.Add(child);
        }

        public void AddDecorator(IBehaviorTreeDecorator decorator)
        {
            if(decorator == null) throw new ArgumentNullException(nameof(decorator));
            _decorators.Add(decorator);
        }

        public void AddService(IBehaviorTreeService service)
        {
            if(service == null) throw new ArgumentNullException(nameof(service));
            _services.Add(service);
        }

        internal ENodeState Execute(BehaviorTreeContext context, float deltaTime)
        {
            if(!EvaluateDecorators(context))
            {
                Abort(context);
                return ENodeState.Failure;
            }

            if(!_isInitialized)
            {
                _isInitialized = true;
                _timeInState = 0f;
                context.Debugger?.OnNodeEnter(this, context);
                OnInitialize(context);
                AttachServices(context);
            }

            TickServices(context, deltaTime);
            ENodeState result = OnTick(context, deltaTime);
            result = ApplyResultModifiers(context, deltaTime, result);
            _timeInState += deltaTime;

            if(result != ENodeState.Running)

            {
                OnTerminate(context, result);
                CleanupAfterExit(context, result);
            }

            return result;
        }

        public void Abort(BehaviorTreeContext context)
        {
            if(!_isInitialized)
            {
                return;
            }

            foreach (TreeNode child in _children)
            {
                child.Abort(context);
            }

            foreach (IBehaviorTreeDecorator decorator in _decorators)
            {
                decorator.OnNodeAborted(this, context);
            }

            CleanupAfterExit(context, ENodeState.Failure);
            context.Debugger?.OnNodeAborted(this, context);
        }

        protected abstract void OnInitialize(BehaviorTreeContext context);
        protected abstract ENodeState OnTick(BehaviorTreeContext context, float deltaTime);
        protected abstract void OnTerminate(BehaviorTreeContext context, ENodeState result);

        private ENodeState ApplyResultModifiers(BehaviorTreeContext context, float deltaTime, ENodeState currentState)
        {
            ENodeState modifiedState = currentState;
            foreach (IBehaviorTreeDecorator decorator in _decorators)
            {
                if(decorator is IBehaviorTreeResultModifier resultModifier)
                {
                    modifiedState = resultModifier.ModifyResult(this, context, deltaTime, modifiedState);
                }
            }

            return modifiedState;
        }

        private bool EvaluateDecorators(BehaviorTreeContext context)
        {
            foreach (IBehaviorTreeDecorator decorator in _decorators)

            {
                if(!decorator.ShouldExecute(this, context))
                {
                    return false;
                }
            }

            return true;
        }

        private void TickServices(BehaviorTreeContext context, float deltaTime)
        {
            foreach (IBehaviorTreeService service in _services)
            {
                if(!_serviceTimers.TryGetValue(service, out float elapsed))
                {
                    elapsed = 0f;
                    _serviceTimers[service] = 0f;
                }

                elapsed += deltaTime;
                if(elapsed >= Math.Max(service.UpdateInterval, float.Epsilon))
                {
                    service.TickService(this, context, deltaTime);
                    elapsed = 0f;
                }

                _serviceTimers[service] = elapsed;
            }
        }

        private void AttachServices(BehaviorTreeContext context)
        {
            foreach (IBehaviorTreeService service in _services)
            {
                service.OnAttached(this, context);
                _serviceTimers[service] = 0f;
            }
        }

        private void DetachServices(BehaviorTreeContext context)
        {
            foreach (IBehaviorTreeService service in _services)
            {
                service.OnDetached(this, context);
            }

            _serviceTimers.Clear();
        }

        private void CleanupAfterExit(BehaviorTreeContext context, ENodeState result)
        {
            DetachServices(context);
            _isInitialized = false;
            context.Debugger?.OnNodeExit(this, context, result, _timeInState);
            _timeInState = 0f;

            foreach (IBehaviorTreeDecorator decorator in _decorators)
            {
                decorator.OnNodeFinished(this, context, result);
            }
        }
    }
}