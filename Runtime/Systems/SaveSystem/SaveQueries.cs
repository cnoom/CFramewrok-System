using CFramework.Core.QuerySystem;
using CFramework.Systems.SaveSystem.Data;

namespace CFramework.Systems.SaveSystem
{
    public static class SaveQueries
    {
        /// <summary>
        ///     获取当前的存档的Key
        /// </summary>
        /// <returns>存档的Key</returns>
        public struct CurrentProfile : IQueryData<string>
        {
        }

        /// <summary>
        ///     获取当前的存档的指定槽位。
        /// </summary>
        /// <returns>存档的指定槽位。</returns>
        public struct CurrentProfileSlot : IQueryData<Slot>
        {
            /// <summary>存档的槽位 Id。</summary>
            public string SlotId;

            /// <param name="slotId">存档的槽位 Id。</param>
            public CurrentProfileSlot(string slotId)
            {
                SlotId = slotId;
            }
        }
    }

}