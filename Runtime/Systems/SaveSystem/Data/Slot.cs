using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CFramework.Systems.SaveSystem.Data
{
    // 持久化用的轻量 DTO
    [Serializable]
    public class SaveContainerDto
    {
        public Dictionary<string, bool> boolDict = new Dictionary<string, bool>();
        public Dictionary<string, float> floatDict = new Dictionary<string, float>();
        public Dictionary<string, int> intDict = new Dictionary<string, int>();
        public Dictionary<string, string> objectDict = new Dictionary<string, string>();
        public Dictionary<string, string> stringDict = new Dictionary<string, string>();
    }

    public class Slot
    {
        private readonly Dictionary<string, bool> _boolDict = new Dictionary<string, bool>();
        private readonly Dictionary<string, float> _floatDict = new Dictionary<string, float>();

        private readonly Dictionary<string, int> _intDict = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _objectDict = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _stringDict = new Dictionary<string, string>();
        internal bool Dirty { get; private set; }

        // 便捷：存储点（checkpoint）操作，内部以 "checkpoint:{id}" 作为键
        public void SetCheckpoint<T>(string checkpointId, T snapshot)
        {
            SetObject($"checkpoint:{checkpointId}", snapshot);
        }

        public T GetCheckpoint<T>(string checkpointId)
        {
            return GetObject<T>($"checkpoint:{checkpointId}");
        }

        private void ToDirty()
        {
            Dirty = true;
        }

        internal void Save()
        {
            Dirty = false;
        }

        // 创建 DTO 快照用于序列化
        internal SaveContainerDto ToDtoSnapshot()
        {
            return new SaveContainerDto
            {
                intDict = new Dictionary<string, int>(_intDict),
                floatDict = new Dictionary<string, float>(_floatDict),
                stringDict = new Dictionary<string, string>(_stringDict),
                boolDict = new Dictionary<string, bool>(_boolDict),
                objectDict = new Dictionary<string, string>(_objectDict)
            };
        }

        // 从 DTO 填充容器（反序列化后调用）
        internal void LoadFromDto(SaveContainerDto dto)
        {
            _intDict.Clear();
            _floatDict.Clear();
            _stringDict.Clear();
            _boolDict.Clear();
            _objectDict.Clear();

            foreach (KeyValuePair<string, int> kv in dto.intDict) _intDict[kv.Key] = kv.Value;
            foreach (KeyValuePair<string, float> kv in dto.floatDict) _floatDict[kv.Key] = kv.Value;
            foreach (KeyValuePair<string, string> kv in dto.stringDict) _stringDict[kv.Key] = kv.Value;
            foreach (KeyValuePair<string, bool> kv in dto.boolDict) _boolDict[kv.Key] = kv.Value;
            foreach (KeyValuePair<string, string> kv in dto.objectDict) _objectDict[kv.Key] = kv.Value;

            Save(); // 载入完成视为干净状态
        }

        #region 获取

        public int GetInt(string key, int defaultValue = 0)
        {
            return _intDict.GetValueOrDefault(key, defaultValue);
        }

        public float GetFloat(string key, float defaultValue = 0)
        {
            return _floatDict.GetValueOrDefault(key, defaultValue);
        }

        public string GetString(string key, string defaultValue = "")
        {
            return _stringDict.GetValueOrDefault(key, defaultValue);
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return _boolDict.GetValueOrDefault(key, defaultValue);
        }

        public T GetObject<T>(string key)
        {
            string json = _objectDict.GetValueOrDefault(key, string.Empty);
            if(string.IsNullOrEmpty(json))
            {
                return default;
            }

            T tObject = JsonConvert.DeserializeObject<T>(json);
            return tObject;
        }

        #endregion

        #region 保存

        public void SetInt(string key, int value)
        {
            _intDict[key] = value;
            ToDirty();
        }

        public void SetFloat(string key, float value)
        {
            _floatDict[key] = value;
            ToDirty();
        }

        public void SetString(string key, string value)
        {
            _stringDict[key] = value;
            ToDirty();
        }

        public void SetBool(string key, bool value)
        {
            _boolDict[key] = value;
            ToDirty();
        }

        public void SetObject<T>(string key, T value)
        {
            string json = JsonConvert.SerializeObject(value);
            _objectDict[key] = json;
            ToDirty();
        }

        #endregion
    }
}