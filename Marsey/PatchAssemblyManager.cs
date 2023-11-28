using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Marsey.Subversion;

namespace Marsey;

/// <summary>
/// Initializes a given assembly, validates its structure, and adds it to the list of patch assemblies
/// </summary>
public static class AssemblyInitializer
{
    /// <param name="assembly">The assembly to initialize</param>
    /// <param name="subverter">Is the initialized assembly a subverter</param>
    /// <remarks>Function returns if neither MarseyPatch nor SubverterPatch can be found in the assembly</remarks>
    public static void Initialize(Assembly assembly, bool subverter = false)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        
        Type? DataType = null;
        Type? MarseyType = assembly.GetType("MarseyPatch");
        Type? SubverterType = assembly.GetType("SubverterPatch");

        if (MarseyType != null && SubverterType != null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"{assembly.GetName().Name} is both a marseypatch and a subverter!");
            return;
        }

        if (MarseyType != null)
        {
            DataType = MarseyType;
            bool ignoreField = false;
            List<FieldInfo>? targets = AssemblyFieldHandler.GetPatchAssemblyFields(DataType);
            FieldInfo? ignoreFieldInfo = DataType.GetField("ignoreFields");
            
            if (ignoreFieldInfo != null)
                ignoreField = ignoreFieldInfo.GetValue(null) is bool;
            
            if (ignoreField)
                MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{assembly.GetName().Name} MarseyPatch is ignoring fields, not assigning");
            else if (targets != null && ignoreField != true)
                AssemblyFieldHandler.SetAssemblyTargets(targets);
            else
            {
                MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Couldn't get assembly fields on {assembly.GetName().Name}.");
                return;
            }
        }
        
        // Prefer subverter over marseypatch if enabled
        if (subverter)
            DataType = SubverterType;

        if (DataType == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, MarseyType != null 
                ? $"{Path.GetFileName(assembly.Location)}: MarseyPatch in subverter folder" 
                : SubverterType != null ? $"{Path.GetFileName(assembly.Location)}: SubverterPatch in Marsey folder" 
                : $"{assembly.GetName().Name} had no supported data type. Is it namespaced?");
            return;
        }


        AssemblyFieldHandler.GetFields(DataType, out string name, out string description);
        MarseyPatch patch = new MarseyPatch(assembly.Location, assembly, name, description);

        if (!subverter)
            PatchListManager.AddToList(patch);
        else
            Subverter.AddSubvert(patch);
    }
}

/// <summary>
/// Manages patch lists
/// </summary>
public static class PatchListManager
{
    private static readonly List<MarseyPatch> _patchAssemblies = new List<MarseyPatch>();

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
    /// Adds to patch list if none present
    /// </summary>
    /// <param name="patch">MarseyPatch object</param>
    public static void AddToList(MarseyPatch patch)
    {
        string assemblypath = patch.Asmpath;

        if (_patchAssemblies.Any(p => p.Asmpath == assemblypath)) return;

        _patchAssemblies.Add(patch);
    }
    
    /// <summary>
    /// Returns a either a MarseyPatch list or a Subverter depending if the bool is true
    /// </summary>
    /// <param name="subverter">Return a subverter list, false by default</param>
    /// <returns></returns>
    public static List<MarseyPatch> GetPatchList(bool subverter = false)
    {
        List<MarseyPatch> patches = subverter ? Subverter.GetSubverterPatches() : _patchAssemblies;
        return patches;
    }
}

/// <summary>
/// Validates and manages patch assembly fields
/// </summary>
public static class AssemblyFieldHandler
{
    private static Assembly? _robustAss;
    private static Assembly? _clientAss;
    private static Assembly? _robustSharedAss;
    private static Assembly? _clientSharedAss;

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
                SetupLogger(assembly);
            else
                MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{assembly.GetName().Name} has no MarseyLogger class");
        }
    }

    /// <summary>
    /// Obtains fields for each of the game's assemblies.
    /// Returns null if any of the fields is null.
    /// </summary>
    /// <returns>List of fields of assemblies within a MarseyPatch</returns>
    /// <exception cref="Nullable">Returns null if any field in MarseyPatch is missing</exception>
    public static List<FieldInfo>? GetPatchAssemblyFields(Type marseyPatchType)
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
    public static void SetAssemblyTargets(List<FieldInfo> targets)
    {
        targets[0].SetValue(null, _robustAss);
        targets[1].SetValue(null,_robustSharedAss);
        targets[2].SetValue(null,_clientAss);
        targets[3].SetValue(null,_clientSharedAss);
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
    /// Sets patch delegate to MarseyLogger::Log(AssemblyName, string)
    /// Executed only by the Loader.
    /// </summary>
    /// <see cref="InitLogger"/>
    /// <param name="patch">Assembly from MarseyPatch</param>
    private static void SetupLogger(Assembly patch)
    {
        Type marseyLoggerType = patch.GetType("MarseyLogger")!;

        Type logDelegateType = marseyLoggerType.GetNestedType("Forward", BindingFlags.Public)!;

        MethodInfo logMethod = typeof(MarseyLogger).GetMethod("Log", new []{typeof(AssemblyName), typeof(string)})!;

        Delegate logDelegate = Delegate.CreateDelegate(logDelegateType, null, logMethod);

        marseyLoggerType.GetField("logDelegate", BindingFlags.Public | BindingFlags.Static)!.SetValue(null, logDelegate);
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
}