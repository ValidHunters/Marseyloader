using System.Reflection;
using Marsey.Misc;
using Marsey.Stealthsey;

namespace Marsey.Game.Patches;

static class AsmDumpPatches
{
    private static short _counter;
    
    /// <summary>
    /// Intercept assemblies, save to disk
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private static bool LGAPrefix(ref Stream assembly)
    {
        FileHandler.SaveAssembly(Dumper.path, $"{_counter}.dll", assembly);
        _counter++;

        return true;
    }

    /// <summary>
    /// Assemblies finished loading, close the game
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private static void TLMPostfix()
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Dumper", $"Dumps saved to {Dumper.path}");
        MarseyLogger.Log(MarseyLogger.LogType.INFO, "Dumper", $"Exited with {_counter} assemblies.");
        Environment.Exit(0);
    }

}