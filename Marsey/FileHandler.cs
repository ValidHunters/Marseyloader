using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using static Marsey.Marserializer.Marserializer;

namespace Marsey;

/// <summary>
/// Handles file operations in the patch folder
/// </summary>
public abstract class FileHandler
{
    /// <summary>
    ///  Move "Enabled" assemblies to the "Enabled" folder.
    /// </summary>
    public static void PrepAssemblies(string[]? path)
    {
        List<string> patches = PatchAssemblyManager.GetPatchList()
            .Where(p => p.Enabled)
            .Select(p => p.Asmpath)
            .ToList();
        
       Serialize(path, patches);
    }


    /// <summary>
    /// Loads assemblies from a specified folder.
    /// </summary>
    /// <param name="path">folder with patch dll's, set to "Marsey" by default</param>
    /// <param name="marserializer">Are we loading from marserializer</param>
    /// <param name="subverter">Is the initialized assembly a subverter</param>
    public static void LoadAssemblies(string[]? path = null, bool marserializer = false, bool subverter = false)
    {
        path ??= new[] { MarseyVars.MarseyPatchFolder };

        PatchAssemblyManager.RecheckPatches();

        List<string>? files = marserializer ? Deserialize(path) : GetPatches(path);

        if (files == null) return;
        
        foreach (string file in files)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(file);
                PatchAssemblyManager.InitAssembly(assembly, subverter);
            }
            catch (PatchAssemblyException ex)
            {
                Utility.Log(Utility.LogType.FATL, ex.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the file paths of all DLL files in a specified subdirectory
    /// </summary>
    /// <param name="subdir">An array of strings representing the path to the subdirectory</param>
    /// <returns>An array of strings containing the full paths to each DLL file in the specified subdirectory</returns>
    public static List<string> GetPatches(string[] subdir)
    {
        var updatedSubdir = subdir.Prepend(Directory.GetCurrentDirectory()).ToArray();
        string path = Path.Combine(updatedSubdir);

        return Directory.GetFiles(path, "*.dll").ToList();
    }
}
