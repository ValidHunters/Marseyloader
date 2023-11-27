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
    /// Marseypatches are saved to this one
    /// </summary>
    public const string MarseyPatchFolder = "Marsey";

    /// <summary>
    /// Subverter patches saved to this one
    /// </summary>
    public const string SubverterPatchFolder = "Subversion";

    /// <summary>
    /// Should code be side-loaded
    /// Defaults to false
    /// </summary>
    public static bool Subverter = false;
    
    /// <summary>
    /// Log DEBG messages
    /// <see cref="Utility.SetupFlags"/>
    /// </summary>
    public static bool DebugAllowed;

    /// <summary>
    /// Log messages sent from patches
    /// <see cref="Utility.SetupFlags"/>
    /// </summary>
    public static bool PatchLogAllowed;

    /// <summary>
    /// Throws an exception on client if any patch had failed applying.
    /// <see cref="Utility.SetupFlags"/>
    /// </summary>
    public static bool ThrowOnFail;
}
