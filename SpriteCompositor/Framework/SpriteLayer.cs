using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace SpriteCompositor.Framework;

public sealed class SpriteLayer
{
    // public
    public string Id { get; set; } = null!;
    public string Texture { get; set; } = null!;
    public PatchMode Mode { get; set; } = PatchMode.Overlay;
    public Rectangle? FromArea { get; set; } = null;
    public Rectangle? ToArea { get; set; } = null;

    // private
    internal IAssetName? TxAsset;
    internal Texture2D? Tx;
}

public sealed class SpriteComp
{
    // public
    public string Id { get; set; } = null!;
    public List<SpriteLayer> Layers { get; set; } = [];

    internal IAssetName? IdAsset = null;

    internal Texture2D Initialize(IAssetName assetName)
    {
        IdAsset = assetName;
        ModEntry.Log($"Initialize: {assetName}", LogLevel.Info);
        return new Texture2D(Game1.graphics.GraphicsDevice, 1, 1) { Name = assetName.Name };
    }

    internal void Compose(IAssetData asset, IGameContentHelper content)
    {
        ModEntry.Log($"Compose: {asset.Name}");
        IAssetDataForImage editor = asset.AsImage();
        int width = editor.Data.Width;
        int height = editor.Data.Height;
        // pass 1 load tx and find needed dimensions
        foreach (SpriteLayer layer in Layers)
        {
            if (layer.Texture == null)
                continue;
            layer.TxAsset ??= content.ParseAssetName(layer.Texture);
            if (layer.TxAsset.StartsWith(AssetManager.TxPrefix))
            {
                ModEntry.Log($"Cannot use {ModEntry.ModId} asset in comp layer", LogLevel.Warn);
                continue;
            }
            if (!content.DoesAssetExist<Texture2D>(layer.TxAsset))
            {
                ModEntry.Log($"Asset '{layer.TxAsset}' does not exist", LogLevel.Warn);
                continue;
            }

            layer.Tx ??= content.Load<Texture2D>(layer.TxAsset);
            if (layer.ToArea.HasValue)
            {
                width = Math.Max(width, layer.ToArea.Value.X + layer.ToArea.Value.Width);
                height = Math.Max(height, layer.ToArea.Value.Y + layer.ToArea.Value.Height);
            }
            else
            {
                width = Math.Max(width, layer.Tx.Width);
                height = Math.Max(height, layer.Tx.Height);
            }
        }
        editor.ExtendImage(width, height);
        foreach (SpriteLayer layer in Layers)
        {
            ModEntry.Log($"Layer: {layer.TxAsset}");
            if (layer.Tx == null)
                continue;
            editor.PatchImage(layer.Tx, layer.FromArea, layer.ToArea, layer.Mode);
        }
    }

    internal IAssetName? WillInvalidate(IReadOnlySet<IAssetName> names)
    {
        if (IdAsset == null)
        {
            return IdAsset;
        }
        bool shouldInvalidate = false;
        ModEntry.Log($"Invalidate: {string.Join(',', names)}");
        foreach (SpriteLayer layer in Layers)
        {
            if (layer.TxAsset != null && names.Contains(layer.TxAsset))
            {
                layer.Tx = null;
                shouldInvalidate = true;
            }
        }
        if (shouldInvalidate || names.Contains(IdAsset))
        {
            ModEntry.Log($"InvalidateCache: {IdAsset}");
            IAssetName idAsset = IdAsset;
            IdAsset = null;
            return idAsset;
        }
        return null;
    }
}
