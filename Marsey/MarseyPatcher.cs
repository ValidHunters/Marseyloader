using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.GameAssembly;
using Marsey.GameAssembly.Patches;
using Marsey.PatchAssembly;
using Marsey.Patches;
using Marsey.Stealthsey;
using Marsey.Subversion;
using Marsey.Misc;

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
        Utility.SetupFlags();
        HarmonyManager.Init(new Harmony(MarseyVars.Identifier));
        
        // Hide the loader
        Hidesey.Initialize();

        Preload();
        
        // Tell the loader were done here, start the game
        _flag.Set();
    }

    // We might want to patch things before the loader has even a chance to execute anything
    public void Preload()
    {
        // Preload marseypatches, if available
        Marsyfier.Preload();
        
        // If set - Disable redialing and remote command execution
        Jammer.Patch();
        Blackhole.Patch();

        // Patch assembly loading methods, if enabled
        Dumper.Patch();
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

        // Prepare marseypatches
        FileHandler.LoadAssemblies(marserializer: true, filename: Marsyfier.MarserializerFile);
        List<MarseyPatch> patches = Marsyfier.GetMarseyPatches();
        
        if (patches.Count != 0)
        {
            // Connect patches to internal logger
            AssemblyFieldHandler.InitHelpers(patches);

            // Execute patches
            GamePatcher.Patch(patches);
        }
        
        Hidesey.Cleanup();
    }
}
