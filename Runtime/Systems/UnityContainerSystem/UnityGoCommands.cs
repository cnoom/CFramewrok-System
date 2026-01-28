using CFramework.Core.CommandSystem;
using JetBrains.Annotations;

namespace CFramework.Systems.UnityContainerSystem
{
    public static class UnityGoCommands
    {
        public readonly struct RegisterGoBind : ICommandData
        {
            public readonly GoBind GoBind;
            public RegisterGoBind([NotNull] GoBind goBind)
            {
                GoBind = goBind;
            }
        }

        public readonly struct UnregisterGoBind : ICommandData
        {
            public readonly string Scope;
            public readonly string GoName;
            public UnregisterGoBind([NotNull] string goName, [NotNull] string scope)
            {
                (GoName, Scope) = (goName, scope);
            }
            public UnregisterGoBind([NotNull] string goName)
            {
                (GoName, Scope) = (goName, string.Empty);
            }
        }

        public readonly struct RegisterGosBind : ICommandData
        {
            public readonly GoGroupBind GoGroupBind;
            public RegisterGosBind([NotNull] GoGroupBind goGroupBind)
            {
                GoGroupBind = goGroupBind;
            }
        }

        public readonly struct UnregisterGosBind : ICommandData
        {
            public readonly string GroupName;
            public UnregisterGosBind([NotNull] string groupName)
            {
                GroupName = groupName;
            }
        }
    }
}