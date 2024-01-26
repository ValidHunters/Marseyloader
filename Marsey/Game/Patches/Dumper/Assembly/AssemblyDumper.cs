using HarmonyLib;
using Marsey.Config;
using Marsey.Handbreak;
using Marsey.Misc;

namespace Marsey.Game.Patches;

/// <summary>
/// Deprecated by accident, ResourceDumper does this and better
/// <see cref="ResourceDumper"/>
/// </summary>
public class AssemblyDumper
{
    public void Patch()
    {
        Type? ModLoader = Helpers.TypeFromQualifiedName("Robust.Shared.ContentPack.ModLoader");

        if (ModLoader == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, "ModLoader is null.");
            return;
        }
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Dumper", "Patching TLM");
        Helpers.PatchMethod(ModLoader, 
            "TryLoadModules", 
            typeof(AsmDumpPatches), 
            "TLMPostfix", 
            HarmonyPatchType.Postfix);
        
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Dumper", "Patching LGA");
        Helpers.PatchMethod(ModLoader, 
            "LoadGameAssembly",
            typeof(AsmDumpPatches),
            "LGAPrefix",
            HarmonyPatchType.Prefix, 
            new[] { typeof(Stream), typeof(Stream), typeof(bool) });
    }
}