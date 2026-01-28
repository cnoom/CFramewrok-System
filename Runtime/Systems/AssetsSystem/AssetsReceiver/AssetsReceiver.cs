using System;
using System.Threading;
using CFramework.Core.Attributes;
using CFramework.Core.Log;
using Cysharp.Threading.Tasks;

namespace CFramework.Systems.AssetsSystem.AssetsReceiver
{
    internal interface IAssetsReceiver
    {
        internal AssetsSystemModule AssetsSystemModule { get; set; }
    }

    internal class AssetsReceiver<TAssetType> : IAssetsReceiver
    {
        private AssetsSystemModule _assetsSystemModule;
        private CFLogger _cfLogger;

        public Type AssetType { get; set; }

        AssetsSystemModule IAssetsReceiver.AssetsSystemModule
        {
            get => _assetsSystemModule;
            set => _assetsSystemModule = value;
        }

        [QueryHandler]
        public UniTask<TAssetType> LoadAsset(AssetsQueries.Asset query, CancellationToken cancellationToken)
        {
            return _assetsSystemModule.LoadAssetAsync<TAssetType>(query.Address, cancellationToken);
        }

        [QueryHandler]
        public UniTask<TAssetType[]> LoadAssets(AssetsQueries.Assets query, CancellationToken cancellationToken)
        {
            return _assetsSystemModule.LoadAssetsAsync<TAssetType>(query.Label, cancellationToken);
        }

        [CommandHandler]
        public UniTask OnCommand(AssetsCommands.ReleaseAsset<TAssetType> command, CancellationToken ct)
        {
            _assetsSystemModule.ReleaseAsset<TAssetType>(command.Path);
            return UniTask.CompletedTask;
        }

        [CommandHandler]
        public UniTask OnCommand(AssetsCommands.ReleaseAssets<TAssetType> command, CancellationToken ct)
        {
            _assetsSystemModule.ReleaseAsset<TAssetType>(command.Label);
            return UniTask.CompletedTask;
        }
    }
}