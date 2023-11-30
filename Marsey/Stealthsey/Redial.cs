using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;
using Marsey.Handbrake;
using MonoMod.Utils;

namespace Marsey.Stealthsey;

/// <summary>
/// Manages AssemblyLoad event delegates
/// </summary>
public static class Redial
{
    private static Delegate[]? _phonebook;
    
    /// <summary>
    /// Disables AssemblyLoad Callbacks in CurrentDomain
    /// </summary>
    public static void Disable()
    {
        MethodInfo prefix =
            typeof(HideseyPatches)
                .GetMethod("Skip", BindingFlags.Public | BindingFlags.Static)!;
        
        _phonebook = FillPhonebook();

        if (_phonebook == null) return;
        
        foreach (Delegate dial in _phonebook)
        {
            Manual.Prefix(dial.Method, prefix); // skip
        }
    }

    /// <summary>
    /// Unpatches AssemblyLoad callbacks
    /// </summary>
    public static void Enable()
    {
        if (_phonebook == null) return;
        
        foreach (Delegate dial in _phonebook)
        {
            Manual.Unpatch(dial.Method, HarmonyPatchType.Prefix);
        }
    }

    /// <summary>
    /// Returns a list of AssemblyLoad delegates
    /// </summary>
    private static Delegate[]? FillPhonebook()
    {
        FieldInfo? fInfo = typeof(AssemblyLoadContext).GetField("AssemblyLoad", BindingFlags.Static | BindingFlags.NonPublic);

        MulticastDelegate? eventDelegate = (MulticastDelegate?)fInfo?.GetValue(AppDomain.CurrentDomain);

        if (eventDelegate == null) return null; // If event delegate is null we don't even have one
        
        Delegate[] delegates = eventDelegate.GetInvocationList();

        if (delegates != _phonebook) MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"Redialing {delegates.Length} delegates.");

        return delegates;
    }
}