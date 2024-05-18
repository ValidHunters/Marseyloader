using System.Reflection;
using Marsey.Config;
using Marsey.Game.Resources;
using Marsey.Game.Resources.Dumper.Resource;
using Marsey.PatchAssembly;
using Marsey.Patches;
using Marsey.Stealthsey;
using Marsey.Subversion;

namespace Marsey.Misc;

/// <summary>
/// Handles file operations in the patch folder
/// </summary>
public abstract class FileHandler
{
    /// <summary>
    /// Prepare data about enabled mods to send to the loader
    /// </summary>
public static async Task PrepareMods(string[]? path = null)
{
    path ??= new[] { MarseyVars.MarseyFolder };
    string[] patchPath = new[] { MarseyVars.MarseyPatchFolder };
    string[] resPath = new[] { MarseyVars.MarseyResourceFolder };

    List<MarseyPatch> marseyPatches = Marsyfier.GetMarseyPatches();
    List<SubverterPatch> subverterPatches = Subverter.GetSubverterPatches();
    List<ResourcePack> resourcePacks = ResMan.GetRPacks();

    IPC.Server server = new();

    // Prepare preloading MarseyPatches
    List<string> preloadpaths = marseyPatches
        .Where(p => p is { Enabled: true, Preload: true })
        .Select(p => p.Asmpath)
        .ToList();

    // Send preloading MarseyPatches through named pipe
    string preloadData = string.Join(",", preloadpaths);
    Task preloadTask = server.ReadySend("PreloadMarseyPatchesPipe", preloadData);

    // If we actually do have any - remove them from the marseypatch list
    if (preloadpaths.Count != 0)
    {
        marseyPatches.RemoveAll(p => preloadpaths.Contains(p.Asmpath));
    }

    // Prepare remaining MarseyPatches
    List<string> marseyAsmpaths = marseyPatches.Where(p => p.Enabled).Select(p => p.Asmpath).ToList();
    string marseyData = string.Join(",", marseyAsmpaths);
    Task marseyTask = server.ReadySend("MarseyPatchesPipe", marseyData);

    // Prepare SubverterPatches
    List<string> subverterAsmpaths = subverterPatches.Where(p => p.Enabled).Select(p => p.Asmpath).ToList();
    string subverterData = string.Join(",", subverterAsmpaths);
    Task subverterTask = server.ReadySend("SubverterPatchesPipe", subverterData);

#if DEBUG
    // Prepare ResourcePacks
    List<string> rpackPaths = resourcePacks.Where(rp => rp.Enabled).Select(rp => rp.Dir).ToList();
    string rpackData = string.Join(",", rpackPaths);
    Task resourceTask = server.ReadySend("ResourcePacksPipe", rpackData);

    // Wait for all tasks to complete
    await Task.WhenAll(preloadTask, marseyTask, subverterTask, resourceTask);
#else
    // Wait for all tasks to complete
    await Task.WhenAll(preloadTask, marseyTask, subverterTask);
#endif
}



    /// <summary>
    /// Loads assemblies from a specified folder.
    /// </summary>
    /// <param name="path">folder with patch dll's, set to "Marsey" by default</param>
    /// <param name="pipe">Are we loading from an IPC pipe</param>
    /// <param name="pipename">Name of an IPC pipe to load the patches from</param>
    public static void LoadAssemblies(string[]? path = null, bool pipe = false, string pipename = "MarseyPatchesPipe")
    {
        path ??= new[] { MarseyVars.MarseyPatchFolder };

        if (!pipe)
        {
            PatchListManager.RecheckPatches();
        }

        List<string> files = pipe ? GetFilesFromPipe(pipename) : GetPatches(path);

        foreach (string file in files)
        {
            LoadExactAssembly(file);
        }
    }

    /// <summary>
    /// Retrieve a list of patch filepaths from pipe
    /// </summary>
    /// <param name="name">Name of the pipe</param>
    public static List<string> GetFilesFromPipe(string name)
    {
        IPC.Client client = new();
        string data = client.ConnRecv(name);

        return string.IsNullOrEmpty(data) ? new List<string>() : data.Split(',').ToList();
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
