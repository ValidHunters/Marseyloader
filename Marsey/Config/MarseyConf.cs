using Marsey.Game.Patches;
using Marsey.Game.Patches.Marseyports;
using Marsey.Misc;
using Marsey.Stealthsey;

namespace Marsey.Config;

/// <summary>
/// Variables used and changed at runtime
/// </summary>
public static class MarseyConf
{
    /// <summary>
    /// Defines how strict is Hidesey
    /// </summary>
    public static HideLevel MarseyHide = HideLevel.Normal;

    /// <summary>
    /// Log patcher output to separate file
    /// </summary>
    public static bool SeparateLogger;

    /// <summary>
    /// Should we log anything from the loader
    /// </summary>
    public static bool Logging;

    /// <summary>
    /// Log DEBG messages
    /// </summary>
    public static bool DebugAllowed;

    /// <summary>
    /// Log TRCE messages
    /// </summary>
    public static bool TraceAllowed;

    /// <summary>
    /// Throws an exception on client if any patch had failed applying.
    /// </summary>
    public static bool ThrowOnFail;

    /// <summary>
    /// Disable strict fork checks when applying resource packs.
    /// </summary>
    public static bool DisableResPackStrict;

    /// <see cref="HWID"/>
    public static bool ForceHWID;

    /// <see cref="DiscordRPC.Disable"/>
    public static bool KillRPC;

    /// <see cref="DiscordRPC.Fake"/>
    public static bool FakeRPC;

    /// <see cref="Marsey.Game.Resources.Dumper.Dumper"/>
    public static bool Dumper;

    /// <see cref="Jammer"/>
    public static bool JamDials;

    /// <see cref="Blackhole"/>
    public static bool DisableREC;

    /// <summary>
    /// Enables backports and fixes for the game
    /// </summary>
    /// <see cref="Marsey.Game.Patches.Marseyports.MarseyPortMan"/>
    public static bool Backports;

    /// <summary>
    /// Disable any backports
    /// </summary>
    public static bool DisableAnyBackports;

    /// <summary>
    /// Reflect changes made here to the Dictionary in the launcher's Connector.cs
    /// </summary>
    public static readonly Dictionary<string, Action<string>> EnvVarMap = new Dictionary<string, Action<string>>
    {
        { "MARSEY_LOGGING", value => Logging = value == "true" },
        { "MARSEY_LOADER_DEBUG", value => DebugAllowed = value == "true" },
        { "MARSEY_LOADER_TRACE", value => TraceAllowed = value == "true" },
        { "MARSEY_THROW_FAIL", value => ThrowOnFail = value == "true" },
        { "MARSEY_SEPARATE_LOGGER", value => SeparateLogger = value == "true" },
        { "MARSEY_DISABLE_STRICT", value => DisableResPackStrict = value == "true" },
        { "MARSEY_FORCINGHWID", value => ForceHWID = value == "true" },
        { "MARSEY_FORCEDHWID", value => HWID.SetHWID(value)},
        { "MARSEY_DISABLE_PRESENCE", value => KillRPC = value == "true" },
        { "MARSEY_FAKE_PRESENCE", value => FakeRPC = value == "true"},
        { "MARSEY_PRESENCE_USERNAME", value => DiscordRPC.SetUsername(value)},
        { "MARSEY_DUMP_ASSEMBLIES", value => Dumper = value == "true" },
        { "MARSEY_JAMMER", value => JamDials = value == "true" },
        { "MARSEY_DISABLE_REC", value => DisableREC = value == "true" },
        { "MARSEY_BACKPORTS", value => Backports = value == "true" },
        { "MARSEY_NO_ANY_BACKPORTS", value => DisableAnyBackports = value == "true" },
        { "MARSEY_FORKID", MarseyPortMan.SetForkID },
        { "MARSEY_ENGINE", MarseyPortMan.SetEngineVer },
        { "MARSEY_HIDE_LEVEL", value => MarseyHide = (HideLevel)Enum.Parse(typeof(HideLevel), value) }
    };

    // Conf variables that do not go into the EnvVarMap go here

    /// <summary>
    /// Wait for a debugger to attach in the loader process before executing anything
    /// </summary>
    public static bool JumpLoaderDebug;
}
