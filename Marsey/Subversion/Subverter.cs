using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Marsey.Subversion;


/// <summary>
/// Manages patches/addons based on the Subverter patch
/// </summary>
public static class Subverter
{
    private static List<SubverterPatch> _subverterPatches = new List<SubverterPatch>();
    public const string MarserializerFile = "subversion.marsey";

    public static void AddSubvert(SubverterPatch patch)
    {
        string assemblypath = patch.Asmpath;
        
        if (Subverse.CheckSubverterDuplicate(patch)) return;

        if (_subverterPatches.Any(p => p.Asmpath == assemblypath)) return;
        
        PatchListManager.IncrementPatchCount();
        
        _subverterPatches.Add(patch);
    }

    public static void ResetList() => _subverterPatches = new List<SubverterPatch>();

    public static List<SubverterPatch> GetSubverterPatches() => _subverterPatches;
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