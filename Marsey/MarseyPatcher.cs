using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;

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

        Utility.SetupFlags();

        GameAssemblyManager.Init(new Harmony(MarseyVars.Identifier));

        GameAssemblyManager.GetGameAssemblies(out var clientAss, out var robustSharedAss, out var clientSharedAss);

        PatchAssemblyManager.SetAssemblies(robClientAssembly, clientAss, robustSharedAss, clientSharedAss);

        FileHandler.LoadAssemblies(new []{"Marsey", "Enabled"});

        PatchAssemblyManager.InitLogger();

        GameAssemblyManager.PatchProc();
    }
}
