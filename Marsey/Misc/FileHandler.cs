using System.Reflection;
using Marsey.Config;
using Marsey.Game.Patches;
using Marsey.Game.Resources;
using Marsey.Game.Resources.Dumper.Resource;
using Marsey.PatchAssembly;
using Marsey.Patches;
using Marsey.Serializer;
using Marsey.Stealthsey;
using Marsey.Subversion;

namespace Marsey.Misc;

/// <summary>
/// Handles file operations in the patch folder
/// </summary>
public abstract class FileHandler
{
    /// <summary>
    /// <para>
    /// Serialize enabled mods.
    /// Executed by the launcher at connection.
    /// </para>
    /// 
    /// <para>
    /// We cant directly give marseys to the loader or tell it what mods to load because its started in a separate process.
    /// Because of this we leave an array of paths to assemblies to enabled mods for the loader to read.
    /// </para>
    /// </summary>
    public static void PrepareMods(string[]? path = null)
    {
        path ??= new[] { MarseyVars.MarseyFolder };
        string[] patchPath = new[] { MarseyVars.MarseyPatchFolder };
        string[] resPath = new[] { MarseyVars.MarseyResourceFolder };

        List<MarseyPatch> marseyPatches = Marsyfier.GetMarseyPatches();
        List<SubverterPatch> subverterPatches = Subverter.GetSubverterPatches();
        List<ResourcePack> resourcePacks = ResMan.GetRPacks();

        // Serialize preloading MarseyPatches
        List<string> preloadpaths = marseyPatches
            .Where(p => p is { Enabled: true, Preload: true })
            .Select(p => p.Asmpath)
            .ToList();
        
        Marserializer.Serialize(patchPath, Marsyfier.PreloadMarserializerFile, preloadpaths);
        
        // If we actually do have any - remove them from the marseypatch list
        if (preloadpaths.Count != 0)
            marseyPatches.RemoveAll(p => preloadpaths.Contains(p.Asmpath));

        // Serialize remaining MarseyPatches
        List<string> marseyAsmpaths = marseyPatches.Where(p => p.Enabled).Select(p => p.Asmpath).ToList();
        Marserializer.Serialize(patchPath, Marsyfier.MarserializerFile, marseyAsmpaths);

        // Serialize SubverterPatches
        List<string> subverterAsmpaths = subverterPatches.Where(p => p.Enabled).Select(p => p.Asmpath).ToList();
        Marserializer.Serialize(patchPath, Subverter.MarserializerFile, subverterAsmpaths);
        
        // Serialize ResourcePacks
        List<string> rpackPaths = resourcePacks.Where(rp => rp.Enabled).Select(rp => rp.Dir).ToList();
        Marserializer.Serialize(resPath, ResMan.MarserializerFile, rpackPaths);
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
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{file} could not be found");
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
            return [];
        }
        catch (Exception ex)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Failed to find patches: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Saves assembly from stream to file.
    /// </summary>
    public static void SaveAssembly(string path, string name, Stream asmStream)
    {
        Directory.CreateDirectory(path);
        
        string fullpath = Path.Combine(path, name);
        
        using FileStream st = new FileStream(fullpath, FileMode.Create, FileAccess.Write);
        asmStream.CopyTo(st);
    }
    
    /// <see cref="ResDumpPatches"/>
    public static void CheckRenameDirectory(string path)
    {
        // GetParent once shows itself, GetParent twice shows the actual parent
        
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
    
        string dirName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        string? parentDir = Directory.GetParent(Directory.GetParent(path)?.FullName ?? throw new InvalidOperationException())?.FullName;
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "FileHandler", $"Parentdir is {parentDir}");

        if (string.IsNullOrEmpty(dirName) || string.IsNullOrEmpty(parentDir)) return;

        string newPath = Path.Combine(parentDir, $"{dirName}{DateTime.Now:yyyyMMddHHmmss}");
        
        if (Directory.Exists(newPath))
        {
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, "FileHandler",
                $"Cannot move directory. Destination {newPath} already exists.");
            return;
        }

        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "FileHandler", $"Trying to move {path} to {newPath}");
        // Completely evil, do not try-catch this - if it fails - it fails and kills everything.
        Directory.Move(path, newPath);
    }
    
    /// <see cref="ResDumpPatches"/>
    public static void CreateDir(string filePath)
    {
        string? directoryName = Path.GetDirectoryName(filePath);
        if (directoryName != null && !Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
    }
    
    /// <see cref="ResDumpPatches"/>
    public static void SaveToFile(string filePath, MemoryStream stream)
    {
        using FileStream st = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Saving to {filePath}");
        stream.WriteTo(st);
    }
}
