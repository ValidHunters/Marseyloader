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
    private static Assembly RobustAss;
    private static Assembly ClientAss;
    private static Assembly RobustSharedAss;
    private static Assembly ClientSharedAss;

    private static List<Patch> PatchAssemblies = new List<Patch>();

    // Patcher
    private static Harmony harmony;

    public static void EnsureDirectoriesExist()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        string marseyPath = Path.Combine(currentDirectory, "Marsey");
        if (!Directory.Exists(marseyPath))
        {
            Directory.CreateDirectory(marseyPath);
        }

        string enabledPath = Path.Combine(marseyPath, "Enabled");
        if (!Directory.Exists(enabledPath))
        {
            Directory.CreateDirectory(enabledPath);
        }
    }
    public static void PrepAssemblies()
    {
        string[] path = { "Marsey", "Enabled" };
        foreach (string file in GetPatches(path))
            File.Delete(file);

        foreach (Patch p in PatchAssemblies)
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

    public static string[] GetPatches(string[] subdir)
    {
        subdir.Prepend(Directory.GetCurrentDirectory());
        string path = Path.Combine(subdir);

        return Directory.GetFiles(path, "*.dll");
    }

    public static void LoadAssemblies(string[] path = null)
    {
        path ??= new[] { "Marsey" };

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

    private static void SetAssemblyTarget(FieldInfo required, FieldInfo target)
    {
        string reqType = (string)required.GetValue(null);

        switch (reqType)
        {
            case "RC":
                target.SetValue(null, RobustAss);
                break;
            case "RS":
                target.SetValue(null, RobustSharedAss);
                break;
            case "CC":
                target.SetValue(null, ClientAss);
                break;
            case "CS":
                target.SetValue(null, ClientAss);
                break;
        }
    }

    private static void InitAssembly(Assembly assembly)
    {
        var marseyPatchType = assembly.GetType("MarseyPatch");

        if (marseyPatchType != null)
        {
            // Get all fields of the MarseyPatch type
            var reqAsm = marseyPatchType.GetField("ReqAsm");
            var targAsm = marseyPatchType.GetField("TargetAssembly");

            if (reqAsm != null && targAsm != null)
            {
                SetAssemblyTarget(reqAsm, targAsm);
            }
        }
        var Patch = new Patch(assembly, (string)marseyPatchType.GetField("Name").GetValue(null), (string)marseyPatchType.GetField("Description").GetValue(null));

        foreach (Patch p in PatchAssemblies)
        {
            if (p.asm == assembly)
                return;
        }

        PatchAssemblies.Add(Patch);
    }

    private static void PatchProc()
    {
        foreach (Patch p in PatchAssemblies)
        {
            Console.WriteLine($"[MARSEY] Patching {p.asm.GetName()}");
            harmony.PatchAll(p.asm);
        }
    }

    private static void GetGameAssemblies()
    {
        int loops = 0;
        while (RobustSharedAss == null || ClientAss == null || ClientSharedAss == null)
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var e in asms)
            {
                if (RobustSharedAss == null && e.FullName.Contains("Robust.Shared,"))
                {
                    RobustSharedAss = e;
                }
                else if (ClientAss == null && e.FullName.Contains("Content.Client,"))
                {
                    ClientAss = e;
                }
                else if (ClientSharedAss == null && e.FullName.Contains("Content.Shared,"))
                {
                    ClientSharedAss = e;
                }
            }
            Thread.Sleep(100);
            loops++;
        }
        Console.WriteLine($"[MARSEY] Received assemblies in {loops} loops.");
    }

    public static List<Patch> GetPatchList()
    {
        return PatchAssemblies;
    }

    public static void Boot(Assembly robClientAssembly)
    {
        RobustAss = robClientAssembly;
        harmony = new Harmony("com.validhunters.marseypatcher");

        GetGameAssemblies();
        LoadAssemblies(new string[]{"Marsey", "Enabled"});

        PatchProc();
    }
}

public class Patch
{
    public Assembly asm { get; set; }
    public string name { get; set; }
    public string desc { get; set; }
    public bool enabled { get; set; }

    public Patch(Assembly asm, string name, string desc)
    {
        this.asm = asm;
        this.name = name;
        this.desc = desc;
        this.enabled = false;
    }
}
