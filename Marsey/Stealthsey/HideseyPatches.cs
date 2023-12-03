using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

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
}