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
        Type marseyPatchType = assembly.GetType("MarseyPatch");
        FieldInfo TargAsm = marseyPatchType.GetField("TargetAssembly");

        if (TargAsm != null)
        {
            Console.WriteLine($"{assembly.FullName} cannot be loaded because it uses an outdated patch!");
            return;
        }

        if (marseyPatchType != null)
        {
            // Get all fields of the MarseyPatch type
            List<FieldInfo> targets = new List<FieldInfo>();
            targets.Add(marseyPatchType.GetField("RobustClient"));
            targets.Add(marseyPatchType.GetField("RobustShared"));
            targets.Add(marseyPatchType.GetField("ContentClient"));
            targets.Add(marseyPatchType.GetField("ContentShared"));

            SetAssemblyTargets(targets);

        }

        var patch = new MarseyPatch(assembly,
            (string)marseyPatchType.GetField("Name").GetValue(null),
            (string)marseyPatchType.GetField("Description").GetValue(null));

        foreach (MarseyPatch p in _patchAssemblies)
        {
            if (p.asm == assembly)
                return;
        }

        _patchAssemblies.Add(patch);
    }

    public static void PrepAssemblies()
    {
        string[] path = { "Marsey", "Enabled" };
        foreach (string file in GetPatches(path))
            File.Delete(file);

        foreach (var p in _patchAssemblies)
        {
            if (p.enabled)
            {
                string asmLocation = p.asm.Location;

                File.Copy(p.asm.Location,
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

    public static string[] GetPatches(string[] subdir)
    {
        subdir.Prepend(Directory.GetCurrentDirectory());
        string path = Path.Combine(subdir);

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
        if (GetPatches(new string[]{"Marsey"}).Length == _patchAssemblies.Count)
            return;

        _patchAssemblies = new List<MarseyPatch>();
    }

    /// <summary>
    /// Patches the game using assemblies in List.
    /// </summary>
    private static void PatchProc()
    {
        foreach (MarseyPatch p in _patchAssemblies)
        {
            Console.WriteLine($"[MARSEY] Patching {p.asm.GetName()}");
            _harmony.PatchAll(p.asm);
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
        while (_robustSharedAss == null || _clientAss == null || _clientSharedAss == null)
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var e in asms)
            {
                if (_robustSharedAss == null && e.FullName.Contains("Robust.Shared,"))
                {
                    _robustSharedAss = e;
                }
                else if (_clientAss == null && e.FullName.Contains("Content.Client,"))
                {
                    _clientAss = e;
                }
                else if (_clientSharedAss == null && e.FullName.Contains("Content.Shared,"))
                {
                    _clientSharedAss = e;
                }
            }
            Thread.Sleep(200);
            loops++;
        }
        Console.WriteLine($"[MARSEY] Received assemblies in {loops} loops.");
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
        LoadAssemblies(new string[]{"Marsey", "Enabled"});

        PatchProc();
    }
}

public class MarseyPatch
{
    public Assembly asm { get; set; }
    public string name { get; set; }
    public string desc { get; set; }
    public bool enabled { get; set; }

    public MarseyPatch(Assembly asm, string name, string desc)
    {
        this.asm = asm;
        this.name = name;
        this.desc = desc;
        this.enabled = false;
    }
}
