using System;
using JetBrains.Annotations;
using Marsey.Stealthsey;
using SS14.Launcher.Utility;

namespace SS14.Launcher.Models.Data;

/// <summary>
/// Contains definitions for all launcher configuration values.
/// </summary>
/// <remarks>
/// The fields of this class are automatically searched for all CVar definitions.
/// </remarks>
/// <see cref="DataManager"/>
[UsedImplicitly]
public static class CVars
{
    /// <summary>
    /// Default to using compatibility options for rendering etc,
    /// that are less likely to immediately crash on buggy drivers.
    /// </summary>
    public static readonly CVarDef<bool> CompatMode = CVarDef.Create("CompatMode", false);

    /// <summary>
    /// Run client with dynamic PGO.
    /// </summary>
    public static readonly CVarDef<bool> DynamicPgo = CVarDef.Create("DynamicPgo", true);

    /// <summary>
    /// On first launch, the launcher tells you that SS14 is EARLY ACCESS.
    /// This stores whether they dismissed that, though people will insist on pretending it defaults to true.
    /// </summary>
    public static readonly CVarDef<bool> HasDismissedEarlyAccessWarning
        = CVarDef.Create("HasDismissedEarlyAccessWarning", false);

    /// <summary>
    /// Disable checking engine build signatures when launching game.
    /// Only enable if you know what you're doing.
    /// </summary>
    /// <remarks>
    /// This is ignored on release builds, for security reasons.
    /// </remarks>
    public static readonly CVarDef<bool> DisableSigning = CVarDef.Create("DisableSigning", false);

    /// <summary>
    /// Enable logging of launched client instances to file.
    /// </summary>
    public static readonly CVarDef<bool> LogClient = CVarDef.Create("LogClient", false);

    /// <summary>
    /// Enable logging of launched client instances to file.
    /// </summary>
    public static readonly CVarDef<bool> LogLauncher = CVarDef.Create("LogLauncher", false);

    /// <summary>
    /// Verbose logging of launcher logs.
    /// </summary>
    public static readonly CVarDef<bool> LogLauncherVerbose = CVarDef.Create("LogLauncherVerbose", false);

    /// <summary>
    /// Currently selected login in the drop down.
    /// </summary>
    public static readonly CVarDef<string> SelectedLogin = CVarDef.Create("SelectedLogin", "");

    /// <summary>
    /// Maximum amount of TOTAL versions to keep in the content database.
    /// </summary>
    public static readonly CVarDef<int> MaxVersionsToKeep = CVarDef.Create("MaxVersionsToKeep", 15);

    /// <summary>
    /// Maximum amount of versions to keep of a specific fork ID.
    /// </summary>
    public static readonly CVarDef<int> MaxForkVersionsToKeep = CVarDef.Create("MaxForkVersionsToKeep", 3);

    /// <summary>
    /// Whether to display override assets (trans rights).
    /// </summary>
    public static readonly CVarDef<bool> OverrideAssets = CVarDef.Create("OverrideAssets", false);

    /// <summary>
    /// Stores the minimum player count value used by the "minimum player count" filter.
    /// </summary>
    /// <seealso cref="ServerFilter.PlayerCountMin"/>
    public static readonly CVarDef<int> FilterPlayerCountMinValue = CVarDef.Create("FilterPlayerCountMinValue", 0);

    /// <summary>
    /// Stores the maximum player count value used by the "maximum player count" filter.
    /// </summary>
    /// <seealso cref="ServerFilter.PlayerCountMax"/>
    public static readonly CVarDef<int> FilterPlayerCountMaxValue = CVarDef.Create("FilterPlayerCountMaxValue", 0);

    /// <summary>
    /// Enable local overriding of engine versions.
    /// </summary>
    /// <remarks>
    /// If enabled and on a development build,
    /// the launcher will pull all engine versions and modules from <see cref="EngineOverridePath"/>.
    /// This can be set to <c>RobustToolbox/release/</c> to instantly pull in packaged engine builds.
    /// </remarks>
    public static readonly CVarDef<bool> EngineOverrideEnabled = CVarDef.Create("EngineOverrideEnabled", false);

    /// <summary>
    /// Path to load engines from when using <see cref="EngineOverrideEnabled"/>.
    /// </summary>
    public static readonly CVarDef<string> EngineOverridePath = CVarDef.Create("EngineOverridePath", "");

    /// <summary>
    /// Stores whether the user has seen the Wine warning.
    /// </summary>
    public static readonly CVarDef<bool> WineWarningShown = CVarDef.Create("WineWarningShown", false);


    // MarseyCVars start here

    // Stealthsey

    /// <summary>
    /// Define strict level
    /// </summary>
    public static readonly CVarDef<int> MarseyHide = CVarDef.Create("HideLevel", 2);

    // Logging

    /// <summary>
    /// Log messages coming from patches
    /// </summary>
    public static readonly CVarDef<bool> LogPatcher = CVarDef.Create("LogPatcher", true);

    /// <summary>
    /// Log debug messages coming from loader
    /// </summary>
    public static readonly CVarDef<bool> LogLoaderDebug = CVarDef.Create("LogLoaderDebug", false);

    /// <summary>
    /// Log patcher output to a separate file
    /// </summary>
    public static readonly CVarDef<bool> SeparateLogging = CVarDef.Create("SeparateLogging", false);

    /// <summary>
    /// Log patcher output in launcher
    /// </summary>
    public static readonly CVarDef<bool> LogLauncherPatcher = CVarDef.Create("LogLauncherPatcher", false);

    /// <summary>
    /// Log TRC messages
    /// </summary>
    /// <remarks>Hidden behind the debug compile flag</remarks>
    public static readonly CVarDef<bool> LogLoaderTrace = CVarDef.Create("LogLoaderTrace", false);

    // Behavior

    /// <summary>
    /// Throw an exception if a patch fails to apply.
    /// </summary>
    public static readonly CVarDef<bool> ThrowPatchFail = CVarDef.Create("ThrowPatchFail", false);

    /// <summary>
    /// Ignore target checks when using a resource pack
    /// </summary>
    public static readonly CVarDef<bool> DisableStrict = CVarDef.Create("DisableStrict", false);

    /// <summary>
    /// Do we disable RPC?
    /// </summary>
    public static readonly CVarDef<bool> DisableRPC = CVarDef.Create("DisableRPC", false);

    /// <summary>
    /// Do we fake the username on RPC?
    /// </summary>
    public static readonly CVarDef<bool> FakeRPC = CVarDef.Create("FakeRPC", false);

    /// <summary>
    /// Username to fake RPC with
    /// </summary>
    public static readonly CVarDef<string> RPCUsername = CVarDef.Create("RPCUsername", "");

    /// <summary>
    /// Do we disable redialing?
    /// </summary>
    public static readonly CVarDef<bool> JamDials = CVarDef.Create("JamDials", false);

    /// <summary>
    /// Do we disable remote command execution
    /// </summary>
    public static readonly CVarDef<bool> Blackhole = CVarDef.Create("Blackhole", false);

    /// <summary>
    /// Do we force a hwid value
    /// </summary>
    public static readonly CVarDef<bool> ForcingHWId = CVarDef.Create("ForcingHWId", false);

    /// <summary>
    /// Do we use the HWID value bound to LoginInfo
    /// </summary>
    public static readonly CVarDef<bool> LIHWIDBind = CVarDef.Create("LIHWIDBind", false);

    /// <summary>
    /// Do not log in anywhere when starting the loader
    /// </summary>
    public static readonly CVarDef<bool> NoActiveInit = CVarDef.Create("NoActiveInit", false);

    /// <summary>
    /// Apply backports to game when able
    /// </summary>
    public static readonly CVarDef<bool> Backports = CVarDef.Create("Backports", false);

    /// <summary>
    /// Apply backports to game when able
    /// </summary>
    public static readonly CVarDef<bool> DisableAnyEngineBackports = CVarDef.Create("DisableAnyEngineBackports", false);

    // HWID

    /// <summary>
    /// Do we use a random HWID each time?
    /// </summary>
    public static readonly CVarDef<bool> RandHWID = CVarDef.Create("RandHWID", false);

    /// <summary>
    /// HWId to use on servers
    /// </summary>
    public static readonly CVarDef<string> ForcedHWId = CVarDef.Create("ForcedHWId", "");

    // API

    /// <summary>
    /// Enable MarseyApi
    /// </summary>
    public static readonly CVarDef<bool> MarseyApi = CVarDef.Create("MarseyApi", true);

    /// <summary>
    /// MarseyApi endpoint url
    /// </summary>
    public static readonly CVarDef<string> MarseyApiEndpoint = CVarDef.Create("MarseyApiEndpoint", "https://fujo.love/api/v1");

    /// <summary>
    /// Ignore "Marsey-M" (required updates) releases
    /// </summary>
    public static readonly CVarDef<bool> MarseyApiIgnoreForced = CVarDef.Create("MarseyApiIgnoreForced", false);

    // Fluff

    /// <summary>
    /// Use a random title and tagline combination
    /// </summary>
    public static readonly CVarDef<bool> RandTitle = CVarDef.Create("RandTitle", true);

    /// <summary>
    /// Use a random header image
    /// </summary>
    public static readonly CVarDef<bool> RandHeader = CVarDef.Create("RandHeader", true);

    /// <summary>
    /// Replace connection messages with a random (un)funny action
    /// </summary>
    public static readonly CVarDef<bool> RandConnAction = CVarDef.Create("RandConnAction", false);

    /// <summary>
    /// Username used in guest mode
    /// </summary>
    public static readonly CVarDef<string> GuestUsername = CVarDef.Create("GuestUsername", "Guest");

    /// <summary>
    /// Do not patch anything in the game modules
    /// </summary>
    public static readonly CVarDef<bool> Patchless = CVarDef.Create("Patchless", false);

    /// <summary>
    /// HWID2 - Disallow sending hwid to server.
    /// </summary>
    public static readonly CVarDef<bool> DisallowHwid = CVarDef.Create("DisallowHwid", false);
}

/// <summary>
/// Base definition of a CVar.
/// </summary>
/// <seealso cref="DataManager"/>
/// <seealso cref="CVars"/>
public abstract class CVarDef
{
    public string Name { get; }
    public object? DefaultValue { get; }
    public Type ValueType { get; }

    private protected CVarDef(string name, object? defaultValue, Type type)
    {
        Name = name;
        DefaultValue = defaultValue;
        ValueType = type;
    }

    public static CVarDef<T> Create<T>(
        string name,
        T defaultValue)
        where T : notnull
    {
        return new CVarDef<T>(name, defaultValue);
    }
}

/// <summary>
/// Generic specialized definition of CVar definition.
/// </summary>
/// <typeparam name="T">The type of value stored in this CVar.</typeparam>
public sealed class CVarDef<T> : CVarDef
{
    public new T DefaultValue { get; }

    internal CVarDef(string name, T defaultValue) : base(name, defaultValue, typeof(T))
    {
        DefaultValue = defaultValue;
    }
}
