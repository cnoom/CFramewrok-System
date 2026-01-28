using System;

namespace CFramework.Data
{
    public class EasyEvent : IDisposable
    {

        public void Dispose()
        {
            mOnEvent = null;
        }
        private event Action mOnEvent;

        public EasyEvent Register(Action onEvent)
        {
            mOnEvent += onEvent;
            return this;
        }

        public EasyEvent Register(EasyEvent otherEvent)
        {
            mOnEvent += otherEvent.mOnEvent;
            return this;
        }

        public EasyEvent UnRegister(Action onEvent)
        {
            mOnEvent -= onEvent;
            return this;
        }

        public EasyEvent UnRegister(EasyEvent otherEvent)
        {
            mOnEvent -= otherEvent.mOnEvent;
            return this;
        }

        public void Trigger()
        {
            mOnEvent?.Invoke();
        }
    }

    public class EasyEvent<T> : IDisposable
    {

        public void Dispose()
        {
            mOnEvent = null;
        }
        private event Action<T> mOnEvent;

        public EasyEvent<T> Register(Action<T> onEvent)
        {
            mOnEvent += onEvent;
            return this;
        }

        public EasyEvent<T> Register(EasyEvent<T> otherEvent)
        {
            mOnEvent += otherEvent.mOnEvent;
            return this;
        }

        public EasyEvent<T> UnRegister(Action<T> onEvent)
        {
            mOnEvent -= onEvent;
            return this;
        }

        public EasyEvent<T> UnRegister(EasyEvent<T> otherEvent)
        {
            mOnEvent -= otherEvent.mOnEvent;
            return this;
        }

        public void Trigger(T arg)
        {
            mOnEvent?.Invoke(arg);
        }
    }

    public class EasyEvent<T1, T2> : IDisposable
    {

        public void Dispose()
        {
            mOnEvent = null;
        }
        private event Action<T1, T2> mOnEvent;

        public EasyEvent<T1, T2> Register(Action<T1, T2> onEvent)
        {
            mOnEvent += onEvent;
            return this;
        }

        public EasyEvent<T1, T2> Register(EasyEvent<T1, T2> otherEvent)
        {
            mOnEvent += otherEvent.mOnEvent;
            return this;
        }

        public EasyEvent<T1, T2> UnRegister(Action<T1, T2> onEvent)
        {
            mOnEvent -= onEvent;
            return this;
        }

        public EasyEvent<T1, T2> UnRegister(EasyEvent<T1, T2> otherEvent)
        {
            mOnEvent -= otherEvent.mOnEvent;
            return this;
        }

        public void Trigger(T1 arg1, T2 arg2)
        {
            mOnEvent?.Invoke(arg1, arg2);
        }
    }

    public class EasyEvent<T1, T2, T3> : IDisposable
    {

        public void Dispose()
        {
            mOnEvent = null;
        }
        private event Action<T1, T2, T3> mOnEvent;

        public EasyEvent<T1, T2, T3> Register(Action<T1, T2, T3> onEvent)
        {
            mOnEvent += onEvent;
            return this;
        }

        public EasyEvent<T1, T2, T3> Register(EasyEvent<T1, T2, T3> otherEvent)
        {
            mOnEvent += otherEvent.mOnEvent;
            return this;
        }

        public EasyEvent<T1, T2, T3> UnRegister(EasyEvent<T1, T2, T3> otherEvent)
        {
            mOnEvent -= otherEvent.mOnEvent;
            return this;
        }

        public EasyEvent<T1, T2, T3> UnRegister(Action<T1, T2, T3> onEvent)
        {
            mOnEvent -= onEvent;
            return this;
        }

        public void Trigger(T1 arg1, T2 arg2, T3 arg3)
        {
            mOnEvent?.Invoke(arg1, arg2, arg3);
        }
    }
}