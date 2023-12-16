using System;
using System.Reflection;
using Marsey.Config;
using Marsey.PatchAssembly;
using Marsey.Stealthsey;

namespace Marsey.Misc;

public static class MarseyLogger
{
    public enum LogType
    {
        INFO,
        WARN,
        FATL,
        DEBG
    }

    /// <summary>
    /// Log function used by the loader
    /// </summary>
    /// <param name="logType">Log level</param>
    /// <param name="message">Log message</param>
    public static void Log(MarseyLogger.LogType logType, string message)
    {
        if (logType == MarseyLogger.LogType.DEBG && MarseyVars.DebugAllowed != true)
            return;

        SharedLog($"[{logType.ToString()}] {message}");
    }

    /// <summary>
    /// Log function used by patches
    /// </summary>
    /// <param name="asm">Assembly name of patch</param>
    /// <param name="message">Log message</param>
    /// <see cref="AssemblyFieldHandler.SetupLogger"/>
    public static void Log(AssemblyName asm, string message)
    {
        if (MarseyVars.PatchLogAllowed)
            SharedLog($"[{asm.Name}] {message}");
    }

    private static void SharedLog(string message)
    {
        if (MarseyVars.MarseyHide < HideLevel.Explicit)
            Console.WriteLine($"[{MarseyVars.MarseyLoggerPrefix}] {message}");
    }
}
public abstract class Utility
{
    /// <summary>
    /// Checks loader environment variables, sets relevant flags in MarseyVars
    ///
    /// Executed only by the loader.
    /// </summary>
    public static void SetupFlags()
    {
        MarseyVars.PatchLogAllowed = CheckEnv("MARSEY_LOG_PATCHES");
        MarseyVars.DebugAllowed = CheckEnv("MARSEY_LOADER_DEBUG");
        MarseyVars.ThrowOnFail = CheckEnv("MARSEY_THROW_FAIL");
        MarseyVars.Subverter = CheckEnv("MARSEY_SUBVERTER");
        MarseyVars.SeparateLogger = CheckEnv("MARSEY_SEPARATE_LOGGER");
    }

    private static bool CheckEnv(string envName)
    {
        string envVar = Environment.GetEnvironmentVariable(envName)!;
        Envsey.CleanFlag(envName);
        return !string.IsNullOrEmpty(envVar) && bool.Parse(envVar);
    }
}
