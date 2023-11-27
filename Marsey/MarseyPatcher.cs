using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using Marsey.Subversion;

namespace Marsey;

/// <summary>
/// Entrypoint for patching
/// </summary>
public class MarseyPatcher
{
    /// <summary>
    /// Boots up the patcher
    ///
    /// Executed by the loader.
    /// </summary>
    /// <param name="robClientAssembly">Robust.Client assembly as *loaded* by the *loader*</param>
    /// <exception cref="Exception">Excepts if Robust.Client assembly is null</exception>
    public static void Boot(Assembly? robClientAssembly)
    {
        if (robClientAssembly == null) throw new Exception("Robust.Client was null.");
        
        // Initialize loader
        Utility.SetupFlags();
        GameAssemblyManager.Init(new Harmony(MarseyVars.Identifier));
        
        // Preload subverter if enabled and present
        if (MarseyVars.Subverter && Subverse.InitSubverter())
        {
            // Side-load custom code
            Subverse.PatchSubverter();
        }

        // Manage game assemblies
        GameAssemblyManager.GetGameAssemblies(out var clientAss, out var robustSharedAss, out var clientSharedAss);
        PatchAssemblyManager.SetAssemblies(robClientAssembly, clientAss, robustSharedAss, clientSharedAss);

        // Prepare patches
        FileHandler.LoadAssemblies(new []{ MarseyVars.MarseyPatchFolder }, marserializer: true);
        List<MarseyPatch> patches = PatchAssemblyManager.GetPatchList(false);

        // Connect to internal logger
        PatchAssemblyManager.InitLogger(patches);

        // Execute patches
        GameAssemblyManager.PatchProc(patches);
    }
}
