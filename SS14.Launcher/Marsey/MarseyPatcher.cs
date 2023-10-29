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
        Type marseyPatchType = assembly.GetType("MarseyPatch") ?? throw new Exception("Loaded assembly does not have MarseyPatch type.");

        if (marseyPatchType.GetField("TargetAssembly") != null) throw new Exception($"{assembly.FullName} cannot be loaded because it uses an outdated patch!");

        List<FieldInfo> targets = GetPatchAssemblyFields(marseyPatchType) ?? throw new Exception($"Couldn't get assembly fields on {assembly.FullName}.");

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

    /// <summary>
    /// Obtains fields for each of the game's assemblies.
    /// Returns null if any of the fields is null.
    /// </summary>
    private static List<FieldInfo>? GetPatchAssemblyFields(Type marseyPatchType)
    {
        var fieldNames = new[] { "RobustClient", "RobustShared", "ContentClient", "ContentShared" };
        var targets = new List<FieldInfo>();

        foreach (var fieldName in fieldNames)
        {
            var field = marseyPatchType.GetField(fieldName);
            if (field == null) return null; // Not all fields could be caught, no point proceeding.
            targets.Add(field);
        }

        return targets;
    }

    /// <summary>
    ///  Move "enabled" assemblies to the "Enabled" folder.
    /// </summary>
    public static void PrepAssemblies()
    {
        string[] path = { "Marsey", "Enabled" };

        foreach (string file in GetPatches(path)) File.Delete(file);

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

    /// <summary>
    /// Loads assemblies from a specified (lie) folder.
    /// </summary>
    /// <param name="path">folder with patch dll's</param>
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
        int loops = 0;
        while (_robustSharedAss == null || _clientAss == null || _clientSharedAss == null && loops < 100)
        {
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
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

            loops++;
            Thread.Sleep(200);
        }

        Console.WriteLine(loops >= 100
            ? $"[MARSEY] Failed to receive assemblies within 20 seconds."
            : $"[MARSEY] Received assemblies.");
    }

    /// <summary>
    /// Starts (Boots) the patcher
    /// </summary>
    /// <param name="robClientAssembly">Robust.Client assembly provided by the Loader</param>
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
