using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Marsey.Config;
using Marsey.GameAssembly;
using Marsey.PatchAssembly;
using Marsey.Patches;
using Marsey.Stealthsey;
using Marsey.Subversion;
using Marsey.Misc;
using Marsey.Stealthsey.Game;

namespace Marsey;

/// <summary>
/// Entrypoint for patching
/// </summary>
public class MarseyPatcher
{
    private static MarseyPatcher? _instance;
    
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
    
    public static void CreateInstance(Assembly? robClientAssembly)
    {
        if (_instance != null)
        {
            throw new Exception("Instance already created.");
        }
        
        _instance = new MarseyPatcher(robClientAssembly);
    }
    
    private MarseyPatcher(Assembly? robClientAssembly)
    {
        if (robClientAssembly == null) throw new Exception("Robust.Client was null.");
        
        // Initialize GameAssemblies
        GameAssemblies.Initialize(robClientAssembly);
        
        // Initialize loader
        Utility.SetupFlags();
        HarmonyManager.Init(new Harmony(MarseyVars.Identifier));
        
        // Hide the loader
        Hidesey.Initialize();
    }
    
    /// <summary>
    /// Boots up the patcher
    ///
    /// Executed by the loader.
    /// </summary>
    /// <exception cref="Exception">Excepts if Robust.Client assembly is null</exception>
    public void Boot()
    {
        // Preload marseypatches, if available
        Marsyfier.Preload();
        
        // Initialize subverter if enabled and present
        if (MarseyVars.Subverter && Subverse.InitSubverter())
        {
            // Side-load custom code
            Subverse.PatchSubverter();
        }

        // Manage game assemblies
        GameAssemblyManager.GetGameAssemblies(out Assembly? clientAss, out Assembly? robustSharedAss, out Assembly? clientSharedAss);
        GameAssemblies.AssignAssemblies(robustSharedAss, clientAss, clientSharedAss);
        
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
