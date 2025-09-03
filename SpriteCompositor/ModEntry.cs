using System.Diagnostics;
using SpriteCompositor.Framework;
using SpriteCompositor.Integration;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace SpriteCompositor;

public sealed class ModEntry : Mod
{
#if DEBUG
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#else
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Trace;
#endif

    public const string ModId = "mushymato.SCOMP";
    public const string ContentPatcher = "Pathoschild.ContentPatcher";
    private static IMonitor? mon;

    internal AssetManager Asset { get; set; } = null!;

    public override void Entry(IModHelper helper)
    {
        mon = Monitor;
        Asset = new AssetManager(helper);
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (Helper.ModRegistry.GetApi<IContentPatcherAPI>(ContentPatcher) is IContentPatcherAPI cpApi)
        {
            cpApi.RegisterToken(ModManifest, "Tx", new TxToken());
        }
    }

    /// <summary>SMAPI static monitor Log wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void Log(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.Log(msg, level);
    }

    /// <summary>SMAPI static monitor LogOnce wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void LogOnce(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.LogOnce(msg, level);
    }

    /// <summary>SMAPI static monitor Log wrapper, debug only</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    [Conditional("DEBUG")]
    internal static void LogDebug(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.Log(msg, level);
    }
}
