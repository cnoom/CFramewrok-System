using CFramework.Data;

namespace CFramework.FSM
{
    public interface IState
    {
        void Enter(Blackboard blackboard); // 进入状态
        void Update(Blackboard blackboard); // 执行状态逻辑
        void Exit(Blackboard blackboard); // 退出状态
    }
}