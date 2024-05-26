using System.Reflection;
using Marsey.Misc;
using Marsey.Stealthsey;

namespace Marsey.Subversion;

/// <summary>
///     Manages Hidesey patch helper class
/// </summary>
public static class Sedition
{
    private static void HideDelegate(Assembly asm)
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "Hiding");
        Hidesey.HidePatch(asm);
    }

    public static void InitHidesey(Assembly assembly, string? assemblyName)
    {
        Type? hideseyType = assembly.GetType("Hidesey");
        if (hideseyType == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{assemblyName} has no Hidesey class");
            return;
        }

        SetupHidesey(hideseyType);
    }

    private static void SetupHidesey(Type hidesey)
    {
        MethodInfo? stealthseyHide =
            typeof(Sedition).GetMethod("HideDelegate", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo? asmHideseyDelegate = hidesey.GetField("hideDelegate", BindingFlags.Public | BindingFlags.Static);

        if (asmHideseyDelegate == null || stealthseyHide == null)
        {
            MarseyLogger.Log(MarseyLogger.LogType.ERRO, $"Missing delegate fields on hidesey");
            return;
        }

        try
        {
            Delegate logDelegate = Delegate.CreateDelegate(asmHideseyDelegate.FieldType, stealthseyHide);
            asmHideseyDelegate.SetValue(null, logDelegate);
        }
        catch (Exception e)
        {
            MarseyLogger.Log(MarseyLogger.LogType.FATL, $"Failed to to assign logger delegate: {e.Message}");
        }
    }
}
