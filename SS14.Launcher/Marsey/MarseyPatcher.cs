using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;

namespace SS14.Launcher.Marsey;

public class MarseyPatcher
{
    // Assemblinos
    private static Assembly? _robustAss;
    private static Assembly? _clientAss;
    private static Assembly? _robustSharedAss;
    private static Assembly? _clientSharedAss;

    private static List<MarseyPatch> _patchAssemblies = new List<MarseyPatch>();

    // Patcher
    private static Harmony? _harmony;

    private static void InitAssembly(Assembly assembly)
    {
        Type? marseyPatchType = assembly.GetType("MarseyPatch");

        if (marseyPatchType == null)
            return;

        FieldInfo? targAsm = marseyPatchType.GetField("TargetAssembly");

        if (targAsm != null)
        {
            Console.WriteLine($"{assembly.FullName} cannot be loaded because it uses an outdated patch!");
            return;
        }

         // Get all fields of the MarseyPatch type
         List<FieldInfo>? targets = GetPatchAssemblyFields(marseyPatchType);

         if (targets == null)
         {
             Console.WriteLine($"Couldn't get patchassembly fields on {assembly.FullName}");
             return;
         }

         SetAssemblyTargets(targets);


         FieldInfo? nameField = marseyPatchType.GetField("Name");
         FieldInfo? descriptionField = marseyPatchType.GetField("Description");

         string name = nameField != null && nameField.GetValue(null) is string nameValue ? nameValue : "Unknown";
         string description = descriptionField != null && descriptionField.GetValue(null) is string descriptionValue ? descriptionValue : "Unknown";

         var patch = new MarseyPatch(assembly, name, description);

         foreach (MarseyPatch p in _patchAssemblies)
         {
            if (p.Asm == assembly)
                return;
         }

        _patchAssemblies.Add(patch);
    }

    private static List<FieldInfo>? GetPatchAssemblyFields(Type marseyPatchType)
    {
        List<FieldInfo> targets = new List<FieldInfo>();
        FieldInfo? robustClientField = marseyPatchType.GetField("RobustClient");
        FieldInfo? robustSharedField = marseyPatchType.GetField("RobustShared");
        FieldInfo? contentClientField = marseyPatchType.GetField("ContentClient");
        FieldInfo? contentSharedField = marseyPatchType.GetField("ContentShared");

        if (robustClientField != null && robustSharedField != null && contentClientField != null && contentSharedField != null)
        {
            targets.Add(robustClientField);
            targets.Add(robustSharedField);
            targets.Add(contentClientField);
            targets.Add(contentSharedField);
        }
        else
        {
            return null;
        }

        return targets;
    }

    public static void PrepAssemblies()
    {
        string[] path = { "Marsey", "Enabled" };
        foreach (string file in GetPatches(path))
            File.Delete(file);

        foreach (var p in _patchAssemblies)
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

    public static void LoadAssemblies(string[]? path = null)
    {
        path ??= new[] { "Marsey" };

        RecheckPatches();

        var files = GetPatches(path);
        foreach (string file in files)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(file);
                InitAssembly(assembly);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load assembly from {file}. Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Sets the assembly target in the patch assembly.
    /// </summary>
    /// <param name="targets">Array of assemblies from the MarseyPatch class</param>
    private static void SetAssemblyTargets(List<FieldInfo> targets)
    {
        targets[0].SetValue(null, _robustAss);
        targets[1].SetValue(null,_robustSharedAss);
        targets[2].SetValue(null,_clientAss);
        targets[3].SetValue(null,_clientSharedAss);
    }

    private static string[] GetPatches(string[] subdir)
    {
        var updatedSubdir = subdir.Prepend(Directory.GetCurrentDirectory()).ToArray();
        string path = Path.Combine(updatedSubdir);

        return Directory.GetFiles(path, "*.dll");
    }

    /// <returns>Patch list</returns>
    public static List<MarseyPatch> GetPatchList()
    {
        return _patchAssemblies;
    }

    /// <summary>
    /// Checks if the amount of patches in folder equals the amount of patches in list.
    /// If not - resets the list.
    /// </summary>
    private static void RecheckPatches()
    {
        if (GetPatches(new []{"Marsey"}).Length == _patchAssemblies.Count)
            return;

        _patchAssemblies = new List<MarseyPatch>();
    }

    /// <summary>
    /// Patches the game using assemblies in List.
    /// </summary>
    private static void PatchProc()
    {
        if (_harmony != null)
        {
            foreach (MarseyPatch p in _patchAssemblies)
            {
                Console.WriteLine($"[MARSEY] Patching {p.Asm.GetName()}");
                _harmony.PatchAll(p.Asm);
            }
        }
    }

    /// <summary>
    /// Obtains game assemblies
    /// The function ends only when Robust.Shared,
    /// Content.Client and Content.Shared are initialized by the game.
    /// Executed only by the Loader.
    /// </summary>
    private static void GetGameAssemblies()
    {
        while (_robustSharedAss == null || _clientAss == null || _clientSharedAss == null)
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var e in asms)
            {
                string? fullName = e.FullName;
                if (fullName != null)
                {
                    if (_robustSharedAss == null && fullName.Contains("Robust.Shared,"))
                    {
                        _robustSharedAss = e;
                    }
                    else if (_clientAss == null && fullName.Contains("Content.Client,"))
                    {
                        _clientAss = e;
                    }
                    else if (_clientSharedAss == null && fullName.Contains("Content.Shared,"))
                    {
                        _clientSharedAss = e;
                    }
                }
            }

            Thread.Sleep(200);
        }
        Console.WriteLine($"[MARSEY] Received assemblies.");
    }

    /// <summary>
    /// Starts (Boots) the patcher
    /// </summary>
    /// <param name="robClientAssembly">Robust.Client assembly provded by the Loader</param>
    public static void Boot(Assembly? robClientAssembly)
    {
        _robustAss = robClientAssembly;
        _harmony = new Harmony("com.validhunters.marseypatcher");

        GetGameAssemblies();
        LoadAssemblies(new []{"Marsey", "Enabled"});

        PatchProc();
    }
}

public class MarseyPatch
{
    public Assembly Asm { get; set; }
    public string Name { get; set; }
    public string Desc { get; set; }
    public bool Enabled { get; set; }

    public MarseyPatch(Assembly asm, string name, string desc)
    {
        this.Asm = asm;
        this.Name = name;
        this.Desc = desc;
        this.Enabled = false;
    }
}
