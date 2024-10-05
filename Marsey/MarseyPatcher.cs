using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.Game;
using Marsey.Game.Managers;
using Marsey.Game.Misc;
using Marsey.Game.Patches;
using Marsey.Game.Patches.Marseyports;
using Marsey.Game.Resources;
using Marsey.Game.Resources.Dumper;
using Marsey.PatchAssembly;
using Marsey.Patches;
using Marsey.Stealthsey;
using Marsey.Subversion;
using Marsey.Misc;
using Marsey.Stealthsey.Reflection;

namespace Marsey;

/// <summary>
/// Entrypoint for patching
/// </summary>
public class MarseyPatcher
{
    private static MarseyPatcher? _instance;
    private static ManualResetEvent? _flag;

    public static MarseyPatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new Exception("MarseyPatcher is not created. Call CreateInstance with the client assembly first.");
            }
            return _instance;
        }
    }

    public static void CreateInstance(Assembly? robClientAssembly, ManualResetEvent mre)
    {
        if (_instance != null)
        {
            throw new Exception("Instance already created.");
        }

        _instance = new MarseyPatcher(robClientAssembly, mre);
    }

    /// <exception cref="Exception">Excepts if Robust.Client assembly is null</exception>
    private MarseyPatcher(Assembly? robClientAssembly, ManualResetEvent mre)
    {
        if (robClientAssembly == null) throw new Exception("Robust.Client was null.");

        _flag = mre;

        // Initialize GameAssemblies
        GameAssemblies.Initialize(robClientAssembly);

        // Initialize loader
        //Utility.SetupFlags();
        Utility.ReadConf();
        HarmonyManager.Init(new Harmony(MarseyVars.Identifier));

        MarseyLogger.Log(MarseyLogger.LogType.INFO, $"Marseyloader started{(MarseyConf.Patchless ? " in patchless mode" : "")}, version {MarseyVars.MarseyVersion}");

        // Init backport manager
        MarseyPortMan.Initialize();

        // Hide the loader
        Hidesey.Initialize();

        Preload();

        // Tell the loader were done here, start the game
        _flag.Set();
    }

    // We might want to patch things before the loader has even a chance to execute anything
    [Patching]
    private void Preload()
    {
        Sentry.Patch();

        // Preload marseypatches, if available
        Marsyfier.Preload();

        // If set - Disable redialing and remote command execution
        Jammer.Patch();
        Blackhole.Patch();

        // Apply engine backports
        MarseyPortMan.PatchBackports();

        // Start Resource Manager
        ResMan.Initialize();
    }

    /// <summary>
    /// Boots up the patcher
    /// Executed by the loader.
    /// </summary>
    public void Boot()
    {
        // Side-load custom code
        if (Subverse.CheckSubversions())
            Subverse.PatchSubverter();

        // Wait for the game itself to load
        GameAssemblyManager.TrySetContentAssemblies();

        // Post assembly-load hide methods
        Hidesey.PostLoad();

        ExecPatcher();

        Afterparty();
    }

    private void ExecPatcher()
    {
        // Prepare marseypatches
        FileHandler.LoadAssemblies(pipe: true);
        List<MarseyPatch> patches = Marsyfier.GetMarseyPatches();

        if (patches.Count != 0)
        {
            // Connect patches to internal logger
            AssemblyFieldHandler.InitHelpers(patches);
        }

        // Execute patches
        Patcher.Patch(patches);
    }

    private void Afterparty()
    {
        // TODO: Test if GameAssemblies.ClientInitialized works here
        while (!GameAssemblies.ClientInitialized()) // Wait until EntryPoint is just about to start
        {
            Thread.Sleep(125);
        }

        // If preclusion is triggered - close the game bruh
        if (Preclusion.State)
            Preclusion.Fire();

        // Apply content-related backports
        MarseyPortMan.PatchBackports(true);

        // Post-Load hidesey methods
        Hidesey.Cleanup();
    }
}
