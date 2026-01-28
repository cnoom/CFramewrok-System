using CFramework.Core.CommandSystem;

namespace CFramework.Systems.SaveSystem
{
    public static class SaveCommands
    {
        /// <summary>
        ///     设置当前的存档。
        /// </summary>
        public struct SetCurrentProfile : ICommandData
        {
            /// <summary>存档的 ID。</summary>
            public string ProfileId;

            /// <param name="profileId">存档的 ID。</param>
            public SetCurrentProfile(string profileId)
            {
                ProfileId = profileId;
            }
        }


        /// <summary>
        ///     删除指定的存档。
        /// </summary>
        public struct DeleteProfile : ICommandData
        {
            /// <summary>存档的 ID。</summary>
            public string ProfileId;

            /// <param name="profileId">存档的 ID。</param>
            public DeleteProfile(string profileId)
            {
                ProfileId = profileId;
            }
        }

        /// <summary>
        ///     删除所有存档（所有存档档案）。
        /// </summary>
        public struct DeleteAllProfiles : ICommandData
        {
        }


        /// <summary>
        ///     保存所有脏数据
        /// </summary>
        public struct SaveAllDirty : ICommandData
        {
        }

        /// <summary>
        ///     保存存档的指定 Id 的槽位。
        /// </summary>
        public struct SaveSlot : ICommandData
        {
            /// <summary>槽位的 ID。</summary>
            public string SlotId;

            /// <param name="slotId">槽位的 ID。</param>
            public SaveSlot(string slotId)
            {
                SlotId = slotId;
            }
        }


        /// <summary>
        ///     删除存档的指定 Id 的槽位。
        /// </summary>
        public struct DeleteSlot : ICommandData
        {
            /// <summary>槽位的 ID。</summary>
            public string SlotId;

            /// <param name="slotId">槽位的 ID。</param>
            public DeleteSlot(string slotId)
            {
                SlotId = slotId;
            }
        }
    }
}