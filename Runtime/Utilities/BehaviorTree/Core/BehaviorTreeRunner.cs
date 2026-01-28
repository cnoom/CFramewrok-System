#if UNITY_5_3_OR_NEWER
using CFramework.BehaviorTree.Decorators.Core;
using CFramework.Data;
using UnityEngine;

namespace CFramework.BehaviorTree.Core
{
    [DisallowMultipleComponent]
    public sealed class BehaviorTreeRunner : MonoBehaviour
    {
        [SerializeReference]
        private TreeNode _rootNode;

        [SerializeField]
        private bool _autoStart = true;

        [SerializeField]
        private bool _restartOnComplete;
        private Blackboard _globalBlackboardOverride;

        private BehaviorTreeInstance _instance;



        private void Awake()
        {
            if(_autoStart)
            {
                StartTree();
            }
        }

        private void Update()
        {
            if(_instance == null)
            {
                return;
            }

            ENodeState result = _instance.Tick(Time.deltaTime);
            if(_restartOnComplete && result != ENodeState.Running)
            {
                _instance.Restart();
            }
        }

        private void OnDestroy()
        {
            _instance?.Abort();
            _instance = null;
        }

        public void SetRootNode(TreeNode rootNode)
        {
            _rootNode = rootNode;
        }

        public void SetGlobalBlackboard(Blackboard blackboard)
        {
            _globalBlackboardOverride = blackboard;
        }

        public void StartTree(BehaviorTreeContext context = null)

        {
            if(_rootNode == null)
            {
                Debug.LogWarning("BehaviorTreeRunner 未配置根节点");
                return;
            }

            // 清理旧实例，避免内存泄漏
            _instance?.Abort();

            BehaviorTreeContext finalContext = context ?? new BehaviorTreeContext.Builder()
                .WithGlobalBlackboard(_globalBlackboardOverride ?? new Blackboard())
                .WithTreeBlackboard(new Blackboard())
                .WithInstanceBlackboard(new Blackboard())
                .Build();

            _instance = new BehaviorTreeInstance(_rootNode, finalContext);
        }

        public void StopTree()
        {
            _instance?.Abort();
            _instance = null;
        }
    }
}
#endif