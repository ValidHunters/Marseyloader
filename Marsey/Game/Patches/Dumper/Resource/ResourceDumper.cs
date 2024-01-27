using System.Reflection;
using HarmonyLib;
using Marsey.Game.ResourcePack;
using Marsey.Game.ResourcePack.Reflection;
using Marsey.Handbreak;
using Marsey.Misc;

namespace Marsey.Game.Patches;

/// <summary>
/// By complete accident this dumps everything
/// </summary>
public static class ResourceDumper
{
    public static MethodInfo? CFRMi; 
    public static void Patch()
    {
        FileHandler.CheckRenameDirectory(Dumper.path); 
        
        if (ResourceTypes.ProtoMan == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, "PrototypeManager is null.");
            return;
        }
        
        if (ResourceTypes.ResPath == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, "ResPath is null.");
            return;
        }
        
        CFRMi = AccessTools.Method(ResourceTypes.ProtoMan, "ContentFileRead", new[] { ResourceTypes.ResPath });
        
        Helpers.PatchMethod(
            targetType: ResourceTypes.ProtoMan,
            targetMethodName: "ContentFindFiles",
            patchType: typeof(ResDumpPatches),
            patchMethodName: "PostfixCFF",
            patchingType: HarmonyPatchType.Postfix,
            new Type[]{ ResourceTypes.ResPath }
        );
    }
}