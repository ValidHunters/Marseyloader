using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using Marsey.Preloader;
using Marsey.Stealthsey;
using Marsey.Subversion;

namespace Marsey;

/// <summary>
/// Entrypoint for patching
/// </summary>
public class MarseyPatcher
{
    public MarseyPatcher(Assembly? robClientAssembly)
    {
        if (robClientAssembly == null) throw new Exception("Robust.Client was null.");
        
        // Initialize FieldHandler
        AssemblyFieldHandler.Init(robClientAssembly);
        
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
        PreloadManager.Preload();

        // Initialize subverter if enabled and present
        if (MarseyVars.Subverter && Subverse.InitSubverter())
        {
            // Side-load custom code
            Subverse.PatchSubverter();
        }
        
        // Manage game assemblies
        GameAssemblyManager.GetGameAssemblies(out Assembly? clientAss, out Assembly? robustSharedAss, out Assembly? clientSharedAss);
        AssemblyFieldHandler.SetAssemblies(clientAss, robustSharedAss, clientSharedAss);

        // Prepare marseypatches
        FileHandler.LoadAssemblies(marserializer: true, filename: PatchListManager.MarserializerFile);
        List<MarseyPatch>? patches = PatchListManager.GetPatchList<MarseyPatch>();

        if (patches == null) return;

        // Connect patches to internal logger
        AssemblyFieldHandler.InitLogger(patches);

        // Execute patches
        GamePatcher.Patch(patches);
    }
}
