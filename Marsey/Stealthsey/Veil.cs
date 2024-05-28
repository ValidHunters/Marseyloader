using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Marsey.Handbreak;
using Marsey.Misc;

namespace Marsey.Stealthsey;

/// <summary>
/// Hide extra assemblies from IReflectionManager
/// </summary>
internal static class Veil
{
    private static List<string?> HiddenAssemblies = [];

    internal static void Patch()
    {
        Type? CRM = Helpers.TypeFromQualifiedName("Robust.Shared.Reflection.ReflectionManager");
        if (CRM == null) return;

        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Veil", "Patching.");

        MethodInfo AsmGetter = AccessTools.PropertyGetter(CRM, "Assemblies");
        MethodInfo Prefix = AccessTools.Method(typeof(Veil), "Prefix");
        Manual.Patch(AsmGetter, Prefix, HarmonyPatchType.Prefix);
    }

    [UsedImplicitly]
    private static bool Prefix(ref IReadOnlyList<Assembly> __result, object __instance)
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

    public static void Hide(Assembly asm)
    {
        string? name = asm.GetName().Name;
        if (name != null) HiddenAssemblies.Add(name);
    }
}
