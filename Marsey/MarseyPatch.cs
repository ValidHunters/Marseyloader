using System.Reflection;

namespace Marsey;

/// <summary>
/// Generic class for a patch accepted by MarseyPatcher.
/// </summary>
public class MarseyPatch
{
    public Assembly Asm { get; set; } // Assembly containing the patch
    public string Name { get; set; } // Patch's name
    public string Desc { get; set; } // Patch's description
    public bool Enabled { get; set; } // Is the patch enabled or not.

    public MarseyPatch(Assembly asm, string name, string desc)
    {
        this.Asm = asm;
        this.Name = name;
        this.Desc = desc;
        this.Enabled = false;
    }
}
