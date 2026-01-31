using System;
using System.Collections.Generic;

namespace CFramework.Data
{
    public class Blackboard
    {

        private readonly Dictionary<int, IBlackboardSlot> _slots = new Dictionary<int, IBlackboardSlot>(64);

        public void SetData<T>(int key, T value)
        {
            if(_slots.TryGetValue(key, out IBlackboardSlot slot))
            {
                if(slot is not BlackboardSlot<T> typedSlot)
                    throw new InvalidOperationException(
                        $"Key {key} 已绑定类型 {slot.ValueType.FullName}，与写入类型 {typeof(T).FullName} 不一致。");
                typedSlot.Value = value;
                return;
            }

            _slots[key] = new BlackboardSlot<T>
            {
                Value = value
            };
        }

        public T GetData<T>(int key)
        {
            if(!_slots.TryGetValue(key, out IBlackboardSlot slot))
                throw new KeyNotFoundException($"Type {typeof(T).FullName} Key {key} not found in blackboard.");
            if(slot is BlackboardSlot<T> typedSlot)
            {
                return typedSlot.Value;
            }

            throw new KeyNotFoundException($"值类型与预期不符\n预期:{typeof(T).FullName} 实际:{slot.ValueType.FullName}");
        }

        public bool TryGetData<T>(int key, out T value)
        {
            if(_slots.TryGetValue(key, out IBlackboardSlot slot) && slot is BlackboardSlot<T> typedSlot)
            {
                value = typedSlot.Value;
                return true;
            }

            value = default;
            return false;
        }

        public T GetOrDefault<T>(int key, T defaultValue = default)
        {
            return TryGetData(key, out T value) ? value : defaultValue;
        }

        public bool ContainsKey(int key)
        {
            return _slots.ContainsKey(key);
        }

        public bool ContainsKey<T>(int key)
        {
            return _slots.TryGetValue(key, out IBlackboardSlot slot) && slot is BlackboardSlot<T>;
        }

        public bool RemoveData(int key)
        {
            return _slots.Remove(key);
        }

        public void ResetWithType<T>(int key, T value)
        {
            // 如果键已存在但类型不同，先删除旧 slot
            if(_slots.TryGetValue(key, out IBlackboardSlot existingSlot))
            {
                if(existingSlot is not BlackboardSlot<T>)
                {
                    _slots.Remove(key);
                }
            }

            // 设置新值（类型安全）
            _slots[key] = new BlackboardSlot<T>
            {
                Value = value
            };
        }

        public void Clear()
        {
            if(_slots.Count == 0) return;
            foreach (KeyValuePair<int, IBlackboardSlot> kv in _slots)
            {
                kv.Value.Clear();
            }

            _slots.Clear();
        }
        private interface IBlackboardSlot
        {
            Type ValueType { get; }
            void Clear();
        }

        private sealed class BlackboardSlot<T> : IBlackboardSlot
        {
            public T Value;
            public Type ValueType => typeof(T);

            public void Clear()
            {
                Value = default;
            }
        }
    }
}