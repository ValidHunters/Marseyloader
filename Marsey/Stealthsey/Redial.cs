using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;
using Marsey.Handbreak;
using Marsey.Misc;
using Marsey.Stealthsey.Reflection;

namespace Marsey.Stealthsey;

/// <summary>
/// Manages AssemblyLoad event delegates
/// Not to be confused with RedialAPI
/// </summary>
public static class Redial
{
    private static readonly string EventFieldName = "AssemblyLoad";
    private static Delegate[] _phonebook = Array.Empty<Delegate>();

    private static Delegate[] Phonebook
    {
        get => _phonebook;
        set => _phonebook = value ?? Array.Empty<Delegate>();
    }

    /// <summary>
    /// Disables AssemblyLoad Callbacks in CurrentDomain
    /// </summary>
    [HideLevelRequirement(HideLevel.Normal)]
    public static void Disable()
    {
        MethodInfo prefix = typeof(HideseyPatches)
            .GetMethod("Skip", BindingFlags.Public | BindingFlags.Static)!;

        Phonebook = FillPhonebook();

        foreach (Delegate dial in Phonebook)
        {
            Manual.Patch(dial.Method, prefix, HarmonyPatchType.Prefix);
        }
    }

    /// <summary>
    /// Enables caught AssemblyLoad callbacks
    /// </summary>
    public static void Enable()
    {
        foreach (Delegate dial in Phonebook)
        {
            Manual.Unpatch(dial.Method, HarmonyPatchType.Prefix);
        }
    }

    /// <summary>
    /// Returns a list of AssemblyLoad delegates
    /// </summary>
    private static Delegate[] FillPhonebook()
    {
        MulticastDelegate? eventDelegate = GetEventDelegate();

        if (eventDelegate == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, "No AssemblyLoad delegates found.");
            return Array.Empty<Delegate>();
        }

        Delegate[] delegates = eventDelegate.GetInvocationList();

        if (!delegates.SequenceEqual(Phonebook))
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Redialing {delegates.Length} delegates.");
        }

        return delegates;
    }

    private static MulticastDelegate? GetEventDelegate()
    {
        FieldInfo? fInfo = typeof(AssemblyLoadContext).GetField(EventFieldName, BindingFlags.Static | BindingFlags.NonPublic);
        return (MulticastDelegate?)fInfo?.GetValue(AppDomain.CurrentDomain);
    }
}
