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

    /// <summary>
    /// Hook HWID
    /// <see cref="Marsey.Stealthsey.Game.HWID"/>
    /// </summary>
    public static bool ForceHWID;

    /// <summary>
    /// Hook Discord RPC
    /// </summary>
    public static bool KillRPC;
}