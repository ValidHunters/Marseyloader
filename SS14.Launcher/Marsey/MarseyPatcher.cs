using System;
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

        harmony = new Harmony("com.validhunters.marseypatcher");
        ThreadProc();
    }
}

[HarmonyPatch]
public static class MyPatcher
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
