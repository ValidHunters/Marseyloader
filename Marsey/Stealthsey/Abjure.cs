using Marsey.Config;

namespace Marsey.Stealthsey;

public static class Abjure
{
    private static Version? engineVer { get; set; } 
    
    /// <summary>
    /// Checks against version with detection methods
    /// </summary>
    /// <returns>True if version is equal or over with detection and hidesey is disabled</returns>
    public static bool CheckMalbox(string engineversion, HideLevel MarseyHide)
    {
        engineVer = new Version(engineversion);

        return engineVer >= MarseyVars.Detection && MarseyHide == HideLevel.Disabled;
    }
}