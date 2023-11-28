using System.Reflection;
using System.Text.Json.Serialization;

namespace Marsey;

public interface IPatch
{
    public string Asmpath { get; set; } // DLL file path
    [JsonIgnore] public Assembly Asm { get; set; } // Assembly containing the patch
    [JsonIgnore] public string Name { get; set; } // Patch's name
    [JsonIgnore] public string Desc { get; set; } // Patch's description
    [JsonIgnore] public bool Enabled { get; set; } // Is the patch enabled or not.
}

/// <summary>
/// Generic class for a patch accepted by MarseyPatcher.
/// </summary>
public class MarseyPatch : IPatch
{
    public string Asmpath { get; set; } // DLL file path
    public Assembly Asm { get; set; } // Assembly containing the patch
    public string Name { get; set; } // Patch's name
    public string Desc { get; set; } // Patch's description
    public bool Preload { get; set; } = false; // Is the patch getting loaded before game assemblies
    public bool Enabled { get; set; } = false; // Is the patch enabled or not.
    
    public MarseyPatch(string asmpath, Assembly asm, string name, string desc, bool preload = false)
    {
        this.Asmpath = asmpath;
        this.Name = name;
        this.Desc = desc;
        this.Asm = asm;
        this.Preload = preload;
    }
}
