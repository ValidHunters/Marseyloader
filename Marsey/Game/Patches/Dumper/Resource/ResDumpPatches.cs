using System.Diagnostics.CodeAnalysis;
using Marsey.Misc;

namespace Marsey.Game.Patches;

internal static class ResDumpPatches
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static void PostfixCFF(ref object __instance, ref dynamic __result)
    {
        if (ResourceDumper.CFRMi == null) return; // If FileReader handle is not available, return

        foreach (dynamic file in __result)
        {
            string canonPath = file.CanonPath;
            string fixedCanonPath = canonPath.StartsWith($"/") ? canonPath[1..] : canonPath;
            string fullpath = Path.Combine(Dumper.path, fixedCanonPath);

            FileHandler.CreateDir(fullpath);

            object? CFRout = ResourceDumper.CFRMi.Invoke(__instance, new object?[] {file});

            if (CFRout is not MemoryStream stream) return;

            FileHandler.SaveToFile(fullpath, stream);
        }
    }
}