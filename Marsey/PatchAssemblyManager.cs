using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Marsey.Subversion;

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

    private static readonly List<MarseyPatch> _patchAssemblies = new List<MarseyPatch>();

    /// <summary>
    /// Initializes a given assembly, validates its structure, and adds it to the list of patch assemblies
    /// </summary>
    /// <param name="assembly">The assembly to initialize</param>
    /// <param name="subverter">Is the initialized assembly a subverter</param>
    /// <remarks>Function returns if neither MarseyPatch nor SubverterPatch can be found in the assembly</remarks>
    public static void InitAssembly(Assembly assembly, bool subverter = false)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        
        Type? DataType = null;
        Type? MarseyType = assembly.GetType("MarseyPatch");
        Type? SubverterType = assembly.GetType("SubverterPatch");

        if (MarseyType != null && SubverterType != null)
        {
            Utility.Log(Utility.LogType.FATL, $"{assembly.GetName().Name} is both a marseypatch and a subverter!");
            return;
        }

        if (MarseyType != null)
        {
            DataType = MarseyType;
            bool ignoreField = false;
            List<FieldInfo>? targets = GetPatchAssemblyFields(DataType);
            FieldInfo? ignoreFieldInfo = DataType.GetField("ignoreFields");
            
            if (ignoreFieldInfo != null)
                ignoreField = ignoreFieldInfo.GetValue(null) is bool;
            
            if (ignoreField)
                Utility.Log(Utility.LogType.DEBG, $"{assembly.GetName().Name} MarseyPatch is ignoring fields, not assigning");
            else if (targets != null && ignoreField != true)
                SetAssemblyTargets(targets);
            else
            {
                Utility.Log(Utility.LogType.FATL, $"Couldn't get assembly fields on {assembly.GetName().Name}.");
                return;
            }
        }

        // MarseyPatch takes precedence over Subverter, for now
        if (subverter)
            DataType = SubverterType;

        if (DataType == null)
        {
            Utility.Log(Utility.LogType.FATL, MarseyType != null 
                ? $"{Path.GetFileName(assembly.Location)}: MarseyPatch in subverter folder" 
                : SubverterType != null ? $"{Path.GetFileName(assembly.Location)}: SubverterPatch in Marsey folder" 
                : $"{assembly.GetName().Name} had no supported data type. Is it namespaced?");
            return;
        }


        GetFields(DataType, out string name, out string description);
        MarseyPatch patch = new MarseyPatch(assembly.Location, assembly, name, description);

        if (!subverter)
            AddToList(patch);
        else
            Subverter.AddSubvert(patch);
    }


    /// <summary>
    /// Initializes logger class in patches that have it.
    /// Executed only by the loader.
    /// MarseyLogger example can be found in the BasePatch MarseyPatch example.
    /// </summary>
    public static void InitLogger(List<MarseyPatch> patches)
    {
        foreach (MarseyPatch patch in patches)
        {
            Assembly assembly = patch.Asm;

            // Check for a logger class
            Type? marseyLoggerType = assembly.GetType("MarseyLogger");

            if (marseyLoggerType != null)
                Utility.SetupLogger(assembly);
            else
                Utility.Log(Utility.LogType.DEBG, $"{assembly.GetName().Name} has no MarseyLogger class");
        }
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
        if (FileHandler.GetPatches(new []{"Marsey"}).Count != _patchAssemblies.Count)
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

    /// <summary>
    /// Obtains patch metadata like name and description
    /// </summary>
    /// <param name="marseyPatchType">Patch data type</param>
    /// <param name="name">Returned name</param>
    /// <param name="description">Returned description</param>
    /// <remarks>If name or description cannot be found - "Unknown" is returned</remarks>
    public static void GetFields(Type? marseyPatchType, out string name, out string description)
    {
        FieldInfo? nameField = marseyPatchType?.GetField("Name");
        FieldInfo? descriptionField = marseyPatchType?.GetField("Description");

        name = nameField != null && nameField.GetValue(null) is string nameValue ? nameValue : "Unknown";
        description = descriptionField != null && descriptionField.GetValue(null) is string descriptionValue ? descriptionValue : "Unknown";
    }

    /// <summary>
    /// Adds to patch list if none present
    /// </summary>
    /// <param name="patch">MarseyPatch object</param>
    public static void AddToList(MarseyPatch patch)
    {
        Assembly assembly = patch.Asm;

        if (_patchAssemblies.Any(p => p.Asm == assembly)) return;

        _patchAssemblies.Add(patch);
    }
    
    /// <summary>
    /// Returns a either a MarseyPatch list or a Subverter depending if the bool is true
    /// </summary>
    /// <param name="subverter">Return a subverter list, false by default</param>
    /// <returns></returns>
    public static List<MarseyPatch> GetPatchList(bool subverter = false)
    {
        var patches = subverter ? Subverter.GetSubverterPatches() : _patchAssemblies;
        return patches;
    }
}

