using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Marsey.Config;
using Marsey.PatchAssembly;
using Marsey.Patches;
using Marsey.Serializer;
using Marsey.Stealthsey;
using Marsey.Subversion;

namespace Marsey.Utility;

/// <summary>
/// Handles file operations in the patch folder
/// </summary>
public abstract class FileHandler
{
    /// <summary>
    /// Serialize enabled patches.
    /// </summary>
    public static void PrepAssemblies(string[]? path = null)
    {
        path ??= new[] { MarseyVars.MarseyPatchFolder };

        List<MarseyPatch> marseyPatches = Marsyfier.GetMarseyPatches();
        List<SubverterPatch> subverterPatches = Subverter.GetSubverterPatches();

        // Serialize preloading MarseyPatches
        List<string> preloadpaths = marseyPatches
            .Where(p => p.Enabled && p.Preload)
            .Select(p => p.Asmpath)
            .ToList();
        
        Marserializer.Serialize(path, Marsyfier.PreloadMarserializerFile, preloadpaths);
        
        // If we actually do have any - remove them from the marseypatch list
        if (preloadpaths.Any())
            marseyPatches.RemoveAll(p => preloadpaths.Contains(p.Asmpath));

        // Serialize remaining MarseyPatches
        List<string> marseyAsmpaths = marseyPatches.Where(p => p.Enabled).Select(p => p.Asmpath).ToList();
        Marserializer.Serialize(path, Marsyfier.MarserializerFile, marseyAsmpaths);

        // Serialize SubverterPatches
        List<string> subverterAsmpaths = subverterPatches.Where(p => p.Enabled).Select(p => p.Asmpath).ToList();
        Marserializer.Serialize(path, Subverter.MarserializerFile, subverterAsmpaths);
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

        if (!marserializer)
        {
            PatchListManager.RecheckPatches();
        }

        List<string> files = marserializer && filename != null
            ? Marserializer.Deserialize(path, filename) ?? new List<string>()
            : GetPatches(path);

        foreach (string file in files)
        {
            LoadExactAssembly(file);
        }
    }

    /// <summary>
    /// Loads an assembly from the specified file path and initializes it.
    /// </summary>
    /// <param name="file">The file path of the assembly to load.</param>
    public static void LoadExactAssembly(string file)
    {
        Redial.Disable(); // Disable any AssemblyLoad callbacks found

        try
        {
            Assembly assembly = Assembly.LoadFrom(file);
            AssemblyInitializer.Initialize(assembly);
        }
        catch (FileNotFoundException)
        {
            if (!file.EndsWith(Subverse.SubverterFile))
            {
                MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{file} could not be found");
            }
        }
        catch (PatchAssemblyException ex)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, ex.Message);
        }
        catch (Exception ex) // Catch any other exceptions that may occur
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"An unexpected error occurred while loading {file}: {ex.Message}");
        }
        finally
        {
            Redial.Enable(); // Enable callbacks in case the game needs them
        }
    }


    /// <summary>
    /// Retrieves the file paths of all DLL files in a specified subdirectory
    /// </summary>
    /// <param name="subdir">An array of strings representing the path to the subdirectory</param>
    /// <returns>An array of strings containing the full paths to each DLL file in the specified subdirectory</returns>
    public static List<string> GetPatches(string[] subdir)
    {
        try
        {
            string[] updatedSubdir = subdir.Prepend(Directory.GetCurrentDirectory()).ToArray();
            string path = Path.Combine(updatedSubdir);

            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.dll").ToList();
            }
            
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Directory {path} does not exist");
            return new List<string>();
        }
        catch (Exception ex)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Failed to find patches: {ex.Message}");
            return new List<string>();
        }
    }
}
