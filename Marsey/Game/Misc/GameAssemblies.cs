using System.Reflection;
using Marsey.Game.Managers;

namespace Marsey.Game.Misc;

/// <summary>
/// Bagholds game assemblies
/// </summary>
public static class GameAssemblies
{
    public static void Initialize(Assembly? robustClient)
    {
        RobustClient = robustClient;
        RobustShared = GameAssemblyManager.GetSharedEngineAssembly();
    }

    public static void AssignContentAssemblies(Assembly? contentClient, Assembly? contentShared)
    {
        ContentClient = contentClient;
        ContentShared = contentShared;
    }
    
    /// <summary>
    /// Checks if GameAssemblyManager has finished capturing assemblies
    /// </summary>
    /// <returns>True if any of the assemblies are filled</returns>
    public static bool ClientInitialized()
    {
        return ContentClient != null || ContentShared != null;
    }
    
    public static Assembly? RobustClient { get; private set; }

    public static Assembly? RobustShared { get; private set; }

    public static Assembly? ContentClient { get; private set; }

    public static Assembly? ContentShared { get; private set; }
}