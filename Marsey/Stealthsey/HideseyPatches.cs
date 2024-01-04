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
    /// <summary>
    /// This is a postfix patch which swaps an assembly list with a less honest one
    /// </summary>
    public static void LieLoader(ref Assembly[] __result)
    {
        __result = Hidesey.LyingDomain(__result);
    }
    
    public static void LieContext(ref IEnumerable<Assembly> __result)
    {
        __result = Hidesey.LyingContext(__result);
    }

    public static void LieManifest(ref IEnumerable<AssemblyLoadContext> __result)
    {
        __result = Hidesey.LyingManifest(__result);
    }

    /// <summary>
    /// Same but with referenced assemblies
    /// </summary>
    public static void LieReference(ref AssemblyName[] __result)
    {
        __result = Hidesey.LyingReference(__result);
    }

    public static void LieTyper(ref Type[] __result)
    {
        __result = Hidesey.LyingTyper(__result);
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
        
        if (__originalMethod.GetCustomAttributes(typeof(HideLevelRequirement), false).FirstOrDefault() is HideLevelRequirement hideLevelRequirement 
            && MarseyConf.MarseyHide < hideLevelRequirement.Level)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, 
                $"Not executing {fullMethodName} due to lower MarseyHide level. " +
                        $"Required: {hideLevelRequirement.Level}, Current: {MarseyConf.MarseyHide}");
            return false;
        }
        
        if (__originalMethod.GetCustomAttributes(typeof(HideLevelRestriction), false).FirstOrDefault() is HideLevelRestriction hideLevelRestriction 
            && MarseyConf.MarseyHide >= hideLevelRestriction.MaxLevel)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, 
                $"Not executing {fullMethodName} due to equal or above MarseyHide level. " +
                $"Threshold: {hideLevelRestriction.MaxLevel}, Current: {MarseyConf.MarseyHide}");
            return false;
        }

        return true;
    }
}