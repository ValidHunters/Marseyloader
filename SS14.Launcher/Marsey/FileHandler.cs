using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SS14.Launcher.Marsey;

public class FileHandler
{
    /// <summary>
    ///  Move "Enabled" assemblies to the "Enabled" folder.
    /// </summary>
    public static void PrepAssemblies()
    {
        string[] path = { "Marsey", "Enabled" };

        foreach (string file in GetPatches(path)) File.Delete(file);

        foreach (var p in PatchAssemblyManager.GetPatchList())
        {
            if (p.Enabled)
            {
                string asmLocation = p.Asm.Location;

                File.Copy(p.Asm.Location,
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "Marsey",
                        "Enabled",
                        Path.GetFileName(asmLocation)), true );
            }

        }
    }

    /// <summary>
    /// Loads assemblies from a specified (lie) folder.
    /// </summary>
    /// <param name="path">folder with patch dll's</param>
    public static void LoadAssemblies(string[]? path = null)
    {
        path ??= new[] { "Marsey" };

        PatchAssemblyManager.RecheckPatches();

        var files = GetPatches(path);
        foreach (string file in files)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(file);
                PatchAssemblyManager.InitAssembly(assembly);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load assembly from {file}. Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Retrieves the file paths of all DLL files in a specified subdirectory
    /// </summary>
    /// <param name="subdir">An array of strings representing the path to the subdirectory</param>
    /// <returns>An array of strings containing the full paths to each DLL file in the specified subdirectory</returns>
    public static string[] GetPatches(string[] subdir)
    {
        var updatedSubdir = subdir.Prepend(Directory.GetCurrentDirectory()).ToArray();
        string path = Path.Combine(updatedSubdir);

        return Directory.GetFiles(path, "*.dll");
    }
}
