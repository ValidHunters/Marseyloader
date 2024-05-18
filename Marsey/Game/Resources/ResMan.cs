using Marsey.Config;
using Marsey.Game.Patches.Marseyports;
using Marsey.Game.Resources.Dumper;
using Marsey.Game.Resources.Reflection;
using Marsey.Misc;

namespace Marsey.Game.Resources;

public static class ResMan
{
    private static readonly List<ResourcePack> _resourcePacks = [];
    private static string? _fork;

    /// <summary>
    /// Executed by the loader
    /// </summary>
    public static void Initialize()
    {
        ResourceTypes.Initialize();

        _fork = MarseyPortMan.fork; // Peak spaghet moment

        // If we're dumping the game we don't want to dump our own respack now would we
        if (MarseyConf.Dumper)
        {
            MarseyDumper.Start();
            return;
        }

#if DEBUG
        // Retrieve enabled resource packs data through named pipe
        List<string> enabledPacks = FileHandler.GetFilesFromPipe("ResourcePacksPipe");

        if (enabledPacks.Count == 0) return;

        MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Detecting {enabledPacks.Count} enabled resource packs.");

        foreach (string dir in enabledPacks)
        {
            InitializeRPack(dir, !MarseyConf.DisableResPackStrict);
        }

        ResourceSwapper.Start();
#endif
    }


    /// <summary>
    /// Executed by the launcher
    /// </summary>
    public static void LoadDir()
    {
        _resourcePacks.Clear();
        string[] subDirs = Directory.GetDirectories(MarseyVars.MarseyResourceFolder);
        foreach (string subdir in subDirs)
        {
            InitializeRPack(subdir);
        }
    }

    /// <summary>
    /// Creates a ResourcePack object from a given path to a directory
    /// </summary>
    /// <param name="path">resource pack directory</param>
    /// <param name="strict">match fork id</param>
    private static void InitializeRPack(string path, bool strict = false)
    {
        ResourcePack rpack = new ResourcePack(path);

        try
        {
            rpack.ParseMeta();
        }
        catch (RPackException e)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, e.ToString());
            return;
        }

        AddRPack(rpack, strict);
    }

    private static void AddRPack(ResourcePack rpack, bool strict)
    {
        if (_resourcePacks.Any(rp => rp.Dir == rpack.Dir)) return;
        if (strict && rpack.Target != _fork && rpack.Target != "") return;

        _resourcePacks.Add(rpack);
    }

    public static List<ResourcePack> GetRPacks() => _resourcePacks;
    public static string? GetForkID() => _fork;
}
