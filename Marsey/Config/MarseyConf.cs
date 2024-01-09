using Marsey.GameAssembly.Patches;
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
    /// <see cref="Utility.SetupFlags"/>
    /// </summary>
    public static bool Logging;

    /// <summary>
    /// Log DEBG messages
    /// <see cref="Utility.SetupFlags"/>
    /// </summary>
    public static bool DebugAllowed;

    /// <summary>
    /// Throws an exception on client if any patch had failed applying.
    /// <see cref="Utility.SetupFlags"/>
    /// </summary>
    public static bool ThrowOnFail;
    
    /// <see cref="HWID"/>
    public static bool ForceHWID;


    /// <see cref="DiscordRPC"/>
    public static bool KillRPC;
    
    /// <see cref="Dumper"/>
    public static bool DumpAssemblies;
    
    /// <see cref="Jammer"/>
    public static bool JamDials;

    /// <see cref="Blackhole"/>
    public static bool DisableREC;

    public static readonly Dictionary<string, Action<bool>> EnvVarMap = new Dictionary<string, Action<bool>>
    {
        { "MARSEY_LOGGING", value => Logging = value },
        { "MARSEY_LOADER_DEBUG", value => DebugAllowed = value },
        { "MARSEY_THROW_FAIL", value => ThrowOnFail = value },
        { "MARSEY_SEPARATE_LOGGER", value => SeparateLogger = value },
        { "MARSEY_FORCINGHWID", value => ForceHWID = value },
        { "MARSEY_DISABLE_PRESENCE", value => KillRPC = value },
        { "MARSEY_DUMP_ASSEMBLIES", value => DumpAssemblies = value },
        { "MARSEY_JAMMER", value => JamDials = value },
        { "MARSEY_DISABLE_REC", value => DisableREC = value }
    };
}