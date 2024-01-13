using System.Reflection;
using HarmonyLib;
using Marsey.Misc;
// ReSharper disable MemberCanBePrivate.Global

namespace Marsey.Handbreak;

public static class Helpers
{
    public static Type? TypeFromQualifiedName(string FQTN)
    {
        return AccessTools.TypeByName(FQTN);
    }

    public static MethodInfo? GetMethod(string FullyQualifiedTypeName, string MethodName, Type[]? parameters = null)
    {
        Type? t = TypeFromQualifiedName(FullyQualifiedTypeName);

        if (t != null) return GetMethod(t, MethodName, parameters);
        
        MarseyLogger.Log(MarseyLogger.LogType.ERRO, $"{FullyQualifiedTypeName} not found.");
        return null;
    }

    /// <summary>
    /// A type can be hidden by hidesey, therefore we need to tell AccessTools about it
    /// </summary>
    public static MethodInfo? GetMethod(Type type, string MethodName, Type[]? parameters = null)
    {
        return AccessTools.Method(type, MethodName, parameters);
    }
    
    /// <summary>
    /// Patches a method from the given pointers
    /// </summary>
    /// <param name="targetType">Class the target method is contained in</param>
    /// <param name="methodName">Target method name</param>
    /// <param name="patchType">Class the patch method is contained in</param>
    /// <param name="patchedMethodName">Patch method name</param>
    /// <param name="patchingType">What type of patch is used</param>
    /// <param name="targetMethodParameters">Parameters used by the target method</param>
    /// <param name="patchMethodParameters">Parameters used by the patch method</param>
    public static void PatchMethod(Type? targetType, string methodName, Type? patchType, string patchedMethodName, HarmonyPatchType patchingType, Type[]? targetMethodParameters = null, Type[]? patchMethodParameters = null)
    {
        if (targetType == null || patchType == null)
        {
            throw new HandBreakException($"Passed type is null. Target: {targetType}, patch: {patchType}");
        }
        
        MethodInfo? targetMethod = GetMethod(targetType, methodName, targetMethodParameters);
        if (targetMethod == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, "Handbreak", $"{methodName} method not found, not patching.");
            return;
        }

        MethodInfo? patchMethod = GetMethod(patchType, patchedMethodName, patchMethodParameters);
        if (patchMethod == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, "Handbreak", $"{patchedMethodName} method not found, not patching.");
            return;
        }

        Manual.Patch(targetMethod, patchMethod, patchingType);
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Handbreak", $"{patchingType.ToString()}: Patched {methodName} with {patchedMethodName}.");
    }
}