using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Marsey.Config;
using Marsey.Misc;
using Marsey.Stealthsey.Reflection;

namespace Marsey.Stealthsey;

/// <summary>
/// Manual patches used with Hidesey
/// Not based off MarseyPatch or SubverterPatch
/// </summary>
public static class HideseyPatches
{
    public static void Lie<T>(ref T __result)
    {
        __result = __result switch
        {
            Assembly[] assemblies => (T)(object)Hidesey.LyingDomain(assemblies),
            IEnumerable<Assembly> assemblyEnumerable => (T)(object)Hidesey.LyingContext(assemblyEnumerable),
            IEnumerable<AssemblyLoadContext> assemblyLoadContextEnumerable => (T)(object)Hidesey.LyingManifest(
                assemblyLoadContextEnumerable),
            AssemblyName[] assemblyNames => (T)(object)Hidesey.LyingReference(assemblyNames),
            Type[] types => (T)(object)Hidesey.LyingTyper(types),
            _ => throw new InvalidOperationException("Unsupported type for LiePatch")
        };
    }

    /// <summary>
    /// This patch skips function execution
    /// </summary>
    public static bool Skip() => false;

    public static bool SkipPatchless() => !MarseyConf.Patchless;

    /// <summary>
    /// Prefix patch that checks if MarseyHide matches or above the attributed HideLevelRequirement
    /// </summary>
    public static bool LevelCheck(MethodBase __originalMethod)
    {

        string fullMethodName = $"{__originalMethod.DeclaringType?.FullName}::{__originalMethod.Name}";
        string parameters = string.Join(", ", __originalMethod.GetParameters().Select(p => p.ParameterType.Name));
        fullMethodName += $"({parameters})";

        object[] customAttributes = __originalMethod.GetCustomAttributes(false);

        // Check if the method has a HideLevelRequirement attribute and compare the required level with the current MarseyHide level.
        HideLevelRequirement? hideLevelRequirement = customAttributes.OfType<HideLevelRequirement>().FirstOrDefault();
        if (hideLevelRequirement != null && MarseyConf.MarseyHide < hideLevelRequirement.Level)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG,
                $"Not executing {fullMethodName} due to lower MarseyHide level. " +
                $"Required: {hideLevelRequirement.Level}, Current: {MarseyConf.MarseyHide}");
            return false;
        }

        // Check if the method has a HideLevelRestriction attribute and ensure the current MarseyHide level is below the threshold.
        HideLevelRestriction? hideLevelRestriction = customAttributes.OfType<HideLevelRestriction>().FirstOrDefault();
        if (hideLevelRestriction != null && MarseyConf.MarseyHide >= hideLevelRestriction.MaxLevel)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG,
                $"Not executing {fullMethodName} due to equal or above MarseyHide level. " +
                $"Threshold: {hideLevelRestriction.MaxLevel}, Current: {MarseyConf.MarseyHide}");
            return false;
        }

        return true;
    }
}
