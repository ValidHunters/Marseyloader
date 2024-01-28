using Marsey.Game.Resources.Dumper.Resource;

namespace Marsey.Game.Resources.Dumper;

/// <summary>
/// Dumps game content
/// </summary>
public static class MarseyDumper
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

        string loc = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        path = Path.Combine(loc, "Dumper/", $"{fork}/");
    }
    
    private static void Patch()
    {
        ResourceDumper.Patch();
    }
}