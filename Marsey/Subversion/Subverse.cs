using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Marsey.Config;
using Marsey.GameAssembly;
using Marsey.Stealthsey;
using Marsey.Misc;
using Marsey.Stealthsey.Reflection;

namespace Marsey.Subversion;

/// <summary>
/// Manages the Subverter helper patch
/// https://github.com/Subversionary/Subverter
/// </summary>
public static class Subverse
{
    public static string SubverterFile = "Subverter.dll";
    private static SubverterPatch? _subverter = null;

    /// <summary>
    /// Initializes the subverter library.
    /// </summary>
    /// <returns>True if the library was initialized successfully.</returns>
    public static bool InitSubverter()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), SubverterFile);
        FileHandler.LoadExactAssembly(path);
    
        List<SubverterPatch> patches = Subverter.GetSubverterPatches();
        SubverterPatch? subverterPatch = patches.FirstOrDefault(p => p.Name == "Subverter");

        if (subverterPatch == null) return false;
        
        AssignSubverter(subverterPatch);
        SetHidesey();
        patches.Clear();
        
        return true;
    }

    private static void SetHidesey()
    {
        if (_subverter == null) return;
        
        Type? subverterPatchType = _subverter.Asm.GetType("SubverterPatch");
        MethodInfo? hideMethod = typeof(Subverter).GetMethod("Hide", BindingFlags.NonPublic | BindingFlags.Static);
        FieldInfo? hideDelegateField = subverterPatchType?.GetField("hideDelegate", BindingFlags.Public | BindingFlags.Static);

        if (subverterPatchType == null || hideMethod == null || hideDelegateField == null)
        {
            List<string> missingComps = new List<string>();
            if (subverterPatchType == null) missingComps.Add("subverterPatchType");
            if (hideMethod == null) missingComps.Add("hideMethod");
            if (hideDelegateField == null) missingComps.Add("hideDelegateField");

            string missingCompStr = string.Join(", ", missingComps);

            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Failed to connect patch to subverter. Missing components: {missingCompStr}.");
            return;
        }
        
        try
        {
            Delegate logDelegate = Delegate.CreateDelegate(hideDelegateField.FieldType, hideMethod);
            hideDelegateField.SetValue(null, logDelegate);
        }
        catch (Exception e)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Failed to to assign hide delegate: {e.Message}");
        }
    }


    /// <summary>
    /// Enables subverter if any of the of the subverter patches are enabled
    /// Used by the launcher to determine if it should load subversions
    /// </summary>
    /// <remarks>If MarseyHide is set to unconditional - defaults to false</remarks>
    public static void CheckEnabled()
    {
        List<SubverterPatch> patches = Subverter.GetSubverterPatches();

        if (patches.Any(p => p.Enabled))
        {
            MarseyVars.Subverter = true;
            return;
        }

        MarseyVars.Subverter = false;
    }

    /// <summary>
    /// Check if a patch is loaded from the same place subverter is
    /// </summary>
    /// <returns></returns>
    public static bool CheckSubverterDuplicate(SubverterPatch subverter)
    {
        if (CheckSubverterPresent())
            return CheckSubverterDuplicate(subverter.Asm);

        return false;
    }
    
    public static bool CheckSubverterDuplicate(Assembly assembly)
    {
        return assembly == _subverter?.Asm;
    }

    /// <summary>
    /// Check if subverter is already defined.
    /// Used by the launcher in the plugins/patches tab.
    /// </summary>
    public static bool CheckSubverterPresent()
    {
        return _subverter != null;
    }
    
    /// <summary>
    /// Patches subverter ahead of everything else
    /// This is done as we attach to the assembly loading function
    /// </summary>
    public static void PatchSubverter()
    {
        if (_subverter != null) GamePatcher.Patch(new List<SubverterPatch>() { _subverter });
    }
    
    private static void AssignSubverter(SubverterPatch subverter)
    {
        _subverter = subverter;
    }
}