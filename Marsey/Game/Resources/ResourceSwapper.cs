using System.Reflection;
using HarmonyLib;
using Marsey.Game.Resources.Reflection;
using Marsey.Handbreak;
using Marsey.Misc;

namespace Marsey.Game.Resources;

public static class ResourceSwapper
{
    private static List<string> filepaths = [];
    
    public static void Start()
    {
        List<ResourcePack> RPacks = ResMan.GetRPacks();
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Starting with {RPacks.Count} RPacks");
        foreach (ResourcePack rpack in RPacks)
            PopulateFiles(rpack.Dir);
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Patching with {filepaths.Count} file replacements");
        Patch();
    }

    private static void PopulateFiles(string directory)
    {
        string absoluteDirectory = Path.GetFullPath(directory);
        string[] files = Directory.GetFiles(absoluteDirectory, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (file.EndsWith("meta.json")) continue;
            filepaths.Add(file);
        }
    }


    private static void Patch()
    {
        if (ResourceTypes.ResPath == null) return;
        
        Helpers.PatchMethod(
            targetType: ResourceTypes.ProtoMan,
            targetMethodName: "ContentFindFiles",
            patchType: typeof(ResourcePatches),
            patchMethodName: "SwapCFFRes",
            patchingType: HarmonyPatchType.Postfix,
            new Type[] { ResourceTypes.ResPath }
        );
    }
    
    public static List<string> ResourceFiles() => filepaths;
}