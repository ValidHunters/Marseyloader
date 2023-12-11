using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Marsey.Stealthsey;
using Marsey.Subversion;
namespace Marsey;

/// <summary>
/// Initializes a given assembly, validates its structure, and adds it to the list of patch assemblies.
/// </summary>
public static class AssemblyInitializer
{
    /// <summary>
    /// Initializes the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to initialize.</param>
    public static void Initialize(Assembly assembly)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));

        Type? dataType = GetDataType(assembly);
        if (dataType == null) return;

        ProcessDataType(assembly, dataType);
    }

    private static Type? GetDataType(Assembly assembly)
    {
        Type? marseyType = assembly.GetType("MarseyPatch");
        Type? subverterType = assembly.GetType("SubverterPatch");

        if (marseyType == null || subverterType == null) return marseyType ?? subverterType;
        MarseyLogger.Log(MarseyLogger.LogType.FATL, $"{assembly.GetName().Name} is both a marseypatch and a subverter!");
        return null;

    }

    private static void ProcessDataType(Assembly assembly, Type dataType)
    {
        bool preloadField = AssemblyFieldHandler.DeterminePreload(dataType);

        if (AssemblyFieldHandler.DetermineIgnore(dataType))
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{assembly.GetName().Name} is ignoring fields, not assigning");
            return;
        }

        List<FieldInfo>? targets = AssemblyFieldHandler.GetPatchAssemblyFields(dataType);
        if (targets == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Couldn't get assembly fields on {assembly.GetName().Name}.");
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

        AssemblyFieldHandler.GetFields(dataType, out string name, out string description);
        TryCreateAddPatch(assembly, dataType, name, description, preloadField);
    }

    private static void TryCreateAddPatch(Assembly assembly, MemberInfo? dataType, string name, string description, bool preloadField)
    {
        if (dataType == null) return;
        
        switch (dataType.Name)
        {
            case "MarseyPatch":
            {
                MarseyPatch patch = new MarseyPatch(assembly.Location, assembly, name, description, preloadField);
                PatchListManager.AddPatchToList(patch);
                break;
            }
            case "SubverterPatch":
            {
                SubverterPatch patch = new SubverterPatch(assembly.Location, assembly, name, description);
                Subverter.AddSubvert(patch);
                break;
            }
        }
    }
}


/// <summary>
/// Manages patch lists
/// </summary>
/// <summary>
/// Manages patch lists.
/// </summary>
public static class PatchListManager
{
    private static readonly List<IPatch> _patches = new List<IPatch>();
    public const string MarserializerFile = "patches.marsey";

    /// <summary>
    /// Checks if the amount of patches in folder equals the amount of patches in list.
    /// If not - resets the lists.
    /// </summary>
    public static void RecheckPatches()
    {
        int folderPatchCount = FileHandler.GetPatches(new[] { MarseyVars.MarseyPatchFolder }).Count;
        if (folderPatchCount != _patches.OfType<MarseyPatch>().Count() + _patches.OfType<SubverterPatch>().Count())
        {
            ResetList();
        }
    }

    /// <summary>
    /// Adds a patch to the list if it is not already present.
    /// </summary>
    /// <param name="patch">The patch to add.</param>
    public static void AddPatchToList(IPatch patch)
    {
        if (_patches.Any(p => p.Asmpath == patch.Asmpath)) return;

        _patches.Add(patch);
    }

    /// <summary>
    /// Returns the list of patches of a specific type.
    /// </summary>
    /// <returns>A list of patches of type T.</returns>
    public static List<T> GetPatchList<T>() where T : IPatch
    {
        return _patches.OfType<T>().ToList();
    }

    /// <summary>
    /// Clears the list of patches.
    /// </summary>
    public static void ResetList()
    {
        _patches.Clear();
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
    /// Sets Robust.Client assembly in class
    /// </summary>
    public static void Init(Assembly? RobustClient)
    {
        _robustAss = RobustClient;
    }

    /// <summary>
    /// Sets other assemblies to fields in class
    /// </summary>
    public static void SetAssemblies(Assembly? clientAss, Assembly? robustSharedAss, Assembly? clientSharedAss)
    {
        _clientAss = clientAss;
        _robustSharedAss = robustSharedAss;
        _clientSharedAss = clientSharedAss;
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
        string[] fieldNames = new[] { "RobustClient", "RobustShared", "ContentClient", "ContentShared" };
        List<FieldInfo> targets = new List<FieldInfo>();

        foreach (string fieldName in fieldNames)
        {
            var field = marseyPatchType.GetField(fieldName);
            if (field == null) return null; // Not all fields could be caught, no point proceeding.
            targets.Add(field);
        }

        return targets;
    }

    /// <summary>
    /// Is the patch asking the loader to ignore setting fields
    /// </summary>
    /// <param name="DataType">Patch class type</param>
    public static bool DetermineIgnore(Type DataType)
    {
        FieldInfo? ignoreFieldInfo = DataType.GetField("ignoreFields");

        if (ignoreFieldInfo != null)
            return ignoreFieldInfo.GetValue(null) is bool;

        return false;
    }

    /// <summary>
    /// Is the patch asking to be loaded before the game?
    /// </summary>
    /// <param name="DataType">Patch class type</param>
    public static bool DeterminePreload(Type DataType)
    {
        FieldInfo? preloadFieldInfo = DataType.GetField("preload");

        if (preloadFieldInfo != null)
            return preloadFieldInfo.GetValue(null) is bool;

        return false;
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
    /// Set patch field to use Robust.Client
    /// </summary>
    /// <param name="target">MarseyPatch.RobustClient from a patch assembly</param>
    public static void SetPreloadTarget(FieldInfo target)
    {
        target.SetValue(null, _robustAss);
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

    /// <summary>
    /// Checks if GameAssemblyManager has finished capturing assemblies
    /// </summary>
    /// <returns>True if any of the assemblies are filled</returns>
    public static bool ClientInitialized()
    {
        return _clientAss != null || _robustSharedAss != null || _clientSharedAss != null;
    }

}
