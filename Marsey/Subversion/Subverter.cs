using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Marsey.Subversion;


/// <summary>
/// Manages patches/addons based on the Subverter patch
/// </summary>
public static class Subverter
{
    public const string MarserializerFile = "subversion.marsey";

    public static void AddSubvert(SubverterPatch patch)
    {
        List<SubverterPatch> list = PatchListManager.GetPatchList<SubverterPatch>();
        
        string assemblypath = patch.Asmpath;
        
        if (Subverse.CheckSubverterDuplicate(patch)) return;

        if (list.Any(p => p.Asmpath == assemblypath)) return;

        PatchListManager.AddPatchToList(patch);
    }

    public static List<SubverterPatch> GetSubverterPatches() => PatchListManager.GetPatchList<SubverterPatch>();
}

public class SubverterPatch : IPatch
{
    public string Asmpath { get; set; }
    public Assembly Asm { get; set; }
    public string Name { get; set; } 
    public string Desc { get; set; }
    
    public bool Enabled { get; set; } = false;
    
    public SubverterPatch(string asmpath, Assembly asm, string name, string desc)
    {
        this.Asmpath = asmpath;
        this.Name = name;
        this.Desc = desc;
        this.Asm = asm;
    }
}