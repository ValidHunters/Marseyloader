using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Serilog;
using HarmonyLib;

namespace SS14.Launcher.Marsey;

public class MarseyPatcher
{
    // Assemblinos
    private static Assembly RobustAss;
    private static Assembly ClientAss;
    private static Assembly RobustSharedAss;
    private static Assembly ClientSharedAss;

    private static List<Assembly> PatchAssemblies = new List<Assembly>();

    // Patcher
    private static Harmony harmony;



    public static void LoadAssemblies()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Marsey");
        foreach (string file in Directory.GetFiles(path, "*.dll"))
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(file);
                InitAssembly(assembly);

                Log.Debug($"Added {file}.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load assembly from {file}. Error: {ex.Message}");
            }
        }
    }

    public static void SetAssemblyTarget(FieldInfo required, FieldInfo target)
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

    public static void InitAssembly(Assembly assembly)
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

        PatchAssemblies.Add(assembly);
    }

    private static void PatchProc()
    {
        Console.WriteLine("[MARSEY] Patching.");
        foreach (Assembly ass in PatchAssemblies)
        {
            Console.WriteLine($"[MARSEY] Patching {ass.GetName()}");
            harmony.PatchAll(ass);
        }
    }

    public static void GetGameAssemblies()
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

    public static void Boot(Assembly robClientAssembly)
    {
        RobustAss = robClientAssembly;
        harmony = new Harmony("com.validhunters.marseypatcher");
        Harmony.DEBUG = true;

        GetGameAssemblies();
        LoadAssemblies();
        PatchProc();
    }
}
