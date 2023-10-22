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
    public static Assembly RobustAss;
    private static Assembly ClientAss;

    private static Harmony harmony;

    public static List<Assembly> Assemblies = new List<Assembly>();

    public static void LoadAssemblies()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Marsey");
        foreach (string file in Directory.GetFiles(path, "*.dll"))
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(file);
                Assemblies.Add(assembly);

                var marseyPatchType = assembly.GetType("MarseyPatch");

                if (marseyPatchType != null)
                {
                    // Get all fields of the MarseyPatch type
                    var reqAsm = marseyPatchType.GetField("ReqAsm");
                    var Asm = marseyPatchType.GetField("TargetAssembly");

                    if (reqAsm != null && Asm != null)
                    {
                        string reqAsmVal = (string)reqAsm.GetValue(null);
                        if (reqAsmVal == "RC")
                        {
                            Asm.SetValue(null, RobustAss);
                        }
                    }
                }

                Log.Debug($"Added {file}.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load assembly from {file}. Error: {ex.Message}");
            }
        }
    }



    private static void ThreadProc()
    {
        bool loaded = false;
        Log.Debug("Loaded into threadproc");
        Thread t = new Thread(new ThreadStart(ThreadProc2));
        t.Start();
        return;
        /*
        while (true)
        {
            Thread.Sleep(2500);
            Assembly[] assemblies;
            foreach(var e in assemblies)
            {

                if (e.FullName.Contains("Content.Client,") && loaded == true)
                {
                    ClientAss = e;

                }
            }
        }*/
    }

    private static void ThreadProc2()
    {
        Console.WriteLine("[MARSEY] Got robust assembly, waiting 5 seconds to start a patch");
        Thread.Sleep(5000);
        Console.WriteLine("[Marsey] Patching.");
        harmony.PatchAll();

    }

    public static void Boot(Assembly robCliAssembly)
    {
        RobustAss = robCliAssembly;
        LoadAssemblies();
        harmony = new Harmony("com.validhunters.marseypatcher");
        ThreadProc();
    }
}

[HarmonyPatch]
public static class DODPatch
{
    // The method to be patched
    private static MethodBase TargetMethod()
    {
        var tp = MarseyPatcher.RobustAss.GetType("Robust.Client.Graphics.Clyde.Clyde");
        return tp.GetMethod("DrawOcclusionDepth", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    // Prefix that will be executed before the original method
    [HarmonyPrefix]
    private static bool PrefSkip()
    {
        return false;
    }
}
