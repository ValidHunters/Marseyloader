using System.Reflection;
using HarmonyLib;
using Marsey.Game.ResourcePack.Reflection;

namespace Marsey.Game.ResourcePack;

public static class ResourceSwapper
{
    private static List<string> filepaths = [];
    
    public static void Start()
    {
        List<ResourcePack> RPacks = ResMan.GetRPacks();
        foreach (ResourcePack rpack in RPacks)
            PopulateFiles(rpack.Dir);
    }

    private static void PopulateFiles(string directory)
    {
        string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (file.EndsWith("meta.json")) continue;
            string relativePath = file.Replace(directory + Path.DirectorySeparatorChar, "");
            filepaths.Add(relativePath);
        }
    }
    
    public static List<string> ResourceFiles() => filepaths;
}