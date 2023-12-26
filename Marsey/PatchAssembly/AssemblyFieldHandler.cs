using System;
using System.Collections.Generic;
using System.Reflection;
using Marsey.GameAssembly;
using Marsey.Patches;
using Marsey.Misc;

namespace Marsey.PatchAssembly;

/// <summary>
/// Validates and manages patch assembly fields
/// </summary>
public static class AssemblyFieldHandler
{
    /// <summary>
    /// Initialize helper classes in patches
    /// </summary>
    public static void InitHelpers(IEnumerable<IPatch> patches)
    {
        InitLoggerAndEntry(patches);
    }
    
    /// <summary>
    /// Initializes logger and entry classes in patches that have them.
    /// Executed only by the loader.
    /// </summary>
    private static void InitLoggerAndEntry(IEnumerable<IPatch> patches)
    {
        foreach (IPatch patch in patches)
        {
            Assembly assembly = patch.Asm;

            // Check for a logger class
            Type? marseyLoggerType = assembly.GetType("MarseyLogger");
            if (marseyLoggerType != null)
                SetupLogger(assembly);
            else
                MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{assembly.GetName().Name} has no MarseyLogger class");

            // Check for an entry class
            Type? marseyEntryType = assembly.GetType("MarseyEntry");
            if (marseyEntryType != null)
            {
                MethodInfo? entry = GetEntry(assembly, marseyEntryType);
                if (entry != null) 
                    patch.Entry = entry;
            }
            else
            {
                MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{assembly.GetName().Name} has no MarseyEntry class");
            }
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
            FieldInfo? field = marseyPatchType.GetField(fieldName);
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
    /// If marseypatch has a bool called "preload" - check it.
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
        targets[0].SetValue(null, GameAssemblies.RobustClient);
        targets[1].SetValue(null,GameAssemblies.RobustShared);
        targets[2].SetValue(null,GameAssemblies.ContentClient);
        targets[3].SetValue(null,GameAssemblies.ContentShared);
    }

    /// <summary>
    /// Set patch field to use Robust.Client
    /// </summary>
    /// <param name="target">MarseyPatch.RobustClient from a patch assembly</param>
    public static void SetPreloadTarget(FieldInfo target)
    {
        target.SetValue(null, GameAssemblies.RobustClient);
    }

    /// <summary>
    /// Sets patch delegate to MarseyLogger::Log(AssemblyName, string)
    /// Executed only by the Loader.
    /// </summary>
    /// <param name="patch">Assembly from MarseyPatch</param>
    private static void SetupLogger(Assembly patch)
    {
        Type? marseyLoggerType = patch.GetType("MarseyLogger");
        MethodInfo? logMethod = typeof(MarseyLogger).GetMethod("Log", new[] { typeof(AssemblyName), typeof(string) });
        FieldInfo? logDelegateField = marseyLoggerType?.GetField("logDelegate", BindingFlags.Public | BindingFlags.Static);

        if (marseyLoggerType == null || logMethod == null || logDelegateField == null)
        {
            List<string> missingComps = new List<string>();
            if (marseyLoggerType == null) missingComps.Add("MarseyLoggerType");
            if (logMethod == null) missingComps.Add("LogMethod");
            if (logDelegateField == null) missingComps.Add("LogDelegateField");

            string missingCompStr = string.Join(", ", missingComps);

            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Failed to connect patch to marseylogger. Missing components: {missingCompStr}.");
            return;
        }
        
        try
        {
            Delegate logDelegate = Delegate.CreateDelegate(logDelegateField.FieldType, logMethod);
            logDelegateField.SetValue(null, logDelegate);
        }
        catch (Exception e)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Failed to to assign logger delegate: {e.Message}");
        }
    }

    /// <summary>
    /// Get methodhandle for the entrypoint function in patch
    /// </summary>
    private static MethodInfo? GetEntry(Assembly patch, Type entryType)
    {
        MethodInfo? entry = entryType.GetMethod("Entry", BindingFlags.Public | BindingFlags.Static);
        return entry;
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
