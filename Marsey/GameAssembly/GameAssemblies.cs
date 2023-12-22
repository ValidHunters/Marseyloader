using System.Reflection;

namespace Marsey.GameAssembly;

/// <summary>
/// Bagholds game assemblies
/// </summary>
public static class GameAssemblies
{
    private static Assembly? _robustClient;
    private static Assembly? _robustShared;
    private static Assembly? _contentClient;
    private static Assembly? _contentShared;

    public static void Initialize(Assembly RobustClient)
    {
        _robustClient = RobustClient;
    }

    public static void AssignAssemblies(Assembly? robustShared, Assembly? contentClient, Assembly? contentShared)
    {
        _robustShared = robustShared;
        _contentClient = contentClient;
        _contentShared = contentShared;
    }
    
    /// <summary>
    /// Checks if GameAssemblyManager has finished capturing assemblies
    /// </summary>
    /// <returns>True if any of the assemblies are filled</returns>
    public static bool ClientInitialized()
    {
        return _contentClient != null || _contentShared != null;
    }
    
    public static Assembly? RobustClient => _robustClient;
    public static Assembly? RobustShared => _robustShared;
    public static Assembly? ContentClient => _contentClient;
    public static Assembly? ContentShared => _contentShared;
}