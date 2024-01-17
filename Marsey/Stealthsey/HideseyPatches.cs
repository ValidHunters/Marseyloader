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
        switch (__result)
        {
            case Assembly[] assemblies:
                __result = (T)(object)Hidesey.LyingDomain(assemblies);
                break;
            case IEnumerable<Assembly> assemblyEnumerable:
                __result = (T)(object)Hidesey.LyingContext(assemblyEnumerable);
                break;
            case IEnumerable<AssemblyLoadContext> assemblyLoadContextEnumerable:
                __result = (T)(object)Hidesey.LyingManifest(assemblyLoadContextEnumerable);
                break;
            case AssemblyName[] assemblyNames:
                __result = (T)(object)Hidesey.LyingReference(assemblyNames);
                break;
            case Type[] types:
                __result = (T)(object)Hidesey.LyingTyper(types);
                break;
            default:
                throw new InvalidOperationException("Unsupported type for LiePatch");
        }
    }


    /// <summary>
    /// This patch skips function execution
    /// </summary>
    public static bool Skip() => false;
    
    /// <summary>
    /// Prefix patch that checks if MarseyHide matches or above the attributed HideLevelRequirement
    /// </summary>
    public static bool LevelCheck(MethodBase __originalMethod)
    {
        
        string fullMethodName = $"{__originalMethod.DeclaringType?.FullName}::{__originalMethod.Name}";
        string parameters = string.Join(", ", __originalMethod.GetParameters().Select(p => p.ParameterType.Name));
        fullMethodName += $"({parameters})";
        
        // Check if the method has a HideLevelRequirement attribute and compare the required level with the current MarseyHide level.
        if (__originalMethod.GetCustomAttributes(typeof(HideLevelRequirement), false).FirstOrDefault() is 
                HideLevelRequirement hideLevelRequirement 
                && MarseyConf.MarseyHide < hideLevelRequirement.Level)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, 
                $"Not executing {fullMethodName} due to lower MarseyHide level. " +
                        $"Required: {hideLevelRequirement.Level}, Current: {MarseyConf.MarseyHide}");
            return false;
        }
        
        // Check if the method has a HideLevelRestriction attribute and ensure the current MarseyHide level is below the threshold.
        if (__originalMethod.GetCustomAttributes(typeof(HideLevelRestriction), false).FirstOrDefault() is 
                HideLevelRestriction hideLevelRestriction && 
                MarseyConf.MarseyHide >= hideLevelRestriction.MaxLevel)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, 
                $"Not executing {fullMethodName} due to equal or above MarseyHide level. " +
                $"Threshold: {hideLevelRestriction.MaxLevel}, Current: {MarseyConf.MarseyHide}");
            return false;
        }

        return true;
    }
}