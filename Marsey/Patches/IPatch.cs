using System.Reflection;
using Newtonsoft.Json;

namespace Marsey.Patches;

public interface IPatch
{
    public string Asmpath { get; set; } // DLL file path
    [JsonIgnore] public Assembly Asm { get; set; } // Assembly containing the patch
    [JsonIgnore] public string Name { get; set; } // Patch's name
    [JsonIgnore] public string Desc { get; set; } // Patch's description
    [JsonIgnore] public MethodInfo? Entry { get; set; } // Method to execute on patch, if available
    [JsonIgnore] public bool Enabled { get; set; } // Is the patch enabled or not.
}