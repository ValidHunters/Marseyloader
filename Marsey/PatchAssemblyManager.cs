using System;
using System.Collections.Generic;
using System.Reflection;

namespace Marsey;

/// <summary>
/// Initializes, validates and manages patch assemblies.
/// </summary>
public abstract class PatchAssemblyManager
{
    private static Assembly? _robustAss;
    private static Assembly? _clientAss;
    private static Assembly? _robustSharedAss;
    private static Assembly? _clientSharedAss;

    private static List<MarseyPatch> _patchAssemblies = new List<MarseyPatch>();

    /// <summary>
    /// Initializes a given assembly, validates its structure, and adds it to the list of patch assemblies
    /// </summary>
    /// <param name="assembly">The assembly to initialize</param>
    /// <exception cref="Exception">Excepts if MarseyPatch is not present in the assembly</exception>
    /// <exception cref="Exception">Excepts if "TargetAssembly" is present in the assembly MarseyPatch type"</exception>
    /// <exception cref="Exception">Excepts if GetPatchAssemblyFields returns null</exception>
    public static void InitAssembly(Assembly assembly)
    {
        Type marseyPatchType = assembly.GetType("MarseyPatch") ?? throw new PatchAssemblyException("Loaded assembly does not have MarseyPatch type.");

        if (marseyPatchType.GetField("TargetAssembly") != null) throw new PatchAssemblyException($"{assembly.FullName} cannot be loaded because it uses an outdated patch!");

        List<FieldInfo> targets = GetPatchAssemblyFields(marseyPatchType) ?? throw new PatchAssemblyException($"Couldn't get assembly fields on {assembly.FullName}.");

        SetAssemblyTargets(targets);

        FieldInfo? nameField = marseyPatchType.GetField("Name");
        FieldInfo? descriptionField = marseyPatchType.GetField("Description");

        string name = nameField != null && nameField.GetValue(null) is string nameValue ? nameValue : "Unknown";
        string description = descriptionField != null && descriptionField.GetValue(null) is string descriptionValue ? descriptionValue : "Unknown";

        var patch = new MarseyPatch(assembly, name, description);

        foreach (MarseyPatch p in _patchAssemblies)
        {
            if (p.Asm == assembly)
                return;
        }

        _patchAssemblies.Add(patch);

    }

    /// <summary>
    /// Obtains fields for each of the game's assemblies.
    /// Returns null if any of the fields is null.
    /// </summary>
    /// <returns>List of fields of assemblies within a MarseyPatch</returns>
    /// <exception cref="Nullable">Returns null if any field in MarseyPatch is missing</exception>
    private static List<FieldInfo>? GetPatchAssemblyFields(Type marseyPatchType)
    {
        var fieldNames = new[] { "RobustClient", "RobustShared", "ContentClient", "ContentShared" };
        var targets = new List<FieldInfo>();

        foreach (var fieldName in fieldNames)
        {
            var field = marseyPatchType.GetField(fieldName);
            if (field == null) return null; // Not all fields could be caught, no point proceeding.
            targets.Add(field);
        }

        return targets;
    }

    /// <summary>
    /// Sets the assembly target in the patch assembly.
    /// In order: Robust.Client, Robust.Shared, Content.Client, Content.Shared
    /// </summary>
    /// <param name="targets">Array of assemblies from the MarseyPatch class</param>
    private static void SetAssemblyTargets(List<FieldInfo> targets)
    {
        targets[0].SetValue(null, _robustAss);
        targets[1].SetValue(null,_robustSharedAss);
        targets[2].SetValue(null,_clientAss);
        targets[3].SetValue(null,_clientSharedAss);
    }

    /// <summary>
    /// Checks if the amount of patches in folder equals the amount of patches in list.
    /// If not - resets the list.
    /// </summary>
    public static void RecheckPatches()
    {
        if (FileHandler.GetPatches(new []{"Marsey"}).Length != _patchAssemblies.Count)
            _patchAssemblies.Clear();
    }

    /// <summary>
    /// Sets assemblies to fields in class
    /// </summary>
    public static void SetAssemblies(Assembly? robustAss, Assembly? clientAss, Assembly? robustSharedAss, Assembly? clientSharedAss)
    {
        _robustAss = robustAss;
        _clientAss = clientAss;
        _robustSharedAss = robustSharedAss;
        _clientSharedAss = clientSharedAss;
    }

    /// <returns>Patch list</returns>
    public static List<MarseyPatch> GetPatchList() => _patchAssemblies;
}

