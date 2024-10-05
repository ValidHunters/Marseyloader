using System.Reflection;
using HarmonyLib;
using Marsey.Config;
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
    public static MethodInfo? GetMethod(Type? type, string MethodName, Type[]? parameters = null)
    {
        return AccessTools.Method(type, MethodName, parameters);
    }

    /// <summary>
    /// Patches a method from the given pointers
    /// </summary>
    /// <param name="targetType">Class the target method is contained in</param>
    /// <param name="targetMethodName">Target method name</param>
    /// <param name="patchType">Class the patch method is contained in</param>
    /// <param name="patchMethodName">Patch method name</param>
    /// <param name="patchingType">What type of patch is used</param>
    /// <param name="targetMethodParameters">Parameters used by the target method</param>
    /// <param name="patchMethodParameters">Parameters used by the patch method</param>
    public static void PatchMethod(Type? targetType, string targetMethodName, Type? patchType, string patchMethodName, HarmonyPatchType patchingType, Type[]? targetMethodParameters = null, Type[]? patchMethodParameters = null)
    {
        ValidateTypes(targetType, patchType);

        MethodInfo? targetMethod = GetAndValidateMethod(targetType, targetMethodName, targetMethodParameters, "target");
        MethodInfo? patchMethod = GetAndValidateMethod(patchType, patchMethodName, patchMethodParameters, "patch");

        if (Manual.Patch(targetMethod, patchMethod, patchingType))
            LogPatchSuccess(patchingType, targetMethodName, patchMethodName);
    }

    #region Generics
    public static void PatchGenericMethod(Type? targetType, string targetMethodName, Type? patchType, string patchMethodName, Type returnType, HarmonyPatchType patchingType)
    {
        ValidateTypes(targetType, patchType);

        MethodInfo? targetMethod = GetAndValidateMethod(targetType, targetMethodName, null, "target");
        MethodInfo? patchMethod = GetAndValidateMethod(patchType, patchMethodName, null, "patch");

        MethodInfo? genericMethod = MakeGenericMethod(patchMethod, returnType);

        if (Manual.Patch(targetMethod, genericMethod, patchingType))
            LogPatchSuccess(patchingType, targetMethodName, patchMethodName);
    }

    public static void PatchGenericMethod(MethodInfo? target, Type targetReturnType, MethodInfo? patch, Type patchReturnType, HarmonyPatchType patchType)
    {
        target = MakeGenericMethod(target, targetReturnType);
        patch = MakeGenericMethod(patch, patchReturnType);

        if (Manual.Patch(target, patch, patchType))
            LogPatchSuccess(patchType, target!.Name, patch!.Name);
    }

    public static void PatchGenericMethod(MethodInfo? target, MethodInfo? patch, Type patchReturnType, HarmonyPatchType patchType)
    {
        patch = MakeGenericMethod(patch, patchReturnType);

        if (Manual.Patch(target, patch, patchType))
            LogPatchSuccess(patchType, target!.Name, patch!.Name);
    }

    public static void PatchGenericMethod(MethodInfo target, Type targetReturnType, MethodInfo? patch, HarmonyPatchType patchType)
    {
        target = target.MakeGenericMethod([]);

        if (Manual.Patch(target, patch, patchType))
            LogPatchSuccess(patchType, target.Name, patch!.Name);
    }

    public static void PatchGenericMethod(MethodInfo? target, MethodInfo? patch, HarmonyPatchType patchType)
    {
        if (Manual.Patch(target, patch, patchType))
            LogPatchSuccess(patchType, target!.Name, patch!.Name);
    }

    #endregion

    private static void ValidateTypes(Type? targetType, Type? patchType)
    {
        if (targetType == null || patchType == null)
            throw new HandBreakException($"Passed type is null. Target: {targetType}, patch: {patchType}");
    }

    private static void ValidateMethods(MethodInfo? target, MethodInfo? patch)
    {
        if (target == null || patch == null)
            throw new HandBreakException($"Passed type is null. Target: {target}, patch: {patch}");
    }

    private static MethodInfo? GetAndValidateMethod(Type? type, string methodName, Type[]? methodParameters, string methodType)
    {
        MethodInfo? method = GetMethod(type, methodName, methodParameters);
        if (method == null)
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, "Handbreak", $"{methodName} {methodType} method not found, not patching.");
        return method;
    }

    private static MethodInfo? MakeGenericMethod(MethodInfo? method, Type returnType)
    {
        if (method != null) return method.MakeGenericMethod(returnType);

        MarseyLogger.Log(MarseyLogger.LogType.ERRO, "Handbreak", $"Error making generic method");
        return null;
    }

    private static void LogPatchSuccess(HarmonyPatchType patchingType, string targetMethodName, string patchMethodName)
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Handbreak", $"{patchingType}: Patched {targetMethodName} with {patchMethodName}.");
    }
}
