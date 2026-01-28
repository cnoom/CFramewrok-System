using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CFramework.Core;
using CFramework.Core.Attributes;
using CFramework.Core.Interfaces.LifeScope;
using CFramework.Core.Log;
using CFramework.Core.ModuleSystem;
using CFramework.Systems.SaveSystem.Data;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace CFramework.Systems.SaveSystem
{
    [AutoModule("c存档系统", "用于游戏数据的持久化反持久化")]
    public class SaveSystemModule : IModule, IRegisterAsync, IUnRegisterAsync, ISaveSystem
    {
        private const string DefaultProfileId = "save1";

        // 统一采用复合键："{profileId}:{slotId}"
        private readonly Dictionary<string, Slot> _keyToContainer = new Dictionary<string, Slot>();

        private string _currentProfileId = DefaultProfileId; // 主 profileId，默认值

        private CFLogger _logger;


        public async UniTask RegisterAsync(CancellationToken cancellationToken)
        {
            _logger = CF.CreateLogger(nameof(SaveSystemModule));
            await LoadAllSlotsForProfileAsync(_currentProfileId);
        }

        public async UniTask UnRegisterAsync(CancellationToken cancellationToken)
        {
            // 先保存，再清理缓存
            await SaveAllDirtyAsync(_currentProfileId, CF.CancellationToken);
            _keyToContainer.Clear();
        }


        #region 内部工具方法

        private static string MakeKey(string profileId, string slotId)
        {
            profileId = ValidateId(profileId);
            slotId = ValidateId(slotId);
            if(string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(slotId)) return null;
            return $"{profileId}:{slotId}";
        }

        private async UniTask<Slot> LoadAsync(string profileId, string slotId)
        {
            // 规范化并确保加入缓存（采用复合键）
            string key = MakeKey(profileId, slotId);
            if(string.IsNullOrEmpty(key)) return null;

            if(!_keyToContainer.TryGetValue(key, out Slot container))
            {
                container = new Slot();
                _keyToContainer[key] = container;
            }

            try
            {
                string path = GetSafePath(profileId, slotId);
                if(!File.Exists(path)) return container;
                byte[] bytes = await ReadAllBytesAsync(path);
                string json = Encoding.UTF8.GetString(bytes);
                SaveContainerDto dto = JsonConvert.DeserializeObject<SaveContainerDto>(json);
                if(dto != null) container.LoadFromDto(dto);
            }
            catch (IOException e)
            {
                // 读取异常时保留当前容器状态，避免崩溃
                _logger.LogException(e);
            }

            return container;
        }

        private async UniTask SaveAsync(string profileId, string slotId, CancellationToken cancelToken)
        {
            string key = MakeKey(profileId, slotId);
            if(string.IsNullOrEmpty(key)) return;
            if(!_keyToContainer.TryGetValue(key, out Slot container)) return;
            try
            {
                SaveContainerDto dto = container.ToDtoSnapshot();
                string json = JsonConvert.SerializeObject(dto);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                string path = GetSafePath(profileId, slotId);
                EnsureDirectory(Path.GetDirectoryName(path));
                await AtomicWriteAsync(path, bytes);
                container.Save();
            }
            catch (IOException e)
            {
                // 写入异常时保持 Dirty，等待下次再尝试
                _logger.LogException(e);
            }
        }

        // 保存指定 profileId 下所有脏容器
        private async UniTask SaveAllDirtyAsync(string profileId, CancellationToken cancelToken)
        {
            profileId = ValidateId(profileId);
            List<(string slotId, Slot container)> toSave = new List<(string slotId, Slot container)>();
            foreach (KeyValuePair<string, Slot> kv in _keyToContainer)
            {
                string key = kv.Key;
                int idx = key.IndexOf(':');
                if(idx <= 0) continue;
                string p = key.Substring(0, idx);
                string s = key.Substring(idx + 1);
                if(p == profileId && kv.Value != null && kv.Value.Dirty)
                {
                    toSave.Add((s, kv.Value));
                }
            }

            for(var i = 0; i < toSave.Count; i++)
            {
                (string slotId, Slot container) = toSave[i];
                try
                {
                    SaveContainerDto dto = container.ToDtoSnapshot();
                    string json = JsonConvert.SerializeObject(dto);
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    string path = GetSafePath(profileId, slotId);
                    EnsureDirectory(Path.GetDirectoryName(path));
                    await AtomicWriteAsync(path, bytes);
                    container.Save();
                }
                catch (IOException)
                {
                    // 写入异常：保留 Dirty，继续下一条
                }
            }
        }

        // 删除一个槽位：删除磁盘文件并移除缓存
        private void Delete(string profileId, string slotId)
        {
            string key = MakeKey(profileId, slotId);
            if(string.IsNullOrEmpty(key)) return;

            string path = GetSafePath(profileId, slotId);
            if(File.Exists(path)) File.Delete(path);
            _keyToContainer.Remove(key);
        }

        private static async UniTask AtomicWriteAsync(string path, byte[] bytes)
        {
            string tmp = path + ".tmp";
            const int maxRetries = 3;
            const int retryDelayMs = 100;

            for(var attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // 确保临时文件不存在
                    if(File.Exists(tmp))
                    {
                        try
                        {
                            File.Delete(tmp);
                        }
                        catch
                        {
                            /* 忽略删除失败 */
                        }
                    }

                    // 写入临时文件
                    using (FileStream fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await fs.WriteAsync(bytes, 0, bytes.Length);
                        await fs.FlushAsync();
                    }

                    // 删除目标文件（如果存在）
                    if(File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (IOException)
                        {
                            // 如果删除失败，等待一下再重试
                            if(attempt < maxRetries - 1)
                            {
                                await UniTask.Delay(retryDelayMs);
                                continue;
                            }
                            throw;
                        }
                    }

                    // 移动临时文件到目标位置
                    File.Move(tmp, path);
                    return; // 成功完成
                }
                catch (Exception ex) when (attempt < maxRetries - 1)
                {
                    // 清理可能残留的临时文件
                    try
                    {
                        if(File.Exists(tmp)) File.Delete(tmp);
                    }
                    catch
                    {
                        /* 忽略 */
                    }

                    Debug.LogWarning($"[SaveSystem] 原子写入失败 (尝试 {attempt + 1}/{maxRetries}): {ex.Message}");
                    await UniTask.Delay(retryDelayMs);
                }
            }

            // 最终清理尝试
            try
            {
                if(File.Exists(tmp)) File.Delete(tmp);
            }
            catch
            {
                /* 忽略 */
            }

            // 如果所有重试都失败，抛出异常
            throw new IOException($"[SaveSystem] 无法完成原子写入: {path}，已重试 {maxRetries} 次");
        }

        private static async UniTask<byte[]> ReadAllBytesAsync(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        // 枚举并加载某个 profile 下所有槽位到缓存
        private async UniTask LoadAllSlotsForProfileAsync(string profileId)
        {
            profileId = ValidateId(profileId);
            if(string.IsNullOrEmpty(profileId)) return;

            string safeProfile = SanitizeIdForPath(profileId);
            string dir = GetProfileDir(safeProfile);
            if(!Directory.Exists(dir))
            {
                // 首次进入：创建目录即可，暂时没有文件可加载
                EnsureDirectory(dir);
                return;
            }

            string[] files;
            try
            {
                // 正确枚举 json 文件（此前为 *.tmp 导致无法加载）
                files = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly);
            }
            catch (IOException e)
            {
                _logger.LogException(e);
                return;
            }

            if(files.Length == 0) return;

            for(var i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string slotId = Path.GetFileNameWithoutExtension(file);
                if(string.IsNullOrEmpty(slotId)) continue;
                await LoadAsync(profileId, slotId);
            }
        }

        // 规范化检查：禁止 ':'，并去除首尾空白
        private static string ValidateId(string id)
        {
            if(string.IsNullOrWhiteSpace(id)) return null;
            id = id.Trim();
            id = id.Replace(":", "_");
            return id;
        }

        // 文件路径安全：替换非法文件名字符
        private static string SanitizeIdForPath(string id)
        {
            if(string.IsNullOrEmpty(id)) return string.Empty;
            char[] invalid = Path.GetInvalidFileNameChars();
            StringBuilder sb = new StringBuilder(id.Length);
            for(var i = 0; i < id.Length; i++)
            {
                char c = id[i];
                var bad = false;
                for(var j = 0; j < invalid.Length; j++)
                {
                    if(c == invalid[j])
                    {
                        bad = true;
                        break;
                    }
                }

                sb.Append(bad ? '_' : c);
            }

            return sb.ToString();
        }

        // 清理指定档案的容器缓存
        private void ClearProfile(string profileId)
        {
            profileId = ValidateId(profileId);
            if(string.IsNullOrEmpty(profileId)) return;

            List<string> list = new List<string>();
            foreach (string key in _keyToContainer.Keys)
            {
                int idx = key.IndexOf(':');
                if(idx <= 0) continue;
                string p = key.Substring(0, idx);
                if(p == profileId) list.Add(key);
            }

            for(var i = 0; i < list.Count; i++) _keyToContainer.Remove(list[i]);
        }

        // 显式 profile/slot 获取
        private Slot GetSlot(string profileId, string slotId)
        {
            string key = MakeKey(profileId, slotId);
            if(string.IsNullOrEmpty(key)) return null;
            if(_keyToContainer.TryGetValue(key, out Slot existing)) return existing;
            Slot created = new Slot();
            _keyToContainer[key] = created;
            return created;
        }

        // 基于当前主 profile 获取容器
        private Slot GetSlot(string slotId)
        {
            return GetSlot(_currentProfileId, slotId);
        }

        // 路径策略：{persistentDataPath}/cframework/saves/{profileId}/{slotId}.json
        private static string GetPath(string profileId, string slotId)
        {
            string dir = Path.Combine(Application.persistentDataPath, "cframework", "saves", profileId);
            return Path.Combine(dir, slotId + ".json");
        }

        private static string GetProfileDir(string profileId)
        {
            return Path.Combine(Application.persistentDataPath, "cframework", "saves", profileId);
        }

        private static string GetSafePath(string profileId, string slotId)
        {
            profileId = SanitizeIdForPath(profileId);
            slotId = SanitizeIdForPath(slotId);
            string dir = Path.Combine(Application.persistentDataPath, "cframework", "saves", profileId);
            return Path.Combine(dir, slotId + ".json");
        }

        private static void EnsureDirectory(string dir)
        {
            if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        #endregion

        #region 查询

        [QueryHandler]
        private UniTask<Slot> OnGetContainer(SaveQueries.CurrentProfileSlot query, CancellationToken ct)
        {
            return UniTask.FromResult(GetSlot(query.SlotId));
        }

        [QueryHandler]
        private UniTask<string> OnGetCurrentProfile(SaveQueries.CurrentProfile query, CancellationToken ct)
        {
            return UniTask.FromResult(_currentProfileId);
        }

        #endregion

        #region 命令

        [CommandHandler]
        private UniTask OnCommand(SaveCommands.SetCurrentProfile command, CancellationToken cancelToken)
        {
            return SetCurrentProfile(command.ProfileId, cancelToken);
        }

        [CommandHandler]
        private UniTask OnCommand(SaveCommands.DeleteProfile command, CancellationToken cancelToken)
        {
            return DeleteProfile(command.ProfileId, cancelToken);
        }

        [CommandHandler]
        private UniTask OnCommand(SaveCommands.DeleteAllProfiles command, CancellationToken cancelToken)
        {
            return DeleteAllProfiles(cancelToken);
        }

        [CommandHandler]
        private UniTask OnCommand(SaveCommands.SaveSlot command, CancellationToken cancelToken)
        {
            return SaveAsync(_currentProfileId, command.SlotId, cancelToken);
        }

        [CommandHandler]
        private UniTask OnCommand(SaveCommands.DeleteSlot command, CancellationToken ct)
        {
            Delete(_currentProfileId, command.SlotId);
            return UniTask.CompletedTask;
        }

        [CommandHandler]
        private UniTask OnCommand(SaveCommands.SaveAllDirty command, CancellationToken cancelToken)
        {
            return SaveAllDirtyAsync(_currentProfileId, cancelToken);
        }

        private async UniTask SetCurrentProfile(string profileId, CancellationToken cancelToken)
        {
            // 先校验新 id，非法则直接返回，不影响当前档
            string newId = ValidateId(profileId);
            if(string.IsNullOrEmpty(newId))
            {
                _logger.LogError($"SetCurrentProfile 收到非法 profileId：'{profileId}'");
                return;
            }

            // 切换前保存旧主档案所有脏容器
            await SaveAllDirtyAsync(_currentProfileId, cancelToken);
            // 清理旧档案缓存，避免污染新档案的查询
            ClearProfile(_currentProfileId);
            // 切换到新档案
            _currentProfileId = newId;
            // 加载新档案所有槽位
            await LoadAllSlotsForProfileAsync(newId);
        }

        private async UniTask DeleteProfile(string profileId, CancellationToken cancelToken)
        {
            string pid = ValidateId(profileId);
            if(string.IsNullOrEmpty(pid)) return;
            // 清理内存缓存
            ClearProfile(pid);
            // 删除磁盘目录（递归）
            string dir = Path.Combine(Application.persistentDataPath, "cframework", "saves", SanitizeIdForPath(pid));
            if(Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (IOException e)
                {
                    _logger.LogException(e);
                }
            }

            // 若删除的是当前主档案，可选重置为默认
            if(_currentProfileId == pid)
            {
                _currentProfileId = DefaultProfileId;
                await LoadAllSlotsForProfileAsync(_currentProfileId);
            }
        }

        private async UniTask DeleteAllProfiles(CancellationToken cancelToken)
        {
            _keyToContainer.Clear();

            string rootDir = Path.Combine(Application.persistentDataPath, "cframework", "saves");
            if(Directory.Exists(rootDir))
            {
                try
                {
                    Directory.Delete(rootDir, true);
                }
                catch (IOException e)
                {
                    _logger.LogException(e);
                }
            }

            _currentProfileId = DefaultProfileId;
            await LoadAllSlotsForProfileAsync(_currentProfileId);
        }

        #endregion
    }
}