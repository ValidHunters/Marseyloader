using System.Reflection;
using Marsey.Game.ResourcePack;

namespace Marsey.Game.Patches;

/// <summary>
/// Dumps game content
/// </summary>
public static class Dumper
{
    public static string path = "marsey";
    
    public static void Start()
    {
        GetExactPath();
        Patch();
    }
    
    private static void GetExactPath()
    {
        string fork = ResMan.GetForkID() ?? "marsey";

        string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        path = Path.Combine(loc, "Dumper/", $"{fork}/");
    }
    
    private static void Patch()
    {
        ResourceDumper.Patch();
    }
}