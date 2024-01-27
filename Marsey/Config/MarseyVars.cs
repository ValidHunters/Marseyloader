using System;
using Marsey.Stealthsey;
using Marsey.Misc;

namespace Marsey.Config;

/// <summary>
/// Variables only set at compile
/// </summary>
public static class MarseyVars
{
    /// <summary>
    /// Due to the nature of how Marseyloader is compiled (its not) we cannot check it's version 
    /// </summary>
    public static readonly Version MarseyVersion = new Version("2.12.0");

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
    /// Marseypatches are saved to this one
    /// </summary>
    public static readonly string MarseyPatchFolder = "Marsey";

    /// <summary>
    /// Log identified for marseyloader
    /// </summary>
    public static readonly string MarseyLoggerPrefix = "MARSEY";
    
    /// <summary>
    /// Refuse to play on servers over this engine version if hidesey is disabled
    /// <seealso cref="Abjure"/>
    /// </summary>
    public static readonly Version Detection = new Version("183.0.0");
}
