using System.Reflection;
using HarmonyLib;
using Marsey.Misc;

namespace Marsey.Game.Resources.Reflection;

public static class ResourcePatches
{
    private static void SwapCFFRes(ref object __instance, ref dynamic __result)
    {
        if (ResourceTypes.ResPath == null) return;

        ConstructorInfo? RPTI = AccessTools.Constructor(ResourceTypes.ResPath, new[] { typeof(string) });
        
        List<string> filesToReplace = ResourceSwapper.ResourceFiles();
        List<dynamic> replacedFiles = new List<dynamic>();
        
        foreach (dynamic file in __result)
        {
            string canonpath = file.CanonPath;
            dynamic replacedFile = file;
            
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Checking {canonpath}");
            
            for (int j = 0; j < filesToReplace.Count; j++)
            {
                MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Checking if {filesToReplace[j]} ends with {canonpath}");
                if (!filesToReplace[j].EndsWith(canonpath)) continue;
                MarseyLogger.Log(MarseyLogger.LogType.INFO, "ResSwap", $"Found {canonpath}! Replacing!");
                replacedFile = RPTI.Invoke(new object?[]{ filesToReplace[j]});
                filesToReplace.RemoveAt(j);
                break;
            }
            
            replacedFiles.Add(replacedFile);
        }

        __result = replacedFiles;
    }
}