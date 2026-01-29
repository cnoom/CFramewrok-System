using System;
using System.Collections.Generic;
using CFramework.Core;
using CFramework.Core.Log;

namespace CFramework.Systems.AssetsSystem.AssetsReceiver
{
    public class AssetsReceiverManager : IDisposable
    {
        private readonly AssetsSystemModule _assetsSystemModule;
        private readonly CFLogger _logger;
        private readonly Dictionary<Type, IAssetsReceiver> _receivers;

        public AssetsReceiverManager(AssetsSystemModule assetsSystemModule, CFLogger logger)
        {
            _assetsSystemModule = assetsSystemModule;
            _logger = logger;
            _receivers = new Dictionary<Type, IAssetsReceiver>();
        }

        public void Dispose()
        {
            List<Type> keys = new List<Type>(_receivers.Keys);
            foreach (Type type in keys)
            {
                UnregisterReceiver(type);
            }
        }

        public void RegisterReceiver(Type providerType)
        {
            if(_receivers.ContainsKey(providerType))
            {
                _logger.LogWarning($"重复注册AssetsProvider:{providerType.FullName}");
                return;
            }

            IAssetsReceiver receiver =
                (IAssetsReceiver)Activator.CreateInstance(typeof(AssetsReceiver<>).MakeGenericType(providerType));
            receiver.AssetsSystemModule = _assetsSystemModule;
            _receivers.Add(providerType, receiver);
            CF.RegisterHandler(receiver);
            _logger.LogInfo($"注册AssetsProvider:{providerType.Name}");
        }

        public bool UnregisterReceiver(Type providerType)
        {
            if(!HasReceiver(providerType))
            {
                return false;
            }

            IAssetsReceiver receiver = _receivers[providerType];
            CF.UnregisterHandler(receiver);
            return _receivers.Remove(providerType);
        }

        public bool HasReceiver(Type providerType)
        {
            return _receivers.ContainsKey(providerType);
        }
    }
}