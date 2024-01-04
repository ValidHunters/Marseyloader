using Marsey.Config;

namespace Marsey.Stealthsey;

public static class Abjure
{
    /// <summary>
    /// Checks against version with detection methods
    /// </summary>
    /// <returns>True if version is equal or over with detection and hidesey is disabled</returns>
    public static bool CheckMalbox(string engineversion, HideLevel MarseyHide)
    {
        Version reqVersion = new Version(engineversion);

        return reqVersion >= MarseyVars.Detection && MarseyHide == HideLevel.Disabled;
    }
}