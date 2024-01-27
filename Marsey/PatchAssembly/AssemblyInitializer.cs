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
        ArgumentNullException.ThrowIfNull(assembly);

        Type? dataType = GetDataType(assembly);
        if (dataType == null) return;

        ProcessDataType(assembly, dataType);
    }

    private static Type? GetDataType(Assembly assembly)
    {
        List<Type?> patchTypes = PatchFactory.Keys.Select(assembly.GetType).Where(type => type != null).ToList();

        switch (patchTypes.Count)
        {
            case > 1:
                MarseyLogger.Log(MarseyLogger.LogType.FATL, $"{assembly.GetName().Name} contains multiple patch types!");
                return null;
            case 0:
                MarseyLogger.Log(MarseyLogger.LogType.FATL, $"{assembly.GetName().Name} does not contain any recognized patch types!");
                return null;
            default:
                return patchTypes.Single();
        }
    }
    
    private static void ProcessDataType(Assembly assembly, Type dataType)
    {
        string typeName = dataType.Name;
        string? assemblyName = assembly.GetName().Name;
        bool preload = AssemblyFieldHandler.DeterminePreload(dataType);
        bool ignore = AssemblyFieldHandler.DetermineIgnore(dataType);
        List<FieldInfo>? targets = null;

        if (typeName == "MarseyPatch")
        {
            targets = AssemblyFieldHandler.GetPatchAssemblyFields(dataType);
        }

        switch (typeName)
        {
            case "MarseyPatch" when ignore:
                MarseyLogger.Log(MarseyLogger.LogType.DEBG,
                    $"{assemblyName} is ignoring fields, not assigning");
                break;
            case "MarseyPatch":
                if (targets == null)
                {
                    MarseyLogger.Log(MarseyLogger.LogType.FATL,
                        $"Couldn't get assembly fields on {assemblyName}.");
                    return;
                }
                if (preload)
                {
                    FieldInfo client = targets[0]; // Robust.Client
                    FieldInfo shared = targets[1]; // Robust.Shared
                    AssemblyFieldHandler.SetEngineTargets(client, shared);
                }
                else
                {
                    AssemblyFieldHandler.SetAssemblyTargets(targets);
                }
                break;
        }

        // Retrieve additional fields such as name and description from the data type
        AssemblyFieldHandler.GetFields(dataType, out string name, out string description);
        // Attempt to create and add a patch to the assembly with the retrieved information
        TryCreateAddPatch(assembly, dataType, name, description, preload);
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