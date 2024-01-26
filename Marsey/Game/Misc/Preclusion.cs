using Marsey.Misc;

namespace Marsey.Game.Misc;

/// <summary>
/// Closes the game right before content pack is started.
/// EntryPoint of the content pack is assumed to be the start.
/// <seealso cref="Game.Patches.Sentry"/>
/// </summary>
public static class Preclusion
{
    private static bool _flag = false;

    /// <summary>
    /// Signal to marseyloader to close the game before the game starts
    /// </summary>
    public static void Trigger(string reason)
    {
        if (_flag)
        {
            MarseyLogger.Log(MarseyLogger.LogType.INFO, "Preclusion", $"Preclusion was triggered more than once, reason: {reason}");
            return;
        }
        
        MarseyLogger.Log(MarseyLogger.LogType.INFO, "Preclusion", $"Stopping content boot, reason: {reason}");
        _flag = true;
    }

    public static bool State => _flag;
    
    public static void Fire()
    {
        Environment.Exit(0);
    }
}