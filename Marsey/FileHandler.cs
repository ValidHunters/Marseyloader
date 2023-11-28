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
    /// </summary>
    public static void PrepAssemblies<T>(string[]? path) where T : IPatch
    {
        List<T>? patches = PatchListManager.GetPatchList<T>();
        string filename;

        if (typeof(T) == typeof(MarseyPatch))
            filename = PatchListManager.SerializerFile;
        else if (typeof(T) == typeof(SubverterPatch))
            filename = Subverter.SerializerFile;
        else
            throw new ArgumentException("Unsupported patch type");
        
        if (patches != null)
        {
            List<string> asmpaths = patches.Where(p => p.Enabled).Select(p => p.Asmpath).ToList();
            Serialize(path, filename, asmpaths);
        }
    }

    /// <summary>
    /// Loads assemblies from a specified folder.
    /// </summary>
    /// <param name="path">folder with patch dll's, set to "Marsey" by default</param>
    /// <param name="marserializer">Are we loading from marserializer</param>
    /// <param name="filename">Marserialized json filename</param>
    public static void LoadAssemblies(string[]? path = null, bool marserializer = false, string? filename = null)
    {
        path ??= new[] { MarseyVars.MarseyPatchFolder };

        if (marserializer == false)
            PatchListManager.RecheckPatches();

        if (marserializer && filename == null) return;

        List<string>? files = marserializer ? Deserialize(path, filename) : GetPatches(path);
        if (files == null) return;

        foreach (string file in files)
        {
            LoadExactAssembly(file);
        }
    }

    /// <summary>
    /// Loads single assembly by name
    /// </summary>
    /// <param name="file">path to dll file</param>
    public static void LoadExactAssembly(string file)
    {
        try
        {
            Assembly assembly = Assembly.LoadFrom(file);
            AssemblyInitializer.Initialize(assembly);
        }
        catch (FileNotFoundException)
        {
            if (file.EndsWith("Subverter.dll")) return;
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{file} could not be found");
        }
        catch (PatchAssemblyException ex)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, ex.Message);
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
