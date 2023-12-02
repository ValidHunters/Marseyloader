using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Marsey.Stealthsey;
using Marsey.Subversion;
namespace Marsey;

/// <summary>
/// Initializes a given assembly, validates its structure, and adds it to the list of patch assemblies
/// </summary>
public static class AssemblyInitializer
{
    /// <param name="assembly">The assembly to initialize</param>
    /// <remarks>Function returns if neither MarseyPatch nor SubverterPatch can be found in the assembly</remarks>
    /// <remarks>Because patches were made generic this now loads anything from the Marsey folder</remarks>
    public static void Initialize(Assembly assembly)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));

        Type? DataType = null;
        Type? MarseyType = assembly.GetType("MarseyPatch");
        Type? SubverterType = assembly.GetType("SubverterPatch");

        bool preloadField = false;

        if (MarseyType != null && SubverterType != null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"{assembly.GetName().Name} is both a marseypatch and a subverter!");
            return;
        }

        // MarseyPatch logic
        if (MarseyType != null)
        {
            DataType = MarseyType;
            List<FieldInfo>? targets = AssemblyFieldHandler.GetPatchAssemblyFields(DataType);

            bool ignoreField = AssemblyFieldHandler.DetermineIgnore(DataType);
            preloadField = AssemblyFieldHandler.DeterminePreload(DataType);

            if (ignoreField)
                MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{assembly.GetName().Name} MarseyPatch is ignoring fields, not assigning");
            else if (preloadField && targets != null)
            {
                FieldInfo target = targets[0]; // Robust.Client
                AssemblyFieldHandler.SetPreloadTarget(target);
            }
            else if (targets != null && ignoreField != true)
                AssemblyFieldHandler.SetAssemblyTargets(targets);
            else
            {
                MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Couldn't get assembly fields on {assembly.GetName().Name}.");
                return;
            }
        }

        // Subverter logic
        if (SubverterType != null)
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

        if (MarseyVars.MarseyHide <= HideLevel.Duplicit)
            Hidesey.Hide(assembly); // Hide assembly

        if (SubverterType == null)
        {
            MarseyPatch patch = new MarseyPatch(assembly.Location, assembly, name, description, preloadField);
            PatchListManager.AddPatchToList(patch);
        }
        else
        {
            SubverterPatch patch = new SubverterPatch(assembly.Location, assembly, name, description);
            Subverter.AddSubvert(patch);
        }
    }
}

/// <summary>
/// Manages patch lists
/// </summary>
public static class PatchListManager
{
    private static readonly List<MarseyPatch> _patchAssemblies = new List<MarseyPatch>();
    private static int _patchcount = 0;
    public const string MarserializerFile = "patches.marsey";

    /// <summary>
    /// Checks if the amount of patches in folder equals the amount of patches in list.
    /// If not - resets the lists.
    /// </summary>
    public static void RecheckPatches()
    {
        if (FileHandler.GetPatches(new[] { MarseyVars.MarseyPatchFolder }).Count != _patchcount)
            return;

        ResetList();
        Subverter.ResetList();
    }

    public static void IncrementPatchCount()
    {
        _patchcount++;
    }

    /// <summary>
    /// Adds to patch list if none present
    /// </summary>
    /// <param name="patch">MarseyPatch object</param>
    public static void AddPatchToList<T>(T patch) where T : IPatch
    {
        string assemblypath = patch.Asmpath;
        List<T>? patchList;

        if (typeof(T) == typeof(MarseyPatch))
        {
            patchList = _patchAssemblies as List<T>;
        }
        else if (typeof(T) == typeof(SubverterPatch))
        {
            patchList = Subverter.GetSubverterPatches() as List<T>;
        }
        else
            throw new ArgumentException("Invalid patch type");

        if (patchList == null || patchList.Any(p => p.Asmpath == assemblypath)) return;

        IncrementPatchCount();

        patchList.Add(patch);
    }

    /// <summary>
    /// Returns a either a MarseyPatch list or a Subverter depending if the bool is true
    /// </summary>
    /// <returns>A list of patches</returns>
    public static List<T>? GetPatchList<T>() where T : IPatch
    {
        if (typeof(T) == typeof(MarseyPatch))
            return _patchAssemblies as List<T>;

        if (typeof(T) == typeof(SubverterPatch))
            return Subverter.GetSubverterPatches() as List<T>;

        throw new ArgumentException("Invalid patch type passed");
    }

    /// <summary>
    /// Clears list
    /// </summary>
    public static void ResetList()
    {
        _patchAssemblies.Clear();
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
