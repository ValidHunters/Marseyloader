using System;
using Marsey.Stealthsey;
using Marsey.Misc;

namespace Marsey.Config;

/// <summary>
/// Variables only set at compile
/// </summary>
public static class MarseyVars
{
    // TODO: Kill this
    public static readonly Version MarseyVersion = new Version("2.18");

    public static readonly string EnabledPatchListFileName = "patches.marsey";

    /// <summary>
    /// Default MarseyAPI endpoint url
    /// </summary>
    public static readonly string MarseyApiEndpoint = "https://fujo.love/api/v1";

    /// <summary>
    /// Namespace identifier for Harmony
    /// </summary>
    public static readonly string Identifier = "com.validhunters.marseyloader";

    /// <summary>
    /// Max amount of loops allowed to catch game assemblies
    /// </summary>
    public static readonly int MaxLoops = 50;

    /// <summary>
    /// Cooldown to try the loop again, in ms
    /// </summary>
    public static readonly int LoopCooldown = 200;

    /// <summary>
    /// Name of folder containing files used by Marsey
    /// </summary>
    public static readonly string MarseyFolder = "Marsey";

    /// <summary>
    /// Folder containing mods
    /// </summary>
    public static readonly string MarseyPatchFolder = Path.Combine(MarseyFolder, "Mods");

    /// <summary>
    /// Folder containing Resource Packs
    /// </summary>
    public static readonly string MarseyResourceFolder = Path.Combine(MarseyFolder, "ResourcePacks");

    /// <summary>
    /// Log identified for marseyloader
    /// </summary>
    public static readonly string MarseyLoggerPrefix = "MARSEY";

    public static readonly string MarseyLoggerFileName = "client.marsey.log";

    /// <summary>
    /// Refuse to play on servers over or equal to this engine version if hidesey is disabled
    /// <see cref="Abjure"/>
    /// </summary>
    public static readonly Version Detection = new Version("183.0.0");
}
