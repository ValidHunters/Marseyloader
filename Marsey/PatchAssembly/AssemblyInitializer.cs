using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Marsey.Patches;
using Marsey.Stealthsey;
using Marsey.Subversion;
using Marsey.Misc;

namespace Marsey.PatchAssembly;

/// <summary>
/// Initializes a given assembly, validates its structure, and adds it to the list of patch assemblies.
/// </summary>
public static class AssemblyInitializer
{
    private static readonly Dictionary<string, Func<Assembly, string, string, bool, IPatch>> PatchFactory =
        new Dictionary<string, Func<Assembly, string, string, bool, IPatch>>
        {
            { "MarseyPatch", (assembly, name, description, preloadField) => new MarseyPatch(assembly.Location, assembly, name, description, preloadField) },
            { "SubverterPatch", (assembly, name, description, preloadField) => new SubverterPatch(assembly.Location, assembly, name, description) }
        };

    public static void Initialize(Assembly assembly)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));

        Type? dataType = GetDataType(assembly);
        if (dataType == null) return;

        ProcessDataType(assembly, dataType);
    }

    private static Type? GetDataType(Assembly assembly)
    {
        // Check for each patch type name defined in the PatchFactory dictionary
        Type? patchType = null;
        foreach (string patchTypeName in PatchFactory.Keys)
        {
            Type? type = assembly.GetType(patchTypeName);
            
            if (type == null) continue;
            
            // Discard patch if it has multiple data types
            if (patchType != null)
            {
                MarseyLogger.Log(MarseyLogger.LogType.FATL, $"{assembly.GetName().Name} contains multiple patch types!");
                return null;
            }
            
            patchType = type;
        }

        // Patch found, bail
        if (patchType != null) 
            return patchType;
        
        // If no patch type was found, log an error
        MarseyLogger.Log(MarseyLogger.LogType.FATL, $"{assembly.GetName().Name} does not contain any recognized patch types!");
        return null;

    }
    
    private static void ProcessDataType(Assembly assembly, Type dataType)
    {
        string typeName = dataType.Name;
        bool preloadField = AssemblyFieldHandler.DeterminePreload(dataType);
        
        switch (typeName)
        {
            case "MarseyPatch" when AssemblyFieldHandler.DetermineIgnore(dataType):
                MarseyLogger.Log(MarseyLogger.LogType.DEBG,
                    $"{assembly.GetName().Name} is ignoring fields, not assigning");
                return;
            case "MarseyPatch":
            {
                List<FieldInfo>? targets = AssemblyFieldHandler.GetPatchAssemblyFields(dataType);
                if (targets == null)
                {
                    MarseyLogger.Log(MarseyLogger.LogType.FATL,
                        $"Couldn't get assembly fields on {assembly.GetName().Name}.");
                    return;
                }

                if (preloadField)
                {
                    FieldInfo target = targets[0]; // Robust.Client
                    AssemblyFieldHandler.SetPreloadTarget(target);
                }
                else
                {
                    AssemblyFieldHandler.SetAssemblyTargets(targets);
                }

                break;
            }
        }

        AssemblyFieldHandler.GetFields(dataType, out string name, out string description);
        TryCreateAddPatch(assembly, dataType, name, description, preloadField);
    }

    private static void TryCreateAddPatch(Assembly assembly, MemberInfo? dataType, string name, string description, bool preloadField)
    {
        if (dataType == null) return;

        // Check if its even valid
        if (!PatchFactory.TryGetValue(dataType.Name, out Func<Assembly, string, string, bool, IPatch>? createPatch)) return;
        
        Hidesey.HidePatch(assembly); // Conceal assembly from the game
        
        IPatch patch = createPatch(assembly, name, description, preloadField);
        PatchListManager.AddPatchToList(patch);
    }
}