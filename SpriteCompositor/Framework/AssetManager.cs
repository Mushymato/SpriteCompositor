using StardewModdingAPI;
using StardewModdingAPI.Events;
using SpriteCompAsset = System.Collections.Generic.Dictionary<string, SpriteCompositor.Framework.SpriteComp>;

namespace SpriteCompositor.Framework;

internal class AssetManager
{
    internal const string TxPrefix = $"{ModEntry.ModId}.Tx/";

    internal static readonly Dictionary<string, IAssetName> ValidAssetNames = [];
    internal readonly Dictionary<IAssetName, SpriteCompAsset> Loaded = [];
    private readonly HashSet<IAssetName> WillInvalidateNextTick = [];

    private readonly IModHelper helper;

    internal AssetManager(IModHelper helper)
    {
        this.helper = helper;

        helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.GameLoop.UpdateTicked += OnUpdatedTicked;

        foreach (IModInfo info in helper.ModRegistry.GetAll())
        {
            if (info.Manifest.ExtraFields.ContainsKey(ModEntry.ModId))
            {
                ValidAssetNames[info.Manifest.UniqueID] = helper.GameContent.ParseAssetName(
                    string.Concat(ModEntry.ModId, "/Layers/", info.Manifest.UniqueID)
                );
            }
        }
        ModEntry.Log($"Active for: {string.Join(',', ValidAssetNames.Keys)}");
        ModEntry.Log($"Tracking: {string.Join(',', ValidAssetNames.Values)}");
    }

    private SpriteCompAsset GetData(IAssetName assetName)
    {
        SpriteCompAsset asset = helper.GameContent.Load<SpriteCompAsset>(assetName);
        Loaded[assetName] = asset;
        return asset;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (ValidAssetNames.ContainsValue(e.Name))
        {
            ModEntry.Log($"Loading '{e.Name}'");
            e.LoadFrom(() => new SpriteCompAsset(), AssetLoadPriority.Exclusive);
        }
        else if (e.Name.StartsWith(TxPrefix))
        {
            ModEntry.Log($"Mine: {e.Name}");
            string[] parts = e.Name.BaseName.Split('/');
            if (parts.Length < 3)
                return;
            string modId = parts[1];
            if (!ValidAssetNames.TryGetValue(modId, out IAssetName? assetName))
                return;
            ModEntry.Log($"Layers: {assetName}");
            SpriteCompAsset spriteComp = GetData(assetName);
            string key = string.Join('/', parts.Skip(2));
            if (spriteComp.TryGetValue(key, out SpriteComp? comp))
            {
                e.LoadFrom(() => comp.Initialize(e.Name), AssetLoadPriority.Exclusive);
                e.Edit((asset) => comp.Compose(asset, helper.GameContent), AssetEditPriority.Default);
            }
        }
    }

    private void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        foreach ((IAssetName name, SpriteCompAsset spriteComp) in Loaded)
        {
            foreach (SpriteComp comp in spriteComp.Values)
            {
                if (comp.WillInvalidate(e.Names) is IAssetName willInvalidate)
                {
                    WillInvalidateNextTick.Add(willInvalidate);
                }
            }
        }
        foreach (IAssetName name in e.Names)
        {
            Loaded.Remove(name);
        }
    }

    private void OnUpdatedTicked(object? sender, UpdateTickedEventArgs e)
    {
        foreach (IAssetName name in WillInvalidateNextTick)
        {
            helper.GameContent.InvalidateCache(name);
        }
        WillInvalidateNextTick.Clear();
    }
}
