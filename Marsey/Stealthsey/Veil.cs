using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Marsey.Handbreak;
using Marsey.Misc;

namespace Marsey.Stealthsey;

/// <summary>
/// Hide subversions from IReflectionManager
/// </summary>
internal static class Veil
{
    private static List<string?> HiddenAssemblies = [];
    private static IEnumerable<Type> _veilCache = [];

    internal static void Patch()
    {
        Type? CRM = Helpers.TypeFromQualifiedName("Robust.Shared.Reflection.ReflectionManager");
        if (CRM == null) return;

        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Veil", "Patching.");

        MethodInfo AsmGetter = AccessTools.PropertyGetter(CRM, "Assemblies");
        MethodInfo AsmPrefix = AccessTools.Method(typeof(Veil), "AsmPrefix");
        Manual.Patch(AsmGetter, AsmPrefix, HarmonyPatchType.Prefix);

        MethodInfo FindAllTypes = AccessTools.Method(CRM, "FindAllTypes");
        MethodInfo GetAllChildren = AccessTools.Method(CRM, "GetAllChildren", new[] { typeof(Type), typeof(bool) });
        MethodInfo FindTypesWithAttribute = AccessTools.Method(CRM, "FindTypesWithAttribute", new[] { typeof(Type) });
        MethodInfo TypePost = AccessTools.Method(typeof(Veil), "TypePost");

        Manual.Patch(FindAllTypes, TypePost, HarmonyPatchType.Postfix);
        Manual.Patch(GetAllChildren, TypePost, HarmonyPatchType.Postfix);
        Manual.Patch(FindTypesWithAttribute, TypePost, HarmonyPatchType.Postfix);
    }

    [UsedImplicitly]
    private static bool AsmPrefix(ref IReadOnlyList<Assembly> __result, object __instance)
    {
        List<Assembly>? originalAssemblies = Traverse.Create(__instance).Field("assemblies").GetValue<List<Assembly>>();
        if (originalAssemblies == null)
        {
            __result = new List<Assembly>().AsReadOnly();
            return false;
        }

        // Filter out assemblies whose names are in HiddenAssemblies
        List<Assembly> veiledAssemblies = originalAssemblies
            .Where(asm =>
            {
                string? value = asm.GetName().Name;
                return value != null && !HiddenAssemblies.Contains(value);
            })
            .ToList();

        MarseyLogger.Log(MarseyLogger.LogType.TRCE, "Veil", $"Hidden {HiddenAssemblies.Count} assemblies.");
        // Return the filtered list as a read-only list
        __result = veiledAssemblies.AsReadOnly();
        return false;
    }

    [UsedImplicitly]
    private static void TypePost(ref IEnumerable<Type> __result)
    {
        if (!Hidesey.FromContent()) return;

        MarseyLogger.Log(MarseyLogger.LogType.TRCE, "Passed fromcontent check with negative?");

        if (Hidesey.caching && _veilCache.Any())
        {
            __result = _veilCache;
            return;
        }

        IEnumerable<Type> hiddenTypes = Facade.GetTypes();
        _veilCache = __result.Except(hiddenTypes).AsEnumerable();
        __result = _veilCache;
    }

    public static void Hide(Assembly asm)
    {
        string? name = asm.GetName().Name;
        if (name != null) HiddenAssemblies.Add(name);
    }
}
