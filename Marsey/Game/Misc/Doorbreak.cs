using System.Reflection;

namespace Marsey.Game.Misc;

/// <summary>
/// Manages MarseyEntry
/// </summary>
public static class Doorbreak
{
    /// <summary>
    /// Invokes MarseyEntry
    /// </summary>
    /// <param name="entry">MethodInfo of MarseyEntry::Entry()</param>
    /// <param name="threading">Call in another thread</param>
    public static void Enter(MethodInfo? entry, bool threading = true)
    {
        if (entry == null) return;

        if (threading)
            new Thread(() => { entry.Invoke(null, []); }).Start();
        else
            entry.Invoke(null, []);
    }
}
