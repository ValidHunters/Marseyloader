using System.Reflection;
using Marsey.Stealthsey;

namespace Marsey.Game.Patches;

/// <summary>
/// Dumps game content
/// </summary>
public static class Dumper
{
    private static ResourceDumper _res = new ResourceDumper();
    public static string path = "marsey";
    
    public static void Start()
    {
        //Preclusion.Trigger("Dumper started.");
        GetExactPath();
        Patch();
    }
    
    private static void GetExactPath()
    {
        string fork = Environment.GetEnvironmentVariable("MARSEY_DUMP_FORKID") ?? "marsey";
        Envsey.CleanFlag(fork);

        string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        path = Path.Combine(loc, "Dumper/", $"{fork}/");
    }
    
    private static void Patch()
    {
        _res.Patch();
    }
}