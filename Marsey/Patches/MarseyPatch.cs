using System.Reflection;

namespace Marsey.Patches;

/// <summary>
/// This class contains the data about a patch (called a Marsey), that is later used the loader to alter the game's functionality.
/// </summary>
public class MarseyPatch : IPatch
{
    public string Asmpath { get; set; } // DLL file path
    public Assembly Asm { get; set; } // Assembly containing the patch
    public string Name { get; set; } // Patch's name
    public string Desc { get; set; } // Patch's description
    public MethodInfo? Entry { get; set; } // Method to execute on patch, if available
    public bool Preload { get; set; } = false; // Is the patch getting loaded before game assemblies
    public bool Enabled { get; set; } // Is the patch enabled or not.

    public MarseyPatch(string asmpath, Assembly asm, string name, string desc, bool preload = false)
    {
        this.Asmpath = asmpath;
        this.Name = name;
        this.Desc = desc;
        this.Asm = asm;
        this.Preload = preload;
    }

    public override bool Equals(object obj)
    {
        if (obj is MarseyPatch other)
        {
            return this.Name == other.Name && this.Desc == other.Desc;
        }
        return false;
    }
}
