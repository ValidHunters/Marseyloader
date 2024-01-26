using System.Diagnostics;
using Marsey.Misc;

namespace Marsey.Game.Patches;

static class ResDumpPatches
{
    // Okay so this is so schizo I will comment every line from memory
    public static void PostfixCFF(ref object __instance, ref dynamic __result)
    {
        if (ResourceDumper.CFRMi == null) return; // Do we have a handle on FileReader?
        
        foreach (dynamic file in __result) // Since we're patching FindFiles were receiving the full list of resources
        {
            string canonPath = file.CanonPath; // We get the entire path
            if (canonPath.StartsWith("/")) // And remove the first slash else were writing to root and fuck that
            {
                canonPath = canonPath.Substring(1);
            }

            string fullpath = Path.Combine(Dumper.path, canonPath);
    
            // Create directory if it doesn't exist
            string? directoryName = Path.GetDirectoryName(fullpath);
            if (directoryName != null && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            
            // This is where we get the actual file - CFR stands for ContentFileRead
            // Since its not a static method we use __instance as the called, which is very convenient, thanks PJB!
            object? CFRout = ResourceDumper.CFRMi.Invoke(__instance, new object?[] {file});

            if (CFRout is not MemoryStream stream) return; // Cast as MemoryStream (CFROut is MemoryStream as is but whatever 

            // Save the thing
            using FileStream st = new FileStream(fullpath, FileMode.Create, FileAccess.Write);
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Saving to {fullpath}");
            stream.WriteTo(st);
        }
    }
}