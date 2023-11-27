using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using Marsey.Subversion;
using static Marsey.Marserializer.Marserializer;

namespace Marsey;

/// <summary>
/// Handles file operations in the patch folder
/// </summary>
public abstract class FileHandler
{
    /// <summary>
    /// Serialize enabled patches
    /// <param name="subverter">Load subverters</param>
    /// </summary>
    public static void PrepAssemblies(string[]? path, bool subverter = false)
    {
        List<MarseyPatch> patches = PatchAssemblyManager.GetPatchList(subverter);
        List<string> asmpaths = patches.Where(p => p.Enabled).Select(p => p.Asmpath).ToList();
        Serialize(path, asmpaths);
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

        if (subverter == false || marserializer == false)
            PatchAssemblyManager.RecheckPatches();
        
        List<string>? files = marserializer ? Deserialize(path) : GetPatches(path);
        if (files == null) return;
        
        foreach (string file in files)
        {
            LoadExactAssembly(file, subverter);
        }
    }

    /// <summary>
    /// Loads single assembly by name
    /// </summary>
    /// <param name="file">path to dll file</param>
    /// <param name="subverter">is it a subverter</param>
    public static void LoadExactAssembly(string file, bool subverter)
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
