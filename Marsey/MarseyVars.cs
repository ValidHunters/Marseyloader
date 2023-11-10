namespace Marsey;

public abstract class MarseyVars
{

    /// <summary>
    ///
    /// Namespace identifier for Harmony
    /// </summary>
    public const string Identifier = "com.validhunters.marseyloader";

    /// <summary>
    /// Max amount of loops allowed to catch game assemblies
    /// </summary>
    public const int MaxLoops = 50;

    /// <summary>
    /// Cooldown to try the loop again, in ms
    /// </summary>
    public const int LoopCooldown = 200;

    /// <summary>
    /// Log DEBG messages
    /// <see cref="Utility.SetupLogFlags"/>
    /// </summary>
    public static bool DebugAllowed;

    /// <summary>
    /// Log messages sent from patches
    /// <see cref="Utility.SetupLogFlags"/>
    /// </summary>
    public static bool PatchLogAllowed;
}
