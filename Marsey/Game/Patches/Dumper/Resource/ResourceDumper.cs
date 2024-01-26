using System.Reflection;
using HarmonyLib;
using Marsey.Handbreak;
using Marsey.Misc;

namespace Marsey.Game.Patches;

/// <summary>
/// By complete accident this dumps everything
/// </summary>
public class ResourceDumper
{
    public static MethodInfo? CFRMi; 
    public void Patch()
    {
        Type? ProtoMan = Helpers.TypeFromQualifiedName("Robust.Shared.ContentPack.ResourceManager");
        Type? ResPath = Helpers.TypeFromQualifiedName("Robust.Shared.Utility.ResPath");

        CFRMi = AccessTools.Method(ProtoMan, "ContentFileRead", new[] { ResPath });

        if (ProtoMan == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, "PrototypeManager is null.");
            return;
        }
        
        if (ResPath == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, "ResPath is null.");
            return;
        }
        
        Helpers.PatchMethod(
            targetType: ProtoMan,
            targetMethodName: "ContentFindFiles",
            patchType: typeof(ResDumpPatches),
            patchMethodName: "PostfixCFF",
            patchingType: HarmonyPatchType.Postfix,
            new Type[]{ ResPath }
        );
    }
}