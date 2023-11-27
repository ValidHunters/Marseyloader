using System.Reflection;
using System.Text.Json.Serialization;

namespace Marsey;

/// <summary>
/// Generic class for a patch accepted by MarseyPatcher.
/// </summary>
public class MarseyPatch
{
    public string Asmpath { get; set; } // DLL file path
    [JsonIgnore] public Assembly Asm { get; set; } // Assembly containing the patch
    [JsonIgnore] public string Name { get; set; } // Patch's name
    [JsonIgnore] public string Desc { get; set; } // Patch's description
    [JsonIgnore] public bool Enabled { get; set; } = false; // Is the patch enabled or not.
    
    public MarseyPatch(string asmpath, Assembly asm, string name, string desc)
    {
        this.Asmpath = asmpath;
        this.Name = name;
        this.Desc = desc;
        this.Asm = asm;
    }
}
