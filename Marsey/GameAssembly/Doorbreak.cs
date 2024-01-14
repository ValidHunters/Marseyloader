using System.Reflection;

namespace Marsey.GameAssembly;

/// <summary>
/// Manages MarseyEntry
/// </summary>
public static class Doorbreak
{
    /// <summary>
    /// Invokes MarseyEntry
    /// </summary>
    /// <param name="entry">MethodInfo of MarseyEntry::Entry()</param>
    public static void Enter(MethodInfo? entry)
    {
        if (entry == null) return;
        
        Thread entryThread = new Thread(() =>
        {
            entry.Invoke(null, new object[] {});
        });
        
        entryThread.Start();
    }
}