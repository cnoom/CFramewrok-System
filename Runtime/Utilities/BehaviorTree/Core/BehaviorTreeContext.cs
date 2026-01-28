using System;
using System.Threading;
using CFramework.BehaviorTree.Core;
using CFramework.Data;

namespace CFramework.BehaviorTree.Decorators.Core
{
    /// <summary>
    ///     行为树运行时上下文，对黑板、调试器、随机数及时间信息进行统一封装。
    /// </summary>
    public sealed class BehaviorTreeContext
    {

        internal BehaviorTreeContext(
            Blackboard globalBlackboard,
            Blackboard treeBlackboard,
            Blackboard instanceBlackboard,
            IBehaviorTreeDebugger debugger,
            CancellationToken cancellationToken,
            Random random)
        {
            GlobalBlackboard = globalBlackboard ?? throw new ArgumentNullException(nameof(globalBlackboard));
            TreeBlackboard = treeBlackboard ?? throw new ArgumentNullException(nameof(treeBlackboard));
            InstanceBlackboard = instanceBlackboard ?? throw new ArgumentNullException(nameof(instanceBlackboard));
            Debugger = debugger;
            CancellationToken = cancellationToken;
            Random = random ?? new Random();
        }
        public Blackboard GlobalBlackboard { get; }
        public Blackboard TreeBlackboard { get; }
        public Blackboard InstanceBlackboard { get; }
        public IBehaviorTreeDebugger Debugger { get; }
        public CancellationToken CancellationToken { get; }
        public Random Random { get; }
        public float DeltaTime { get; private set; }
        public float ElapsedTime { get; private set; }

        internal void BeginTick(float deltaTime)
        {
            DeltaTime = deltaTime;
            ElapsedTime += deltaTime;
        }

        public sealed class Builder
        {
            private CancellationToken _cancellationToken;
            private IBehaviorTreeDebugger _debugger;
            private Blackboard _globalBlackboard;
            private Blackboard _instanceBlackboard;
            private Random _random;
            private Blackboard _treeBlackboard;

            public Builder WithGlobalBlackboard(Blackboard blackboard)
            {
                _globalBlackboard = blackboard;
                return this;
            }

            public Builder WithTreeBlackboard(Blackboard blackboard)
            {
                _treeBlackboard = blackboard;
                return this;
            }

            public Builder WithInstanceBlackboard(Blackboard blackboard)
            {
                _instanceBlackboard = blackboard;
                return this;
            }

            public Builder WithDebugger(IBehaviorTreeDebugger debugger)
            {
                _debugger = debugger;
                return this;
            }

            public Builder WithCancellationToken(CancellationToken cancellationToken)
            {
                _cancellationToken = cancellationToken;
                return this;
            }

            public Builder WithRandom(Random random)
            {
                _random = random;
                return this;
            }

            public BehaviorTreeContext Build()
            {
                return new BehaviorTreeContext(
                    _globalBlackboard ?? new Blackboard(),
                    _treeBlackboard ?? new Blackboard(),
                    _instanceBlackboard ?? new Blackboard(),
                    _debugger,
                    _cancellationToken,
                    _random ?? new Random());
            }
        }
    }
}